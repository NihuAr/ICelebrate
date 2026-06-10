import math
from django.conf import settings
from django.contrib.auth.models import AbstractUser
from django.db import models


class User(AbstractUser):
    """Custom user: participant or organizer."""
    phone_number = models.CharField('Телефон', max_length=30, blank=True)
    date_of_birth = models.DateField('Дата рождения', null=True, blank=True)
    preferences = models.JSONField('Предпочтения', default=list, blank=True)
    is_organizer = models.BooleanField('Организатор', default=False)
    company_name = models.CharField('Название компании', max_length=200, blank=True)
    organizer_categories = models.JSONField('Категории организатора', default=list, blank=True)
    profile_image = models.ImageField('Фото профиля', upload_to='profiles/', blank=True, null=True)
    bonus_balance = models.IntegerField('Бонусный баланс', default=0)
    is_email_verified = models.BooleanField('Email подтвержден', default=False)
    is_2fa_enabled = models.BooleanField('2FA включена', default=False)
    remember_device_token = models.CharField('Токен устройства', max_length=36, blank=True, null=True)
    last_2fa_remember_until = models.DateTimeField('Запомнить устройство до', null=True, blank=True)

    class Meta:
        verbose_name = 'Пользователь'
        verbose_name_plural = 'Пользователи'

    def __str__(self):
        return f'{self.first_name} {self.last_name} ({self.email})'


class Category(models.Model):
    """Event category."""
    name = models.CharField('Название', max_length=100, unique=True)
    color = models.CharField('Цвет HEX', max_length=7, default='#6366F1')

    class Meta:
        verbose_name = 'Категория'
        verbose_name_plural = 'Категории'
        ordering = ['name']

    def __str__(self):
        return self.name


class Event(models.Model):
    """Event created by an organizer."""
    PAYMENT_CHOICES = [
        ('free', 'Бесплатно'),
        ('cash', 'Наличные'),
        ('card', 'Карта (терминал)'),
        ('both', 'Наличные и карта'),
    ]

    title = models.CharField('Название', max_length=300)
    description = models.TextField('Описание', blank=True)
    date_time = models.DateTimeField('Дата и время')
    duration_hours = models.DecimalField('Длительность (часы)', max_digits=5, decimal_places=1, default=1)
    location = models.CharField('Место', max_length=300)
    price = models.DecimalField('Стоимость (BYN)', max_digits=10, decimal_places=2, default=0)
    payment_method = models.CharField('Способ оплаты', max_length=10, choices=PAYMENT_CHOICES, default='free')
    category = models.ForeignKey(Category, on_delete=models.SET_NULL, null=True, blank=True, related_name='events', verbose_name='Категория')
    image = models.ImageField('Изображение', upload_to='events/', blank=True, null=True)
    color = models.CharField('Цвет (фон)', max_length=7, default='#6366F1')
    visible_only_for_creator = models.BooleanField('Видно только организатору', default=False)
    organizer = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE, related_name='organized_events', verbose_name='Организатор')
    created_at = models.DateTimeField('Создано', auto_now_add=True)

    class Meta:
        verbose_name = 'Мероприятие'
        verbose_name_plural = 'Мероприятия'
        ordering = ['-date_time']

    def __str__(self):
        return self.title

    @property
    def bonus_points(self):
        """Calculate bonus points for attending: hours + price/10."""
        hours = float(self.duration_hours)
        price = float(self.price)
        return math.floor(hours * settings.BONUS_POINTS_PER_HOUR + price * settings.BONUS_POINTS_PER_BYN)


class Contractor(models.Model):
    """Contractor / performer attached to an event."""
    event = models.ForeignKey(Event, on_delete=models.CASCADE, related_name='contractors', verbose_name='Мероприятие')
    name = models.CharField('Имя', max_length=200)
    role = models.CharField('Роль', max_length=200)

    class Meta:
        verbose_name = 'Подрядчик'
        verbose_name_plural = 'Подрядчики'

    def __str__(self):
        return f'{self.name} ({self.role})'


class EventDocument(models.Model):
    """Document attached to an event."""
    event = models.ForeignKey(Event, on_delete=models.CASCADE, related_name='documents', verbose_name='Мероприятие')
    file = models.FileField('Файл', upload_to='documents/')
    original_name = models.CharField('Имя файла', max_length=300)
    uploaded_at = models.DateTimeField('Загружено', auto_now_add=True)

    class Meta:
        verbose_name = 'Документ'
        verbose_name_plural = 'Документы'

    def __str__(self):
        return self.original_name


class Application(models.Model):
    """
    Application (request) from a participant to attend an event.
    Replaces the old EventParticipant model.
    """
    STATUS_CHOICES = [
        ('pending', 'В обработке'),
        ('approved', 'Подтверждено'),
        ('rejected', 'Отклонено'),
        ('cancelled', 'Отменено участником'),
    ]
    ATTENDANCE_CHOICES = [
        ('unknown', 'Неизвестно'),
        ('attended', 'Посетил'),
        ('missed', 'Не пришёл'),
    ]

    event = models.ForeignKey(Event, on_delete=models.CASCADE, related_name='applications', verbose_name='Мероприятие')
    user = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE, related_name='applications', verbose_name='Участник')
    status = models.CharField('Статус заявки', max_length=10, choices=STATUS_CHOICES, default='pending')
    attendance = models.CharField('Посещение', max_length=10, choices=ATTENDANCE_CHOICES, default='unknown')
    created_at = models.DateTimeField('Дата заявки', auto_now_add=True)
    updated_at = models.DateTimeField('Обновлено', auto_now=True)

    class Meta:
        verbose_name = 'Заявка'
        verbose_name_plural = 'Заявки'
        unique_together = ['event', 'user']
        ordering = ['-created_at']

    def __str__(self):
        return f'{self.user} → {self.event} [{self.get_status_display()}]'


class Notification(models.Model):
    """In-app notification for a user."""
    user = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE, related_name='notifications', verbose_name='Пользователь')
    title = models.CharField('Заголовок', max_length=300)
    message = models.TextField('Сообщение')
    is_read = models.BooleanField('Прочитано', default=False)
    event = models.ForeignKey(Event, on_delete=models.SET_NULL, null=True, blank=True, verbose_name='Мероприятие')
    created_at = models.DateTimeField('Дата', auto_now_add=True)

    class Meta:
        verbose_name = 'Уведомление'
        verbose_name_plural = 'Уведомления'
        ordering = ['-created_at']

    def __str__(self):
        return f'{self.title} → {self.user}'


class Review(models.Model):
    """Review for an event."""
    event = models.ForeignKey(Event, on_delete=models.CASCADE, related_name='reviews', verbose_name='Мероприятие')
    user = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE, related_name='reviews', verbose_name='Автор')
    rating = models.PositiveSmallIntegerField('Оценка')
    comment = models.TextField('Комментарий', blank=True)
    created_at = models.DateTimeField('Дата', auto_now_add=True)

    class Meta:
        verbose_name = 'Отзыв'
        verbose_name_plural = 'Отзывы'
        unique_together = ['event', 'user']
        ordering = ['-created_at']

    def __str__(self):
        return f'{self.user} → {self.event} ({self.rating}★)'


class Favorite(models.Model):
    """Favorite event for a user."""
    user = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE, related_name='favorites', verbose_name='Пользователь')
    event = models.ForeignKey(Event, on_delete=models.CASCADE, related_name='favorited_by', verbose_name='Мероприятие')
    created_at = models.DateTimeField('Добавлено', auto_now_add=True)

    class Meta:
        verbose_name = 'Избранное'
        verbose_name_plural = 'Избранное'
        unique_together = ['user', 'event']

    def __str__(self):
        return f'{self.user} ♥ {self.event}'


class BonusTransaction(models.Model):
    """Bonus points ledger entry."""
    REASON_CHOICES = [
        ('attendance', 'Посещение мероприятия'),
        ('penalty', 'Штраф за неявку'),
        ('promo_purchase', 'Покупка промокода'),
        ('admin_adjustment', 'Корректировка администратором'),
    ]

    user = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE, related_name='bonus_transactions', verbose_name='Пользователь')
    amount = models.IntegerField('Сумма баллов')
    reason = models.CharField('Причина', max_length=20, choices=REASON_CHOICES)
    description = models.CharField('Описание', max_length=500, blank=True)
    event = models.ForeignKey(Event, on_delete=models.SET_NULL, null=True, blank=True, verbose_name='Мероприятие')
    created_at = models.DateTimeField('Дата', auto_now_add=True)

    class Meta:
        verbose_name = 'Бонусная операция'
        verbose_name_plural = 'Бонусные операции'
        ordering = ['-created_at']

    def __str__(self):
        sign = '+' if self.amount > 0 else ''
        return f'{sign}{self.amount} → {self.user} ({self.get_reason_display()})'


class Promo(models.Model):
    """Promo code created by an organizer, purchasable with bonus points."""
    organizer = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE, related_name='created_promos', verbose_name='Организатор')
    title = models.CharField('Название', max_length=300)
    description = models.TextField('Описание', blank=True)
    bonus_price = models.PositiveIntegerField('Цена в баллах')
    usage_limit = models.PositiveIntegerField('Лимит использований', default=0, help_text='0 = без лимита')
    valid_until = models.DateTimeField('Действует до', null=True, blank=True)
    is_active = models.BooleanField('Активен', default=True)
    created_at = models.DateTimeField('Создано', auto_now_add=True)

    class Meta:
        verbose_name = 'Промокод'
        verbose_name_plural = 'Промокоды'
        ordering = ['-created_at']

    def __str__(self):
        return f'{self.title} ({self.bonus_price} баллов)'

    @property
    def purchases_count(self):
        return self.purchases.count()

    @property
    def is_available(self):
        from django.utils import timezone
        if not self.is_active:
            return False
        if self.valid_until and self.valid_until < timezone.now():
            return False
        if self.usage_limit > 0 and self.purchases_count >= self.usage_limit:
            return False
        return True


class UserPromo(models.Model):
    """A promo purchased by a user."""
    STATUS_CHOICES = [
        ('active', 'Активен'),
        ('used', 'Использован'),
        ('expired', 'Истёк'),
    ]

    user = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE, related_name='purchased_promos', verbose_name='Покупатель')
    promo = models.ForeignKey(Promo, on_delete=models.CASCADE, related_name='purchases', verbose_name='Промокод')
    unique_code = models.CharField('Уникальный код', max_length=50, unique=True)
    status = models.CharField('Статус', max_length=10, choices=STATUS_CHOICES, default='active')
    purchased_at = models.DateTimeField('Куплено', auto_now_add=True)

    class Meta:
        verbose_name = 'Купленный промокод'
        verbose_name_plural = 'Купленные промокоды'
        ordering = ['-purchased_at']

    def __str__(self):
        return f'{self.user} → {self.promo.title} [{self.unique_code}]'


class EmailOTP(models.Model):
    """One-time password for 2FA and password reset."""
    PURPOSE_CHOICES = [
        ('login', 'Вход в систему'),
        ('password_reset', 'Сброс пароля'),
        ('verify_email', 'Подтверждение email'),
    ]

    user = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE, related_name='email_otps', verbose_name='Пользователь')
    code = models.CharField('Код', max_length=6)
    purpose = models.CharField('Назначение', max_length=20, choices=PURPOSE_CHOICES)
    email = models.EmailField('Email')
    expires_at = models.DateTimeField('Истекает')
    attempts_left = models.IntegerField('Осталось попыток', default=5)
    used = models.BooleanField('Использован', default=False)
    created_at = models.DateTimeField('Создан', auto_now_add=True)

    class Meta:
        verbose_name = 'Email OTP'
        verbose_name_plural = 'Email OTPs'
        ordering = ['-created_at']
        indexes = [
            models.Index(fields=['email', 'purpose', 'created_at']),
        ]

    def __str__(self):
        return f'{self.email} - {self.purpose} - {"***" + self.code[-3:]}'
