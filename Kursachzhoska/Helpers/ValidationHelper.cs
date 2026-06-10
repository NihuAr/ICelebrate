using System;
using System.Text.RegularExpressions;

namespace Kursachzhoska.Helpers
{
    public static class ValidationHelper
    {
        // Email validation (только латинские буквы, цифры и разрешенные символы, без кириллицы)
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Проверка на кириллицу
                var cyrillicRegex = new Regex(@"[а-яА-ЯёЁ]");
                if (cyrillicRegex.IsMatch(email))
                    return false;

                // Проверка формата email (только латинские буквы, цифры, точки, дефисы, подчеркивания)
                var regex = new Regex(@"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        // Password validation (минимум 6 символов)
        public static bool IsValidPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
        }

        // Phone validation (начинается с + и содержит цифры)
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            var regex = new Regex(@"^\+?\d{10,15}$");
            return regex.IsMatch(phone.Replace(" ", "").Replace("-", ""));
        }

        // Name validation (только буквы)
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var regex = new Regex(@"^[a-zA-Zа-яА-ЯёЁ\s\-]+$");
            return regex.IsMatch(name) && name.Length >= 2;
        }

        // Date of birth validation (возраст от 16 до 100 лет)
        public static bool IsValidDateOfBirth(DateTime? date)
        {
            if (!date.HasValue)
                return false;

            var age = DateTime.Now.Year - date.Value.Year;
            if (date.Value > DateTime.Now.AddYears(-age))
                age--;

            return age >= 16 && age <= 100;
        }

        // Generic not empty validation
        public static bool IsNotEmpty(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}

