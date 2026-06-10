using System;
using System.IO;
using Microsoft.Win32;

namespace Kursachzhoska.Helpers
{
    public static class FileHelper
    {
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "When2Meet"
        );
        
        private static readonly string ImagesPath = Path.Combine(AppDataPath, "Images");
        private static readonly string DocumentsPath = Path.Combine(AppDataPath, "Documents");
        
        static FileHelper()
        {
            // Создаем папки, если их нет
            Directory.CreateDirectory(ImagesPath);
            Directory.CreateDirectory(DocumentsPath);
        }
        
        /// <summary>
        /// Открывает диалог выбора изображения и копирует его в папку приложения
        /// </summary>
        /// <returns>Путь к скопированному файлу или null</returns>
        public static string? SelectAndSaveImage(string? oldImagePath = null)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*",
                Title = "Select an image"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Удаляем старое изображение, если оно есть
                    if (!string.IsNullOrEmpty(oldImagePath) && File.Exists(oldImagePath))
                    {
                        try { File.Delete(oldImagePath); } catch { }
                    }
                    
                    // Генерируем уникальное имя файла
                    var extension = Path.GetExtension(openFileDialog.FileName);
                    var newFileName = $"{Guid.NewGuid()}{extension}";
                    var newFilePath = Path.Combine(ImagesPath, newFileName);
                    
                    // Копируем файл
                    File.Copy(openFileDialog.FileName, newFilePath, true);
                    
                    return newFilePath;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Открывает диалог выбора документа и копирует его в папку приложения
        /// </summary>
        /// <returns>Путь к скопированному файлу или null</returns>
        public static string? SelectAndSaveDocument()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*|PDF files (*.pdf)|*.pdf|Word files (*.doc;*.docx)|*.doc;*.docx",
                Title = "Select a document"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Генерируем уникальное имя файла
                    var fileName = Path.GetFileName(openFileDialog.FileName);
                    var extension = Path.GetExtension(openFileDialog.FileName);
                    var newFileName = $"{Guid.NewGuid()}_{fileName}";
                    var newFilePath = Path.Combine(DocumentsPath, newFileName);
                    
                    // Копируем файл
                    File.Copy(openFileDialog.FileName, newFilePath, true);
                    
                    return newFilePath;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Удаляет файл
        /// </summary>
        public static void DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch { }
        }
        
        /// <summary>
        /// Получает имя файла из полного пути
        /// </summary>
        public static string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }
    }
}

