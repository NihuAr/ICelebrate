from django.contrib.auth import get_user_model
from django.db.models import Q
from rest_framework import serializers
from .models import (
    Category, Event, Contractor, EventDocument,
    Application, Notification, Review, Favorite,
    BonusTransaction, Promo, UserPromo,
)

User = get_user_model()


# ──────────────────────────── Auth ────────────────────────────

class RegisterSerializer(serializers.ModelSerializer):
    password = serializers.CharField(write_only=True, min_length=6)

    class Meta:
        model = User
        fields = (
            'id', 'username', 'email', 'password',
            'first_name', 'last_name', 'phone_number',
            'date_of_birth', 'preferences',
        )

    def create(self, validated_data):
        password = validated_data.pop('password')
        username = validated_data.get('username')
        email = validated_data.get('email')

        # Normalize email to lower
        if email:
            validated_data['email'] = email.lower()

        # Remove unverified duplicates with same username or email
        qs = User.objects.filter(
            Q(username__iexact=username) | Q(email__iexact=email)
        ).filter(is_email_verified=False)
        if qs.exists():
            qs.delete()

        # If a verified user already exists, block
        if User.objects.filter(Q(username__iexact=username) | Q(email__iexact=email)).exists():
            raise serializers.ValidationError('Пользователь с таким именем или email уже существует')

        user = User(**validated_data)
        user.set_password(password)
        user.save()
        return user


class UserSerializer(serializers.ModelSerializer):
    class Meta:
        model = User
        fields = (
            'id', 'username', 'email', 'first_name', 'last_name',
            'phone_number', 'date_of_birth', 'preferences',
            'is_organizer', 'company_name', 'organizer_categories',
            'profile_image', 'bonus_balance', 'is_2fa_enabled', 'is_email_verified',
        )
        read_only_fields = ('id', 'bonus_balance')


class UserPublicSerializer(serializers.ModelSerializer):
    """Minimal public info about a user."""
    class Meta:
        model = User
        fields = ('id', 'first_name', 'last_name', 'company_name', 'profile_image')


# ──────────────────────────── Category ────────────────────────

class CategorySerializer(serializers.ModelSerializer):
    class Meta:
        model = Category
        fields = '__all__'


# ──────────────────────────── Event ───────────────────────────

class ContractorSerializer(serializers.ModelSerializer):
    class Meta:
        model = Contractor
        fields = ('id', 'name', 'role')


class EventDocumentSerializer(serializers.ModelSerializer):
    class Meta:
        model = EventDocument
        fields = ('id', 'file', 'original_name', 'uploaded_at')
        read_only_fields = ('uploaded_at',)


class EventListSerializer(serializers.ModelSerializer):
    category = CategorySerializer(read_only=True)
    organizer = UserPublicSerializer(read_only=True)
    bonus_points = serializers.IntegerField(read_only=True)
    is_favorite = serializers.SerializerMethodField()

    class Meta:
        model = Event
        fields = (
            'id', 'title', 'description', 'date_time', 'duration_hours',
            'location', 'price', 'payment_method', 'category', 'image',
            'color', 'visible_only_for_creator', 'organizer', 'bonus_points', 'is_favorite', 'created_at',
        )

    def get_is_favorite(self, obj):
        request = self.context.get('request')
        if request and request.user.is_authenticated:
            return Favorite.objects.filter(user=request.user, event=obj).exists()
        return False


class EventDetailSerializer(EventListSerializer):
    contractors = ContractorSerializer(many=True, read_only=True)
    documents = EventDocumentSerializer(many=True, read_only=True)
    applications_count = serializers.SerializerMethodField()
    average_rating = serializers.SerializerMethodField()

    class Meta(EventListSerializer.Meta):
        fields = EventListSerializer.Meta.fields + (
            'contractors', 'documents', 'applications_count', 'average_rating',
        )

    def get_applications_count(self, obj):
        return obj.applications.filter(status='approved').count()

    def get_average_rating(self, obj):
        reviews = obj.reviews.all()
        if not reviews.exists():
            return None
        return round(sum(r.rating for r in reviews) / reviews.count(), 1)


class EventCreateSerializer(serializers.ModelSerializer):
    category_id = serializers.IntegerField(write_only=True, required=False, allow_null=True)

    class Meta:
        model = Event
        fields = (
            'id', 'title', 'description', 'date_time', 'duration_hours',
            'location', 'price', 'payment_method', 'category_id',
            'image', 'color', 'visible_only_for_creator',
        )
        read_only_fields = ('id',)

    def create(self, validated_data):
        validated_data['organizer'] = self.context['request'].user
        category_id = validated_data.pop('category_id', None)
        if category_id:
            validated_data['category_id'] = category_id
        return super().create(validated_data)


# ──────────────────────────── Application ─────────────────────

class ApplicationSerializer(serializers.ModelSerializer):
    user = UserPublicSerializer(read_only=True)
    event_title = serializers.CharField(source='event.title', read_only=True)
    status_display = serializers.CharField(source='get_status_display', read_only=True)
    attendance_display = serializers.CharField(source='get_attendance_display', read_only=True)

    class Meta:
        model = Application
        fields = (
            'id', 'event', 'user', 'event_title',
            'status', 'status_display',
            'attendance', 'attendance_display',
            'created_at', 'updated_at',
        )
        read_only_fields = ('id', 'user', 'created_at', 'updated_at')


# ──────────────────────────── Notification ────────────────────

class NotificationSerializer(serializers.ModelSerializer):
    class Meta:
        model = Notification
        fields = ('id', 'title', 'message', 'is_read', 'event', 'created_at')
        read_only_fields = ('id', 'created_at')


# ──────────────────────────── Review ──────────────────────────

class ReviewSerializer(serializers.ModelSerializer):
    user = UserPublicSerializer(read_only=True)

    class Meta:
        model = Review
        fields = ('id', 'event', 'user', 'rating', 'comment', 'created_at')
        read_only_fields = ('id', 'user', 'created_at')


# ──────────────────────────── Favorite ────────────────────────

class FavoriteSerializer(serializers.ModelSerializer):
    event = EventListSerializer(read_only=True)

    class Meta:
        model = Favorite
        fields = ('id', 'event', 'created_at')


# ──────────────────────────── Bonus ───────────────────────────

class BonusTransactionSerializer(serializers.ModelSerializer):
    class Meta:
        model = BonusTransaction
        fields = ('id', 'amount', 'reason', 'description', 'event', 'created_at')
        read_only_fields = '__all__'


# ──────────────────────────── Promo ───────────────────────────

class PromoSerializer(serializers.ModelSerializer):
    organizer = UserPublicSerializer(read_only=True)
    is_available = serializers.BooleanField(read_only=True)
    purchases_count = serializers.IntegerField(read_only=True)

    class Meta:
        model = Promo
        fields = (
            'id', 'organizer', 'title', 'description', 'bonus_price',
            'usage_limit', 'valid_until', 'is_active', 'is_available',
            'purchases_count', 'created_at',
        )
        read_only_fields = ('id', 'created_at')


class PromoCreateSerializer(serializers.ModelSerializer):
    class Meta:
        model = Promo
        fields = ('id', 'title', 'description', 'bonus_price', 'usage_limit', 'valid_until')
        read_only_fields = ('id',)

    def create(self, validated_data):
        validated_data['organizer'] = self.context['request'].user
        return super().create(validated_data)


class UserPromoSerializer(serializers.ModelSerializer):
    promo = PromoSerializer(read_only=True)

    class Meta:
        model = UserPromo
        fields = ('id', 'promo', 'unique_code', 'status', 'purchased_at')
        read_only_fields = '__all__'
