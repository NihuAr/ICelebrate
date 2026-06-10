#!/usr/bin/env python
import os
import django
from datetime import datetime, timedelta

os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'config.settings')
django.setup()

from django.contrib.auth import get_user_model
from api.models import Promo

User = get_user_model()

# Get admin user
try:
    admin_user = User.objects.get(username='admin')
except User.DoesNotExist:
    print("Admin user not found!")
    exit(1)

# Clear existing promos from admin
Promo.objects.filter(organizer=admin_user).delete()

# Create mock promos
promos_data = [
    {
        'title': '10% скидка на следующее мероприятие',
        'description': 'Получите 10% скидку на любое платное мероприятие',
        'bonus_price': 50,
        'usage_limit': 100,
        'valid_until': datetime.now() + timedelta(days=30),
    },
    {
        'title': 'Бесплатный вход на премиум-событие',
        'description': 'Посетите премиум-мероприятие совершенно бесплатно',
        'bonus_price': 150,
        'usage_limit': 20,
        'valid_until': datetime.now() + timedelta(days=60),
    },
    {
        'title': '20 дополнительных бонусов',
        'description': 'Получите 20 бонусных баллов на счёт',
        'bonus_price': 30,
        'usage_limit': 0,
        'valid_until': None,
    },
    {
        'title': 'VIP-доступ на месяц',
        'description': 'Месячный доступ к эксклюзивным событиям',
        'bonus_price': 200,
        'usage_limit': 10,
        'valid_until': datetime.now() + timedelta(days=90),
    },
    {
        'title': 'Бесплатная консультация организатора',
        'description': 'Получите бесплатную консультацию от опытного организатора',
        'bonus_price': 75,
        'usage_limit': 50,
        'valid_until': datetime.now() + timedelta(days=45),
    },
]

for promo_data in promos_data:
    promo = Promo.objects.create(
        organizer=admin_user,
        title=promo_data['title'],
        description=promo_data['description'],
        bonus_price=promo_data['bonus_price'],
        usage_limit=promo_data['usage_limit'],
        valid_until=promo_data['valid_until'],
        is_active=True,
    )
    print(f"Created promo: {promo.title}")

print(f"\nTotal promos created: {Promo.objects.filter(organizer=admin_user).count()}")
