# iCelebrate — Инструкция по развёртыванию

## Веб-версия (React)

### Для разработки
```bash
cd frontend
npm install
npm run dev
```
Откроется на `http://localhost:5173`

### Для продакшена
```bash
cd frontend
npm run build
```
Статические файлы в `frontend/dist/` — загрузить на хостинг (Netlify, Vercel, GitHub Pages и т.д.)

---

## Десктоп-версия (WPF)

### Требования
- Windows 7 SP1 или выше
- .NET Framework 4.7.2+

### Установка для пользователей

#### Способ 1: Самостоятельная сборка
1. Установите Visual Studio 2022 Community (бесплатно)
2. Откройте `Kursachzhoska.sln`
3. Выберите конфигурацию **Release**
4. Нажмите **Build → Build Solution**
5. Готовый exe в `Kursachzhoska\bin\Release\`

#### Способ 2: Портативная версия (рекомендуется)
1. Опубликуйте приложение:
```bash
cd Kursachzhoska
dotnet publish -c Release -o publish
```
2. Архивируйте папку `publish/` в `iCelebrate-portable.zip`
3. Распространяйте zip-файл

**Для запуска:**
- Распакуйте zip
- Запустите `Kursachzhoska.exe`

#### Способ 3: Установщик (требует WiX Toolset)
```bash
# Установите WiX Toolset отдельно
# Затем создайте MSI установщик
dotnet publish -c Release
# Используйте WiX для создания .msi файла
```

---

## Бэкенд (Django)

### Требования
- Python 3.10+
- PostgreSQL 12+

### Первый запуск
```bash
cd backend
python -m venv venv
venv\Scripts\activate  # Windows
source venv/bin/activate  # Linux/macOS

pip install -r requirements.txt
python manage.py migrate
python manage.py createsuperuser  # admin / admin123
python manage.py runserver
```

API доступен на `http://localhost:8000/api/`

---

## Распространение

### Веб-версия
1. Соберите: `npm run build`
2. Загрузите на Netlify / Vercel / GitHub Pages
3. Поделитесь ссылкой: `https://your-domain.com`

### Десктоп-версия
1. Опубликуйте: `dotnet publish -c Release -o publish`
2. Архивируйте: `iCelebrate-v1.0-portable.zip`
3. Загрузите на GitHub Releases / Google Drive / Яндекс.Диск
4. Поделитесь ссылкой для скачивания

### Инструкция для пользователей
```
1. Скачайте iCelebrate-v1.0-portable.zip
2. Распакуйте в любую папку
3. Запустите iCelebrate.exe
4. Приложение готово к использованию!
```

---

## Конфигурация

### Веб-фронтенд
Файл: `frontend/vite.config.js`
```javascript
proxy: {
  '/api': 'http://localhost:8000',  // Измените на ваш бэкенд
}
```

### Десктоп-приложение
Файл: `Kursachzhoska/Services/ApiService.cs`
```csharp
private const string BaseUrl = "http://localhost:8000/api/";  // Измените на ваш бэкенд
```

### Бэкенд
Файл: `backend/settings.py`
```python
ALLOWED_HOSTS = ['localhost', 'your-domain.com']
CORS_ALLOWED_ORIGINS = [
    'http://localhost:5173',  # Веб-фронтенд
    'http://localhost:3000',
]
```

---

## Проблемы и решения

### Веб-версия не подключается к бэкенду
- Проверьте что бэкенд запущен: `python manage.py runserver`
- Проверьте CORS в `backend/settings.py`
- Проверьте прокси в `frontend/vite.config.js`

### Десктоп-версия не запускается
- Установите .NET Framework 4.7.2+
- Проверьте что бэкенд доступен по адресу в `ApiService.cs`
- Запустите от администратора

### Ошибка БД
```bash
python manage.py migrate
python manage.py flush  # Очистить БД
```

---

## Контакты

Для вопросов и предложений: support@icelebrate.local
