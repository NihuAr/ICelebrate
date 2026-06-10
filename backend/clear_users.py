import os
import django

os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'config.settings')
django.setup()

from django.contrib.auth import get_user_model

User = get_user_model()

# Удаляем всех пользователей кроме admin (id=1)
deleted_count, _ = User.objects.exclude(id=1).delete()
print(f'Удалено пользователей: {deleted_count}')

# Показываем оставшихся
remaining = User.objects.all()
print(f'\nОставшиеся пользователи:')
for user in remaining:
    print(f'  - {user.username} ({user.email})')
