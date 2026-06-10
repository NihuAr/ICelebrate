from django.urls import path, include
from rest_framework.routers import DefaultRouter
from . import views

router = DefaultRouter()
router.register(r'categories', views.CategoryViewSet, basename='category')
router.register(r'events', views.EventViewSet, basename='event')
router.register(r'notifications', views.NotificationViewSet, basename='notification')
router.register(r'reviews', views.ReviewViewSet, basename='review')
router.register(r'promos', views.PromoViewSet, basename='promo')

urlpatterns = [
    # Router
    path('', include(router.urls)),

    # Auth
    path('auth/register/', views.RegisterView.as_view(), name='register'),
    path('auth/profile/', views.ProfileView.as_view(), name='profile'),
    path('auth/become-organizer/', views.BecomeOrganizerView.as_view(), name='become-organizer'),
    path('auth/leave-organizer/', views.LeaveOrganizerView.as_view(), name='leave-organizer'),

    # 2FA & Password Reset
    path('auth/login/', views.LoginStep1View.as_view(), name='login-step1'),
    path('auth/login/otp/', views.LoginStep2View.as_view(), name='login-step2'),
    path('auth/password-reset/request/', views.PasswordResetRequestView.as_view(), name='password-reset-request'),
    path('auth/password-reset/confirm/', views.PasswordResetConfirmView.as_view(), name='password-reset-confirm'),
    path('auth/2fa/toggle/', views.Toggle2FAView.as_view(), name='toggle-2fa'),
    path('auth/verify-email/request/', views.VerifyEmailRequestView.as_view(), name='verify-email-request'),
    path('auth/verify-email/confirm/', views.VerifyEmailConfirmView.as_view(), name='verify-email-confirm'),

    # Applications
    path('applications/', views.ApplicationCreateView.as_view(), name='application-create'),
    path('applications/my/', views.MyApplicationsView.as_view(), name='my-applications'),
    path('applications/<int:pk>/action/', views.ApplicationActionView.as_view(), name='application-action'),
    path('events/<int:event_id>/applications/', views.EventApplicationsView.as_view(), name='event-applications'),

    # Favorites
    path('favorites/', views.FavoriteListView.as_view(), name='favorites'),

    # Bonus
    path('bonus/history/', views.BonusHistoryView.as_view(), name='bonus-history'),

    # User promos
    path('my-promos/', views.MyPromosView.as_view(), name='my-promos'),
    path('my-promos/<int:pk>/qr/', views.PromoQRView.as_view(), name='promo-qr'),
]
