import os
import django

os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'config.settings')
django.setup()

from django.contrib.auth import get_user_model

User = get_user_model()
user = User.objects.get(email='artemmekena@gmail.com')
print(f'Username: {user.username}')
print(f'Email: {user.email}')
print(f'Password check (25389658): {user.check_password("25389658")}')
