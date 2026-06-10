import uuid
import random
from django.conf import settings
from django.contrib.auth import get_user_model
from django.utils import timezone
from django.db import models
from rest_framework import generics, permissions, status, viewsets
from rest_framework.decorators import action
from rest_framework.response import Response
from rest_framework.views import APIView
from django.core.mail import send_mail

from .models import (
    Category, Event, Application, Notification,
    Review, Favorite, BonusTransaction, Promo, UserPromo, EmailOTP,
)
from .serializers import (
    RegisterSerializer, UserSerializer,
    CategorySerializer,
    EventListSerializer, EventDetailSerializer, EventCreateSerializer,
    ApplicationSerializer,
    NotificationSerializer,
    ReviewSerializer,
    FavoriteSerializer,
    BonusTransactionSerializer,
    PromoSerializer, PromoCreateSerializer, UserPromoSerializer,
)
from .permissions import IsOrganizer, IsEventOrganizer

User = get_user_model()


# ──────────────────────────── Auth ────────────────────────────

class RegisterView(generics.CreateAPIView):
    serializer_class = RegisterSerializer
    permission_classes = [permissions.AllowAny]

    def perform_create(self, serializer):
        user = serializer.save()
        # Отправляем код подтверждения email (не блокируем вход)
        code = ''.join(str(random.randint(0, 9)) for _ in range(6))
        expires_at = timezone.now() + timezone.timedelta(minutes=10)
        otp = EmailOTP.objects.create(
            user=user,
            code=code,
            purpose='verify_email',
            email=user.email,
            expires_at=expires_at,
        )
        from_email = getattr(settings, 'DEFAULT_FROM_EMAIL', 'noreply@example.com')
        subject = 'Код подтверждения регистрации'
        message = f'Ваш код подтверждения: {code}\n\nКод действителен 10 минут.'
        try:
            send_mail(subject, message, from_email, [user.email], fail_silently=True)
        except Exception:
            pass


from rest_framework_simplejwt.views import TokenObtainPairView
from rest_framework_simplejwt.serializers import TokenObtainPairSerializer

class CustomTokenSerializer(TokenObtainPairSerializer):
    def validate(self, attrs):
        username = attrs.get('username')
        
        # If input looks like email, find the actual username
        if username and '@' in username:
            try:
                user = User.objects.get(email__iexact=username)
                attrs['username'] = user.username
            except User.DoesNotExist:
                # Email not found, let parent handle the error
                pass

        return super().validate(attrs)

class LoginView(TokenObtainPairView):
    serializer_class = CustomTokenSerializer


class ProfileView(generics.RetrieveUpdateAPIView):
    serializer_class = UserSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_object(self):
        return self.request.user


class BecomeOrganizerView(APIView):
    permission_classes = [permissions.IsAuthenticated]

    def post(self, request):
        user = request.user
        user.is_organizer = True
        user.company_name = request.data.get('company_name', '')
        user.organizer_categories = request.data.get('organizer_categories', [])
        user.save()
        return Response(UserSerializer(user).data)


class LeaveOrganizerView(APIView):
    permission_classes = [permissions.IsAuthenticated]

    def post(self, request):
        user = request.user
        user.is_organizer = False
        user.company_name = ''
        user.organizer_categories = []
        user.save()
        return Response(UserSerializer(user).data)


# ──────────────────────────── Categories ──────────────────────

class CategoryViewSet(viewsets.ReadOnlyModelViewSet):
    queryset = Category.objects.all()
    serializer_class = CategorySerializer
    permission_classes = [permissions.AllowAny]
    pagination_class = None


# ──────────────────────────── Events ──────────────────────────

class EventViewSet(viewsets.ModelViewSet):
    queryset = Event.objects.select_related('category', 'organizer').all()

    def get_serializer_class(self):
        if self.action in ('create', 'update', 'partial_update'):
            return EventCreateSerializer
        if self.action == 'retrieve':
            return EventDetailSerializer
        return EventListSerializer

    def get_permissions(self):
        if self.action in ('create',):
            return [permissions.IsAuthenticated(), IsOrganizer()]
        if self.action in ('update', 'partial_update', 'destroy'):
            return [permissions.IsAuthenticated(), IsEventOrganizer()]
        return [permissions.AllowAny()]

    def get_queryset(self):
        qs = super().get_queryset()
        user = self.request.user if self.request and self.request.user.is_authenticated else None
        if not (user and user.is_organizer):
            qs = qs.filter(visible_only_for_creator=False)
        else:
            # Organizers see their own private events too
            qs = qs.filter(models.Q(visible_only_for_creator=False) | models.Q(organizer=user))
        category = self.request.query_params.get('category')
        if category:
            qs = qs.filter(category__name__icontains=category)
        upcoming = self.request.query_params.get('upcoming')
        if upcoming == 'true':
            qs = qs.filter(date_time__gte=timezone.now())
        past = self.request.query_params.get('past')
        if past == 'true':
            qs = qs.filter(date_time__lt=timezone.now())
        organizer = self.request.query_params.get('organizer')
        if organizer:
            qs = qs.filter(organizer_id=organizer)
        return qs

    # --- Favorite toggle ---
    @action(detail=True, methods=['post'], permission_classes=[permissions.IsAuthenticated])
    def toggle_favorite(self, request, pk=None):
        event = self.get_object()
        fav, created = Favorite.objects.get_or_create(user=request.user, event=event)
        if not created:
            fav.delete()
            return Response({'is_favorite': False})
        return Response({'is_favorite': True}, status=status.HTTP_201_CREATED)


# ──────────────────────────── Applications ────────────────────

class ApplicationCreateView(generics.CreateAPIView):
    """Participant submits application for an event."""
    serializer_class = ApplicationSerializer
    permission_classes = [permissions.IsAuthenticated]

    def perform_create(self, serializer):
        event = Event.objects.get(pk=self.request.data['event'])
        app = serializer.save(user=self.request.user, event=event, status='pending')
        # Notify organizer
        Notification.objects.create(
            user=event.organizer,
            title='Новая заявка',
            message=f'{self.request.user.first_name} {self.request.user.last_name} подал(а) заявку на «{event.title}»',
            event=event,
        )


class MyApplicationsView(generics.ListAPIView):
    """List current user's applications."""
    serializer_class = ApplicationSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        return Application.objects.filter(user=self.request.user).select_related('event', 'user')


class EventApplicationsView(generics.ListAPIView):
    """List applications for a specific event (organizer only)."""
    serializer_class = ApplicationSerializer
    permission_classes = [permissions.IsAuthenticated, IsOrganizer]

    def get_queryset(self):
        event_id = self.kwargs['event_id']
        return Application.objects.filter(
            event_id=event_id,
            event__organizer=self.request.user,
        ).select_related('user', 'event')


class ApplicationActionView(APIView):
    """Approve / reject / mark attendance for an application."""
    permission_classes = [permissions.IsAuthenticated]

    def patch(self, request, pk):
        try:
            app = Application.objects.select_related('event', 'user').get(pk=pk)
        except Application.DoesNotExist:
            return Response({'error': 'Заявка не найдена'}, status=status.HTTP_404_NOT_FOUND)

        action_type = request.data.get('action')

        # --- Organizer actions ---
        if action_type in ('approve', 'reject'):
            if app.event.organizer != request.user:
                return Response({'error': 'Нет доступа'}, status=status.HTTP_403_FORBIDDEN)

            if action_type == 'approve':
                app.status = 'approved'
                Notification.objects.create(
                    user=app.user,
                    title='Заявка одобрена',
                    message=f'Ваша заявка на «{app.event.title}» одобрена!',
                    event=app.event,
                )
            else:
                app.status = 'rejected'
                Notification.objects.create(
                    user=app.user,
                    title='Заявка отклонена',
                    message=f'Ваша заявка на «{app.event.title}» отклонена.',
                    event=app.event,
                )
            app.save()
            return Response(ApplicationSerializer(app).data)

        # --- Attendance ---
        if action_type in ('attended', 'missed'):
            if app.event.organizer != request.user:
                return Response({'error': 'Нет доступа'}, status=status.HTTP_403_FORBIDDEN)
            if app.status != 'approved':
                return Response({'error': 'Заявка не одобрена'}, status=status.HTTP_400_BAD_REQUEST)

            app.attendance = action_type
            app.save()

            if action_type == 'attended':
                points = app.event.bonus_points
                app.user.bonus_balance += points
                app.user.save()
                BonusTransaction.objects.create(
                    user=app.user,
                    amount=points,
                    reason='attendance',
                    description=f'Посещение «{app.event.title}»',
                    event=app.event,
                )
                Notification.objects.create(
                    user=app.user,
                    title=f'+{points} баллов',
                    message=f'Начислено {points} баллов за посещение «{app.event.title}»',
                    event=app.event,
                )
            else:  # missed
                penalty = settings.PENALTY_POINTS
                app.user.bonus_balance -= penalty
                app.user.save()
                BonusTransaction.objects.create(
                    user=app.user,
                    amount=-penalty,
                    reason='penalty',
                    description=f'Штраф за неявку на «{app.event.title}»',
                    event=app.event,
                )
                Notification.objects.create(
                    user=app.user,
                    title=f'-{penalty} баллов',
                    message=f'Списано {penalty} баллов за неявку на «{app.event.title}»',
                    event=app.event,
                )
            return Response(ApplicationSerializer(app).data)

        # --- Participant cancel ---
        if action_type == 'cancel':
            if app.user != request.user:
                return Response({'error': 'Нет доступа'}, status=status.HTTP_403_FORBIDDEN)
            if app.status not in ('pending', 'approved'):
                return Response({'error': 'Невозможно отменить'}, status=status.HTTP_400_BAD_REQUEST)
            app.status = 'cancelled'
            app.save()
            Notification.objects.create(
                user=app.event.organizer,
                title='Заявка отменена',
                message=f'{request.user.first_name} {request.user.last_name} отменил(а) заявку на «{app.event.title}»',
                event=app.event,
            )
            return Response(ApplicationSerializer(app).data)

        return Response({'error': 'Неизвестное действие'}, status=status.HTTP_400_BAD_REQUEST)


# ──────────────────────────── Notifications ───────────────────

class NotificationViewSet(viewsets.ModelViewSet):
    serializer_class = NotificationSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        return Notification.objects.filter(user=self.request.user)

    @action(detail=False, methods=['post'])
    def mark_all_read(self, request):
        self.get_queryset().filter(is_read=False).update(is_read=True)
        return Response({'status': 'ok'})

    @action(detail=False, methods=['post'])
    def clear_all(self, request):
        self.get_queryset().delete()
        return Response({'status': 'ok'})


# ──────────────────────────── Reviews ─────────────────────────

class ReviewViewSet(viewsets.ModelViewSet):
    serializer_class = ReviewSerializer

    def get_permissions(self):
        if self.action in ('create', 'update', 'partial_update', 'destroy'):
            return [permissions.IsAuthenticated()]
        return [permissions.AllowAny()]

    def get_queryset(self):
        qs = Review.objects.select_related('user', 'event').all()
        event_id = self.request.query_params.get('event')
        if event_id:
            qs = qs.filter(event_id=event_id)
        return qs

    def perform_create(self, serializer):
        serializer.save(user=self.request.user)


# ──────────────────────────── Favorites ───────────────────────

class FavoriteListView(generics.ListAPIView):
    serializer_class = FavoriteSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        return Favorite.objects.filter(user=self.request.user).select_related('event__category', 'event__organizer')


# ──────────────────────────── Bonus ───────────────────────────

class BonusHistoryView(generics.ListAPIView):
    serializer_class = BonusTransactionSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        return BonusTransaction.objects.filter(user=self.request.user)


# ──────────────────────────── Promos ──────────────────────────

class PromoViewSet(viewsets.ModelViewSet):
    serializer_class = PromoSerializer

    def get_serializer_class(self):
        if self.action in ('create', 'update', 'partial_update'):
            return PromoCreateSerializer
        return PromoSerializer

    def get_permissions(self):
        if self.action in ('create', 'update', 'partial_update', 'destroy'):
            return [permissions.IsAuthenticated(), IsOrganizer()]
        return [permissions.AllowAny()]

    def get_queryset(self):
        qs = Promo.objects.select_related('organizer').all()
        my = self.request.query_params.get('my')
        if my == 'true' and self.request.user.is_authenticated:
            qs = qs.filter(organizer=self.request.user)
        return qs

    @action(detail=True, methods=['post'], permission_classes=[permissions.IsAuthenticated])
    def purchase(self, request, pk=None):
        promo = self.get_object()
        user = request.user

        if not promo.is_available:
            return Response({'error': 'Промокод недоступен'}, status=status.HTTP_400_BAD_REQUEST)

        if user.bonus_balance < promo.bonus_price:
            return Response({'error': 'Недостаточно баллов'}, status=status.HTTP_400_BAD_REQUEST)

        # Check if already purchased
        if UserPromo.objects.filter(user=user, promo=promo, status='active').exists():
            return Response({'error': 'Вы уже купили этот промокод'}, status=status.HTTP_400_BAD_REQUEST)

        # Deduct points
        user.bonus_balance -= promo.bonus_price
        user.save()

        # Create transaction
        BonusTransaction.objects.create(
            user=user,
            amount=-promo.bonus_price,
            reason='promo_purchase',
            description=f'Покупка промокода «{promo.title}»',
        )

        # Create user promo
        unique_code = uuid.uuid4().hex[:12].upper()
        user_promo = UserPromo.objects.create(
            user=user,
            promo=promo,
            unique_code=unique_code,
        )

        return Response(UserPromoSerializer(user_promo).data, status=status.HTTP_201_CREATED)


class MyPromosView(generics.ListAPIView):
    """List promos purchased by the current user."""
    serializer_class = UserPromoSerializer
    permission_classes = [permissions.IsAuthenticated]

    def get_queryset(self):
        return UserPromo.objects.filter(user=self.request.user).select_related('promo__organizer')


class PromoQRView(APIView):
    """Generate QR code for a purchased promo."""
    permission_classes = [permissions.IsAuthenticated]

    def get(self, request, pk):
        try:
            user_promo = UserPromo.objects.get(pk=pk, user=request.user)
        except UserPromo.DoesNotExist:
            return Response({'error': 'Не найдено'}, status=status.HTTP_404_NOT_FOUND)

        import io
        import base64
        import qrcode

        qr = qrcode.make(user_promo.unique_code)
        buffer = io.BytesIO()
        qr.save(buffer, format='PNG')
        qr_base64 = base64.b64encode(buffer.getvalue()).decode()

        return Response({
            'unique_code': user_promo.unique_code,
            'qr_image': f'data:image/png;base64,{qr_base64}',
        })


# ──────────────────────────── 2FA & Password Reset ───────────

import random
from django.core.mail import send_mail
from rest_framework_simplejwt.tokens import RefreshToken


def generate_otp_code():
    """Generate 6-digit OTP code."""
    return ''.join([str(random.randint(0, 9)) for _ in range(6)])


def send_otp_email(email, code, purpose):
    """Send OTP email to user."""
    if purpose == 'login':
        subject = 'Код подтверждения входа'
        message = f'Ваш код для входа: {code}\n\nКод действителен 5 минут.'
    elif purpose == 'password_reset':
        subject = 'Код для сброса пароля'
        message = f'Ваш код для сброса пароля: {code}\n\nКод действителен 5 минут.'
    else:
        subject = 'Код подтверждения email'
        message = f'Ваш код подтверждения: {code}\n\nКод действителен 10 минут.'
    
    from_email = getattr(settings, 'DEFAULT_FROM_EMAIL', 'noreply@example.com')
    send_mail(subject, message, from_email, [email], fail_silently=False)


def check_throttle(email, purpose, max_per_hour=3):
    """Check if user has exceeded OTP request limit (3 per hour)."""
    one_hour_ago = timezone.now() - timezone.timedelta(hours=1)
    recent_count = EmailOTP.objects.filter(
        email=email,
        purpose=purpose,
        created_at__gte=one_hour_ago
    ).count()
    return recent_count >= max_per_hour


class LoginStep1View(APIView):
    """Step 1: Validate credentials, send OTP if 2FA enabled."""
    permission_classes = [permissions.AllowAny]

    def post(self, request):
        email = request.data.get('email')
        password = request.data.get('password')
        remember_device = request.data.get('remember_device', False)

        if not email or not password:
            return Response({'error': 'Email и пароль обязательны'}, status=status.HTTP_400_BAD_REQUEST)

        # Find user by email
        user = User.objects.filter(email__iexact=email).order_by('id').first()
        if not user:
            return Response({'error': 'Неверный email или пароль'}, status=status.HTTP_401_UNAUTHORIZED)

        # Check password
        if not user.check_password(password):
            return Response({'error': 'Неверный email или пароль'}, status=status.HTTP_401_UNAUTHORIZED)

        # Block login if email not verified
        if not user.is_email_verified:
            return Response({'error': 'Подтвердите email перед входом'}, status=status.HTTP_403_FORBIDDEN)

        # Check if 2FA is enabled
        if not user.is_2fa_enabled:
            # No 2FA - issue tokens directly
            refresh = RefreshToken.for_user(user)
            return Response({
                'access': str(refresh.access_token),
                'refresh': str(refresh),
                'user': UserSerializer(user).data,
                'requires_otp': False,
            })

        # Check for remembered device
        device_token = request.data.get('device_token')
        if device_token and user.remember_device_token == device_token:
            if user.last_2fa_remember_until and user.last_2fa_remember_until > timezone.now():
                # Device is remembered, skip 2FA
                refresh = RefreshToken.for_user(user)
                return Response({
                    'access': str(refresh.access_token),
                    'refresh': str(refresh),
                    'user': UserSerializer(user).data,
                    'requires_otp': False,
                })

        # 2FA required - check throttle
        if check_throttle(user.email, 'login'):
            return Response(
                {'error': 'Слишком много попыток. Попробуйте через час.'},
                status=status.HTTP_429_TOO_MANY_REQUESTS
            )

        # Generate and send OTP
        code = generate_otp_code()
        expires_at = timezone.now() + timezone.timedelta(minutes=5)
        
        otp = EmailOTP.objects.create(
            user=user,
            code=code,
            purpose='login',
            email=user.email,
            expires_at=expires_at,
        )

        try:
            send_otp_email(user.email, code, 'login')
        except Exception as e:
            otp.delete()
            return Response({'error': 'Не удалось отправить email'}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)

        return Response({
            'requires_otp': True,
            'otp_id': otp.id,
            'email': user.email,
        })


class LoginStep2View(APIView):
    """Step 2: Verify OTP and issue tokens."""
    permission_classes = [permissions.AllowAny]

    def post(self, request):
        otp_id = request.data.get('otp_id')
        code = request.data.get('code')
        remember_device = request.data.get('remember_device', False)

        if not otp_id or not code:
            return Response({'error': 'ID OTP и код обязательны'}, status=status.HTTP_400_BAD_REQUEST)

        try:
            otp = EmailOTP.objects.get(id=otp_id, purpose='login')
        except EmailOTP.DoesNotExist:
            return Response({'error': 'Неверный запрос'}, status=status.HTTP_400_BAD_REQUEST)

        # Check if already used
        if otp.used:
            return Response({'error': 'Код уже использован'}, status=status.HTTP_400_BAD_REQUEST)

        # Check expiration
        if timezone.now() > otp.expires_at:
            return Response({'error': 'Код истёк'}, status=status.HTTP_400_BAD_REQUEST)

        # Check attempts
        if otp.attempts_left <= 0:
            return Response({'error': 'Слишком много попыток'}, status=status.HTTP_400_BAD_REQUEST)

        # Verify code
        if otp.code != code:
            otp.attempts_left -= 1
            otp.save()
            return Response(
                {'error': f'Неверный код. Осталось попыток: {otp.attempts_left}'},
                status=status.HTTP_400_BAD_REQUEST
            )

        # Success - mark OTP as used
        otp.used = True
        otp.save()

        # Issue tokens
        user = otp.user
        refresh = RefreshToken.for_user(user)

        # Generate device token if remember_device requested
        device_token = None
        if remember_device:
            device_token = str(uuid.uuid4())
            user.remember_device_token = device_token
            user.last_2fa_remember_until = timezone.now() + timezone.timedelta(days=14)
            user.save()

        response_data = {
            'access': str(refresh.access_token),
            'refresh': str(refresh),
            'user': UserSerializer(user).data,
        }
        if device_token:
            response_data['device_token'] = device_token

        return Response(response_data)


class PasswordResetRequestView(APIView):
    """Request password reset code via email."""
    permission_classes = [permissions.AllowAny]

    def post(self, request):
        email = request.data.get('email')

        if not email:
            return Response({'error': 'Email обязателен'}, status=status.HTTP_400_BAD_REQUEST)

        user = User.objects.filter(email__iexact=email).order_by('id').first()
        if not user:
            return Response({'message': 'Если email существует, код отправлен'})

        if not user.is_email_verified:
            return Response({'error': 'Подтвердите email перед сбросом пароля'}, status=status.HTTP_403_FORBIDDEN)

        # Check throttle
        if check_throttle(email, 'password_reset'):
            return Response(
                {'error': 'Слишком много попыток. Попробуйте через час.'},
                status=status.HTTP_429_TOO_MANY_REQUESTS
            )

        # Generate and send OTP
        code = generate_otp_code()
        expires_at = timezone.now() + timezone.timedelta(minutes=5)
        
        otp = EmailOTP.objects.create(
            user=user,
            code=code,
            purpose='password_reset',
            email=email,
            expires_at=expires_at,
        )

        try:
            send_otp_email(email, code, 'password_reset')
        except Exception:
            otp.delete()
            return Response({'error': 'Не удалось отправить email'}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)

        return Response({'message': 'Код отправлен на email', 'otp_id': otp.id})


class PasswordResetConfirmView(APIView):
    """Confirm password reset with OTP code."""
    permission_classes = [permissions.AllowAny]

    def post(self, request):
        otp_id = request.data.get('otp_id')
        code = request.data.get('code')
        new_password = request.data.get('new_password')

        if not otp_id or not code or not new_password:
            return Response({'error': 'Все поля обязательны'}, status=status.HTTP_400_BAD_REQUEST)

        if len(new_password) < 6:
            return Response({'error': 'Пароль минимум 6 символов'}, status=status.HTTP_400_BAD_REQUEST)

        try:
            otp = EmailOTP.objects.get(id=otp_id, purpose='password_reset')
        except EmailOTP.DoesNotExist:
            return Response({'error': 'Неверный запрос'}, status=status.HTTP_400_BAD_REQUEST)

        # Check if already used
        if otp.used:
            return Response({'error': 'Код уже использован'}, status=status.HTTP_400_BAD_REQUEST)

        # Check expiration
        if timezone.now() > otp.expires_at:
            return Response({'error': 'Код истёк'}, status=status.HTTP_400_BAD_REQUEST)

        # Check attempts
        if otp.attempts_left <= 0:
            return Response({'error': 'Слишком много попыток'}, status=status.HTTP_400_BAD_REQUEST)

        # Verify code
        if otp.code != code:
            otp.attempts_left -= 1
            otp.save()
            return Response(
                {'error': f'Неверный код. Осталось попыток: {otp.attempts_left}'},
                status=status.HTTP_400_BAD_REQUEST
            )

        # Success - reset password
        otp.used = True
        otp.save()

        user = otp.user
        user.set_password(new_password)
        user.save()

        return Response({'message': 'Пароль успешно изменён'})


class Toggle2FAView(APIView):
    """Enable/disable 2FA for current user."""
    permission_classes = [permissions.IsAuthenticated]

    def post(self, request):
        user = request.user
        action = request.data.get('action')

        if action not in ('enable', 'disable'):
            return Response({'error': 'Укажите action: enable или disable'}, status=status.HTTP_400_BAD_REQUEST)

        if action == 'enable':
            # Verify current password before enabling 2FA
            password = request.data.get('password')
            if not password or not user.check_password(password):
                return Response({'error': 'Неверный пароль'}, status=status.HTTP_401_UNAUTHORIZED)
            
            user.is_2fa_enabled = True
            user.save()
            return Response({'message': '2FA включена', 'is_2fa_enabled': True})
        else:
            user.is_2fa_enabled = False
            user.remember_device_token = None
            user.last_2fa_remember_until = None
            user.save()
            return Response({'message': '2FA отключена', 'is_2fa_enabled': False})


class VerifyEmailRequestView(APIView):
    permission_classes = [permissions.AllowAny]

    def post(self, request):
        email = request.data.get('email')
        if not email:
            return Response({'error': 'Email обязателен'}, status=status.HTTP_400_BAD_REQUEST)
        user = User.objects.filter(email__iexact=email).order_by('id').first()
        if not user:
            return Response({'message': 'Если email существует, код отправлен'})

        if user.is_email_verified:
            return Response({'message': 'Email уже подтвержден'})
        if check_throttle(user.email, 'verify_email'):
            return Response({'error': 'Слишком много попыток. Попробуйте через час.'}, status=status.HTTP_429_TOO_MANY_REQUESTS)

        code = generate_otp_code()
        expires_at = timezone.now() + timezone.timedelta(minutes=10)
        otp = EmailOTP.objects.create(
            user=user,
            code=code,
            purpose='verify_email',
            email=user.email,
            expires_at=expires_at,
        )
        try:
            send_otp_email(user.email, code, 'verify_email')
        except Exception:
            otp.delete()
            return Response({'error': 'Не удалось отправить email'}, status=status.HTTP_500_INTERNAL_SERVER_ERROR)

        return Response({'message': 'Код отправлен на email', 'otp_id': otp.id})


class VerifyEmailConfirmView(APIView):
    permission_classes = [permissions.AllowAny]

    def post(self, request):
        otp_id = request.data.get('otp_id')
        code = request.data.get('code')

        if not otp_id or not code:
            return Response({'error': 'ID OTP и код обязательны'}, status=status.HTTP_400_BAD_REQUEST)

        try:
            otp = EmailOTP.objects.get(id=otp_id, purpose='verify_email')
        except EmailOTP.DoesNotExist:
            return Response({'error': 'Неверный запрос'}, status=status.HTTP_400_BAD_REQUEST)

        if otp.used:
            return Response({'error': 'Код уже использован'}, status=status.HTTP_400_BAD_REQUEST)
        if timezone.now() > otp.expires_at:
            return Response({'error': 'Код истёк'}, status=status.HTTP_400_BAD_REQUEST)
        if otp.attempts_left <= 0:
            return Response({'error': 'Слишком много попыток'}, status=status.HTTP_400_BAD_REQUEST)

        if otp.code != code:
            otp.attempts_left -= 1
            otp.save()
            return Response({'error': f'Неверный код. Осталось попыток: {otp.attempts_left}'}, status=status.HTTP_400_BAD_REQUEST)

        otp.used = True
        otp.save()
        user = otp.user
        user.is_email_verified = True
        user.save()
        return Response({'message': 'Email подтвержден'})
