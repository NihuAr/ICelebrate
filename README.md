# iCelebrate — Планировщик групповых мероприятий

> Умный способ организовать групповые мероприятия. Создавайте события, приглашайте участников, делитесь идеями и координируйте все детали — в одном месте.

![iCelebrate](https://img.shields.io/badge/version-1.0.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Web-blue)

---

## 🚀 Быстрый старт

### Веб-версия (для всех)
```bash
cd frontend
npm install
npm run dev
```
Откроется на `http://localhost:5173`

### Десктоп-версия (Windows)
```bash
cd Kursachzhoska
dotnet run
```

### Бэкенд (Django)
```bash
cd backend
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
python manage.py migrate
python manage.py runserver
```

---

## 📋 Возможности

### 👤 Для пользователей
- ✅ Просмотр каталога мероприятий с фильтром по категориям и датам
- ✅ Поиск по названию, описанию, месту
- ✅ Добавление в избранное
- ✅ Регистрация на мероприятия
- ✅ Просмотр истории посещённых событий
- ✅ Оставление отзывов и оценок
- ✅ Система бонусов за посещение
- ✅ Покупка промокодов за бонусы
- ✅ Уведомления о событиях

### 🎯 Для организаторов
- ✅ Создание и редактирование мероприятий
- ✅ Управление заявками участников
- ✅ Отмечание посещаемости
- ✅ Просмотр списка участников
- ✅ Создание промокодов
- ✅ Просмотр отзывов и рейтинга

---

## 🏗️ Архитектура

```
iCelebrate/
├── frontend/          # React + Vite + Tailwind CSS
│   ├── src/
│   │   ├── pages/     # 20+ страниц приложения
│   │   ├── components/# Компоненты (Layout, Sidebar)
│   │   ├── context/   # Auth Context
│   │   └── api.js     # Axios клиент
│   └── package.json
├── backend/           # Django REST API
│   ├── api/           # REST endpoints
│   ├── models.py      # User, Event, Application, Review, etc.
│   └── settings.py    # Конфигурация
├── Kursachzhoska/     # WPF десктоп-приложение (C#)
│   ├── Pages/         # 20+ страниц WPF
│   ├── Services/      # ApiService, DataService
│   └── Models/        # Модели данных
└── DEPLOYMENT.md      # Инструкция по развёртыванию
```

---

## 🛠️ Технологический стек

### Фронтенд
- **React 19** — UI библиотека
- **Vite** — сборщик
- **Tailwind CSS** — стили
- **Lucide React** — иконки
- **React Router** — маршрутизация
- **Axios** — HTTP клиент

### Бэкенд
- **Django 4.2** — веб-фреймворк
- **Django REST Framework** — REST API
- **PostgreSQL 18** — база данных
- **JWT** — аутентификация

### Десктоп
- **WPF** — UI фреймворк
- **C# 11** — язык программирования
- **.NET Framework 4.7.2** — runtime

---

## 📦 Установка

### Требования
- Node.js 18+
- Python 3.10+
- PostgreSQL 12+
- Visual Studio 2022 (для WPF)

### Шаг 1: Клонируйте репозиторий
```bash
git clone https://github.com/yourusername/icelebrate.git
cd icelebrate
```

### Шаг 2: Установите зависимости

**Фронтенд:**
```bash
cd frontend
npm install
```

**Бэкенд:**
```bash
cd backend
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
```

### Шаг 3: Настройте БД
```bash
cd backend
python manage.py migrate
python manage.py createsuperuser
```

### Шаг 4: Запустите
```bash
# Терминал 1 - Бэкенд
cd backend
python manage.py runserver

# Терминал 2 - Фронтенд
cd frontend
npm run dev

# Терминал 3 - Десктоп (опционально)
cd Kursachzhoska
dotnet run
```

---

## 🌐 Развёртывание

### Веб-версия
```bash
cd frontend
npm run build
# Загрузите frontend/dist/ на Netlify / Vercel / GitHub Pages
```

### Десктоп-версия
```powershell
.\publish-desktop.ps1 -Version "1.0.0"
# Загрузите releases/iCelebrate-v1.0.0-portable.zip на GitHub Releases
```

### Бэкенд
Развёртывание на Heroku / PythonAnywhere / DigitalOcean

Подробнее: см. `DEPLOYMENT.md` и `SHARING.md`

---

## 📖 Документация

- **[DEPLOYMENT.md](DEPLOYMENT.md)** — Полная инструкция по развёртыванию
- **[SHARING.md](SHARING.md)** — Как поделиться с друзьями
- **[API.md](backend/API.md)** — Документация REST API
- **[CONTRIBUTING.md](CONTRIBUTING.md)** — Как контрибьютить

---

## 🔐 Безопасность

- JWT аутентификация
- CORS настройки
- Валидация входных данных
- Защита от CSRF
- Хеширование паролей (bcrypt)

---

## 📊 Статистика

- **20+ страниц** в веб-версии
- **20+ страниц** в десктоп-версии
- **10+ моделей** в БД
- **50+ API endpoints**
- **100% русский интерфейс**

---

## 🤝 Контрибьютинг

Приветствуются pull requests! Для больших изменений сначала откройте issue.

1. Fork репозиторий
2. Создайте ветку (`git checkout -b feature/AmazingFeature`)
3. Коммитьте изменения (`git commit -m 'Add AmazingFeature'`)
4. Пушьте в ветку (`git push origin feature/AmazingFeature`)
5. Откройте Pull Request

---

## 📝 Лицензия

MIT License — см. `LICENSE` файл

---

## 👥 Авторы

- **Разработчик** — Ваше имя
- **Дизайн** — Figma
- **Тестирование** — Друзья и коллеги

---

## 🙏 Благодарности

- Django и DRF команде
- React команде
- WPF сообществу
- Всем контрибьюторам

---

## 📞 Поддержка

- 📧 Email: support@icelebrate.local
- 💬 Issues: https://github.com/yourusername/icelebrate/issues
- 🐦 Twitter: @icelebrate

---

## 🎯 Roadmap

- [ ] Мобильное приложение (React Native)
- [ ] Видеоконференции для событий
- [ ] Интеграция с календарями (Google Calendar, Outlook)
- [ ] Платежи (Stripe, PayPal)
- [ ] Аналитика и статистика
- [ ] Темная тема
- [ ] Многоязычность

---

**Made with ❤️ for event planning**
