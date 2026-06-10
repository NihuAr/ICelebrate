# iCelebrate — Как поделиться с другими людьми

## 📱 Веб-версия (для всех)

### Для себя (локальная разработка)
```bash
cd frontend
npm install
npm run dev
```
Откроется на `http://localhost:5173`

### Для друзей (облачный хостинг)

#### Вариант 1: Netlify (самый простой)
1. Соберите проект:
```bash
cd frontend
npm run build
```

2. Зарегистрируйтесь на https://netlify.com (бесплатно)

3. Перетащите папку `frontend/dist/` в Netlify

4. Получите ссылку типа `https://icelebrate-xxx.netlify.app`

5. Поделитесь ссылкой с друзьями!

#### Вариант 2: Vercel
1. Установите Vercel CLI:
```bash
npm install -g vercel
```

2. Из папки `frontend/`:
```bash
vercel
```

3. Следуйте инструкциям

4. Получите ссылку типа `https://icelebrate.vercel.app`

#### Вариант 3: GitHub Pages (бесплатно)
1. Создайте репозиторий на GitHub
2. Загрузите код
3. В Settings → Pages выберите `frontend/dist/` как source
4. Получите ссылку типа `https://username.github.io/icelebrate`

---

## 🖥️ Десктоп-версия (только Windows)

### Для себя
1. Откройте `Kursachzhoska.sln` в Visual Studio
2. Нажмите **Build → Build Solution**
3. Запустите `Kursachzhoska\bin\Release\Kursachzhoska.exe`

### Для друзей (портативная версия)

#### Шаг 1: Опубликуйте приложение
```powershell
# Откройте PowerShell в папке проекта
.\publish-desktop.ps1 -Version "1.0.0"
```

Будет создан файл `releases\iCelebrate-v1.0.0-portable.zip`

#### Шаг 2: Загрузите на облако
Выберите один из вариантов:

**GitHub Releases (рекомендуется)**
1. Создайте новый Release на GitHub
2. Загрузите `iCelebrate-v1.0.0-portable.zip`
3. Скопируйте ссылку для скачивания

**Google Drive**
1. Загрузите файл в Google Drive
2. Откройте доступ (Share → Anyone with link)
3. Скопируйте ссылку

**Яндекс.Диск**
1. Загрузите файл
2. Откройте доступ (Share)
3. Скопируйте ссылку

**OneDrive**
1. Загрузите файл
2. Нажмите Share → Copy link
3. Скопируйте ссылку

#### Шаг 3: Поделитесь инструкцией

Отправьте друзьям:
```
Привет! Скачайте iCelebrate:
[ссылка на скачивание]

Инструкция:
1. Скачайте файл iCelebrate-v1.0.0-portable.zip
2. Распакуйте в любую папку (например, C:\iCelebrate)
3. Запустите iCelebrate.exe
4. Готово! Приложение работает без установки.

Требования: Windows 7 SP1 или выше
```

---

## 🔧 Бэкенд (для продвинутых)

### Локальный бэкенд (для разработки)
```bash
cd backend
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
python manage.py migrate
python manage.py runserver
```

API на `http://localhost:8000/api/`

### Облачный бэкенд (для продакшена)

#### Вариант 1: Heroku (простой)
1. Зарегистрируйтесь на https://heroku.com
2. Установите Heroku CLI
3. Из папки `backend/`:
```bash
heroku login
heroku create icelebrate-api
git push heroku main
heroku run python manage.py migrate
```
4. API на `https://icelebrate-api.herokuapp.com/api/`

#### Вариант 2: PythonAnywhere (очень простой)
1. Зарегистрируйтесь на https://pythonanywhere.com
2. Загрузите код через Web interface
3. Настройте WSGI
4. Получите ссылку типа `https://username.pythonanywhere.com/api/`

#### Вариант 3: DigitalOcean / AWS / Azure (продвинутый)
Требует знания Linux и Docker

---

## 🌐 Полная интеграция

### Если хотите всё вместе (веб + десктоп + бэкенд):

1. **Бэкенд** → Heroku / PythonAnywhere
2. **Веб-фронтенд** → Netlify / Vercel
3. **Десктоп** → GitHub Releases

### Обновите конфигурацию:

**frontend/vite.config.js:**
```javascript
proxy: {
  '/api': 'https://icelebrate-api.herokuapp.com',  // Ваш облачный бэкенд
}
```

**Kursachzhoska/Services/ApiService.cs:**
```csharp
private const string BaseUrl = "https://icelebrate-api.herokuapp.com/api/";
```

---

## 📊 Сравнение вариантов

| Вариант | Сложность | Стоимость | Скорость | Рекомендуется |
|---------|-----------|----------|---------|--------------|
| Локальный | ⭐ | Бесплатно | Быстро | Для разработки |
| Netlify | ⭐ | Бесплатно | Быстро | Веб-версия |
| Heroku | ⭐⭐ | Бесплатно* | Медленно | Бэкенд |
| GitHub Pages | ⭐ | Бесплатно | Быстро | Статический контент |
| DigitalOcean | ⭐⭐⭐ | $5/мес | Быстро | Продакшен |

*Heroku убрал бесплатный уровень в 2022 году

---

## ✅ Чек-лист для распространения

- [ ] Веб-версия собрана (`npm run build`)
- [ ] Десктоп-версия опубликована (`.\publish-desktop.ps1`)
- [ ] Файлы загружены на облако
- [ ] Ссылки скопированы
- [ ] Инструкция отправлена друзьям
- [ ] Бэкенд доступен по интернету (если нужно)
- [ ] Все конфигурации обновлены

---

## 🆘 Помощь

### Друг не может запустить веб-версию
- Проверьте что ссылка правильная
- Очистите кэш браузера (Ctrl+Shift+Delete)
- Попробуйте другой браузер

### Друг не может запустить десктоп-версию
- Проверьте что Windows 7 SP1 или выше
- Попросите запустить от администратора
- Проверьте что бэкенд доступен

### Бэкенд не работает
- Проверьте что база данных запущена
- Проверьте логи: `python manage.py runserver`
- Перезагрузите сервер

---

## 🎉 Готово!

Теперь ваши друзья могут использовать iCelebrate!
