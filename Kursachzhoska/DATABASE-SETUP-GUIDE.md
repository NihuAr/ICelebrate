# 🗄️ Руководство по подключению PostgreSQL к приложению

## ✅ Что сделано

Я успешно интегрировал PostgreSQL базу данных в ваше приложение WPF. Вот что было реализовано:

### 📦 Установленные компоненты
- ✅ NuGet пакет **Npgsql 9.0.4** для работы с PostgreSQL
- ✅ SQL скрипт для создания всех таблиц БД
- ✅ Недостающие модели: `EventCategory`, `Organizer`, `Notification`, `Favorite`
- ✅ Сервис `DatabaseService` для работы с БД
- ✅ Обновлен `DataService` с поддержкой асинхронных операций

### 🎯 Интегрированные страницы

#### 1. **RegisterPage** (Регистрация)
- Сохранение имени, фамилии, даты рождения
- Сохранение email, пароля, номера телефона
- Сохранение выбранных интересов (категорий)

#### 2. **LoginPage** (Вход)
- Проверка email и пароля через БД
- Загрузка данных пользователя и его интересов

#### 3. **CatalogPage** (Каталог мероприятий)
- Загрузка мероприятий из БД
- Фильтрация по категориям (All, Sport, Books, IT, Music, Art, Cinema, Languages)
- Добавление/удаление избранных (кнопка ♥)
- Проверка статуса избранного для каждого мероприятия

#### 4. **FavoritesPage** (Избранное)
- Показ избранных мероприятий пользователя
- Удаление из избранного
- Пустое состояние если нет избранных

#### 5. **EventHistoryPage** (История)
- Показ прошедших мероприятий
- Только те мероприятия, в которых пользователь участвовал

#### 6. **NotificationsPage** (Уведомления)
- Показ уведомлений о приглашениях
- Сортировка по месяцам (October, September, etc.)
- Кнопка **"I confirm"** - принять приглашение → добавляется в участники → переносится на вкладку Affiche
- Кнопка **"I don't"** - отклонить приглашение → удаляется уведомление
- Кнопка **"Clear all"** - удалить все уведомления

#### 7. **ProfilePage** (Профиль)
- Загрузка данных профиля (email, phone, full name, date of birth)
- Удаление аккаунта с подтверждением
- Выход из аккаунта

---

## 🚀 Быстрый старт (3 шага)

### Шаг 1: Установите PostgreSQL

1. Скачайте PostgreSQL: https://www.postgresql.org/download/windows/
2. Установите (запомните пароль для пользователя `postgres`)
3. Оставьте порт по умолчанию: `5432`

### Шаг 2: Создайте базу данных и выполните SQL скрипт

**Через pgAdmin:**
1. Откройте pgAdmin
2. Создайте базу данных `kursachzhoska`
3. Откройте Query Tool
4. Откройте файл `Kursachzhoska\Database\database-schema.sql`
5. Выполните скрипт (F5)

**Через командную строку:**
```bash
psql -U postgres
CREATE DATABASE kursachzhoska;
\q

cd "C:\Users\User\source\repos\Kursachzhoska\Kursachzhoska\Database"
psql -U postgres -d kursachzhoska -f database-schema.sql
```

### Шаг 3: Настройте строку подключения

Откройте файл `Kursachzhoska\Services\DatabaseService.cs` и измените пароль:

```csharp
private string GetConnectionString()
{
    // Измените ВАШ_ПАРОЛЬ на реальный пароль PostgreSQL
    return "Host=localhost;Port=5432;Database=kursachzhoska;Username=postgres;Password=ВАШ_ПАРОЛЬ";
}
```

**Готово!** 🎉 Запустите приложение.

---

## 🧪 Тестовые данные

После выполнения SQL скрипта у вас будут:

**Тестовый пользователь:**
- Email: `katieyka@gmail.com`
- Пароль: `password123`

**Категории мероприятий:**
- Sport, Music, Art, Books, IT, Cinema, Languages

**9 примеров мероприятий:**
- Hatha yoga, Readers club, Exhibition «Art in lines», English speaking club, Stretching, Street music concert, Music concert, Orchestral music concert, Guitar masterclass

---

## 📊 Структура базы данных

Схема БД создана согласно предоставленной диаграмме:

```
User ←→ User_preferences ←→ Event_category
  ↓                              ↓
Participant ←→ Event ←→ Organizer
  ↓            ↓
Review    Contractor_on_event ←→ Contractor
                                    ↓
                                Document

Дополнительные таблицы:
- Favorites (user_id, event_id)
- Notifications (user_id, event_id, message, created_at)
```

---

## 🔄 Режим работы

Приложение автоматически определяет доступность БД:

- ✅ **PostgreSQL доступна** → использует реальную БД
- ❌ **PostgreSQL недоступна** → использует mock данные (тестовый режим)

При запуске проверьте консоль:
- Если видите сообщение `"Using mock data - database connection failed"` → проверьте настройки подключения

---

## 📖 Полная документация

Подробная документация с решением проблем:
- `Kursachzhoska\Database\README.md` - полное руководство
- `Kursachzhoska\Database\connection-config.txt` - настройка подключения
- `Kursachzhoska\Database\database-schema.sql` - SQL скрипт

---

## 🛠️ Решение проблем

### Проблема: "connection failed"

**Решение:**
1. Проверьте, что PostgreSQL запущен (Диспетчер задач → Службы)
2. Проверьте пароль в `DatabaseService.cs`
3. Проверьте, что база данных `kursachzhoska` существует

### Проблема: "table does not exist"

**Решение:**
1. Выполните SQL скрипт `database-schema.sql`
2. Убедитесь, что подключены к БД `kursachzhoska`, а не к `postgres`

---

## 📝 Примечания

1. **Безопасность:** В текущей версии пароли хранятся в открытом виде. Для production рекомендуется использовать хеширование (bcrypt, SHA-256).

2. **Конфигурация:** Строка подключения сейчас в коде. Рекомендуется вынести в `app.config` или переменные окружения.

3. **Резервное копирование:**
   ```bash
   pg_dump -U postgres -d kursachzhoska > backup.sql
   ```

4. **Восстановление:**
   ```bash
   psql -U postgres -d kursachzhoska < backup.sql
   ```

---

## ✨ Особенности реализации

- **Асинхронные операции:** Все обращения к БД асинхронны для лучшей производительности
- **Fallback режим:** При недоступности БД приложение работает с mock данными
- **Индексы:** Созданы индексы для быстрого поиска по email, датам, категориям
- **Каскадное удаление:** При удалении пользователя автоматически удаляются все связанные данные
- **Проверка дубликатов:** UNIQUE ограничения предотвращают дублирование данных

---

**Успешной разработки!** 🚀

Если возникнут вопросы, смотрите полную документацию в `Kursachzhoska\Database\README.md`

