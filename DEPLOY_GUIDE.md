# iCelebrate — Полное руководство по развёртыванию

## 🌐 Развёртывание веб-версии на Netlify

### Шаг 1: Подготовка
```bash
cd frontend
npm install
npm run build
```

### Шаг 2: Загрузка на Netlify
1. Зарегистрируйтесь на https://netlify.com (бесплатно)
2. Нажмите "Add new site" → "Deploy manually"
3. Перетащите папку `frontend/dist/` в Netlify
4. Или подключите GitHub репозиторий для автоматического развёртывания

### Шаг 3: Получение ссылки
- Netlify выдаст ссылку типа `https://icelebrate-xxx.netlify.app`
- Это ваша публичная ссылка!

---

## ☁️ Развёртывание бэкенда на Heroku

### Требования
- Установите [Heroku CLI](https://devcenter.heroku.com/articles/heroku-cli)
- Создайте аккаунт на https://heroku.com

### Шаг 1: Инициализация Git
```bash
cd backend
git init
git add .
git commit -m "Initial commit"
```

### Шаг 2: Создание приложения на Heroku
```bash
heroku login
heroku create icelebrate-api
```

### Шаг 3: Добавление PostgreSQL
```bash
heroku addons:create heroku-postgresql:hobby-dev
```

### Шаг 4: Развёртывание
```bash
git push heroku main
heroku run python manage.py migrate
heroku run python manage.py createsuperuser
```

### Шаг 5: Проверка
```bash
heroku open
# Или откройте https://icelebrate-api.herokuapp.com/api/events/
```

### Шаг 6: Переменные окружения (если нужны)
```bash
heroku config:set SECRET_KEY="your-secret-key"
heroku config:set HEROKU=True
```

---

## 🔗 Связывание фронтенда и бэкенда

### На Netlify (автоматически)
- Файл `frontend/netlify.toml` уже настроен
- Все запросы `/api/*` перенаправляются на `https://icelebrate-api.herokuapp.com/api/`

### Если нужно изменить URL бэкенда:
Отредактируйте `frontend/netlify.toml`:
```toml
[[redirects]]
from = "/api/*"
to = "https://YOUR-API-URL.herokuapp.com/api/:splat"
status = 200
force = true
```

---

## 📋 Чек-лист развёртывания

- [ ] Веб-фронтенд собран (`npm run build`)
- [ ] Загружен на Netlify
- [ ] Получена ссылка Netlify
- [ ] Бэкенд инициализирован в Git
- [ ] Создано приложение на Heroku
- [ ] PostgreSQL добавлена
- [ ] Миграции выполнены
- [ ] Суперпользователь создан
- [ ] Бэкенд доступен по ссылке Heroku
- [ ] Фронтенд подключен к бэкенду

---

## 🧪 Тестирование

### Проверить фронтенд
```bash
# Откройте ссылку Netlify в браузере
https://icelebrate-xxx.netlify.app
```

### Проверить бэкенд
```bash
# Откройте в браузере
https://icelebrate-api.herokuapp.com/api/events/
```

### Проверить связь
1. Откройте фронтенд
2. Попробуйте зарегистрироваться
3. Попробуйте войти
4. Проверьте консоль браузера (F12) на ошибки

---

## 🆘 Решение проблем

### Бэкенд не доступен
```bash
heroku logs --tail
```

### Миграции не выполнены
```bash
heroku run python manage.py migrate
```

### Нужно очистить БД
```bash
heroku pg:reset DATABASE
heroku run python manage.py migrate
```

### Статические файлы не загружаются
```bash
heroku run python manage.py collectstatic --noinput
```

---

## 📱 Инструкция для друзей

```
Привет! Вот ссылка на мой сайт:
https://icelebrate-xxx.netlify.app

Как использовать:
1. Откройте ссылку в браузере
2. Нажмите "Продолжить в веб-версии"
3. Зарегистрируйтесь или войдите (admin / admin123)
4. Создавайте события и приглашайте друзей!

Требования: Интернет + браузер (Chrome, Firefox, Safari)
```

---

## 🚀 Готово!

Теперь ваше приложение доступно для всех в интернете!
