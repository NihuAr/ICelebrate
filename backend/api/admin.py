from django.contrib import admin
from django.contrib.auth.admin import UserAdmin as BaseUserAdmin
from .models import (
    User, Category, Event, Contractor, EventDocument,
    Application, Notification, Review, Favorite,
    BonusTransaction, Promo, UserPromo, EmailOTP,
)


@admin.register(User)
class UserAdmin(BaseUserAdmin):
    list_display = ('username', 'email', 'first_name', 'last_name', 'is_organizer', 'bonus_balance', 'is_2fa_enabled')
    list_filter = ('is_organizer', 'is_staff', 'is_2fa_enabled')
    fieldsets = BaseUserAdmin.fieldsets + (
        ('Профиль', {'fields': ('phone_number', 'date_of_birth', 'preferences', 'profile_image')}),
        ('Организатор', {'fields': ('is_organizer', 'company_name', 'organizer_categories')}),
        ('Бонусы', {'fields': ('bonus_balance',)}),
        ('2FA Безопасность', {'fields': ('is_2fa_enabled', 'remember_device_token', 'last_2fa_remember_until'), 'classes': ('collapse',)}),
    )


@admin.register(Category)
class CategoryAdmin(admin.ModelAdmin):
    list_display = ('name', 'color')


class ContractorInline(admin.TabularInline):
    model = Contractor
    extra = 0


class EventDocumentInline(admin.TabularInline):
    model = EventDocument
    extra = 0


@admin.register(Event)
class EventAdmin(admin.ModelAdmin):
    list_display = ('title', 'date_time', 'duration_hours', 'price', 'payment_method', 'category', 'organizer')
    list_filter = ('category', 'payment_method')
    search_fields = ('title', 'description')
    inlines = [ContractorInline, EventDocumentInline]


@admin.register(Application)
class ApplicationAdmin(admin.ModelAdmin):
    list_display = ('user', 'event', 'status', 'attendance', 'created_at')
    list_filter = ('status', 'attendance')
    list_editable = ('status', 'attendance')


@admin.register(Notification)
class NotificationAdmin(admin.ModelAdmin):
    list_display = ('title', 'user', 'is_read', 'created_at')
    list_filter = ('is_read',)


@admin.register(Review)
class ReviewAdmin(admin.ModelAdmin):
    list_display = ('user', 'event', 'rating', 'created_at')


@admin.register(Favorite)
class FavoriteAdmin(admin.ModelAdmin):
    list_display = ('user', 'event', 'created_at')


@admin.register(BonusTransaction)
class BonusTransactionAdmin(admin.ModelAdmin):
    list_display = ('user', 'amount', 'reason', 'event', 'created_at')
    list_filter = ('reason',)


@admin.register(Promo)
class PromoAdmin(admin.ModelAdmin):
    list_display = ('title', 'organizer', 'bonus_price', 'usage_limit', 'is_active', 'valid_until')
    list_filter = ('is_active',)


@admin.register(UserPromo)
class UserPromoAdmin(admin.ModelAdmin):
    list_display = ('user', 'promo', 'unique_code', 'status', 'purchased_at')
    list_filter = ('status',)


@admin.register(EmailOTP)
class EmailOTPAdmin(admin.ModelAdmin):
    list_display = ('email', 'purpose', 'code_display', 'used', 'attempts_left', 'expires_at', 'created_at')
    list_filter = ('purpose', 'used')
    search_fields = ('email',)
    readonly_fields = ('created_at',)
    
    def code_display(self, obj):
        if obj.used:
            return '***' + obj.code[-3:] if len(obj.code) > 3 else '***'
        return obj.code
    code_display.short_description = 'Код'
