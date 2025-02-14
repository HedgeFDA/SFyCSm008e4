using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace SFyCSm008e4;

internal class Program
{
    /// <summary>
    /// Функция реализующая запрос ввода у пользоватеся строкового значения
    /// </summary>
    /// <param name="message">Текст запроса ввода строки к пользователю</param>
    /// <returns></returns>
    static string InputStringValue(string message)
    {
        Console.Write(message);

        string? value = Console.ReadLine();

        return value == null ? "" : value;
    }

    /// <summary>
    /// Функция реализующая чтение данных из бинарного файла в список "студентов" объекты класса Student
    /// </summary>
    /// <param name="filePath">Путь к бинарному файлу (база данных студентов)</param>
    /// <returns>Список объектов класса Student</returns>
    static List<Student> ReadStudentsFromBinaryFile(string filePath)
    {
        List<Student> result = new List<Student>();

        using FileStream fileStream = new FileStream(filePath, FileMode.Open);

        BinaryReader reader = new BinaryReader(fileStream);

        while (fileStream.Position < fileStream.Length)
        {
            Student student = new Student();

            student.Name = reader.ReadString();
            student.Group = reader.ReadString();
            student.DateOfBirth = DateTime.FromBinary(reader.ReadInt64());
            student.AverageScore = reader.ReadDecimal();

            result.Add(student);
        }

        fileStream.Close();

        return result;
    }

    /// <summary>
    /// Метод сохраняющий данные студентов в текстовые файлы из списка "студентов", в каталог с разделением по группам
    /// </summary>
    /// <param name="outputPath">Путь к каталогу в который будут сохранены текстовые файлы по студентам</param>
    /// <param name="students">Список "студентов" объектов класса Student</param>
    static void SaveStudentsToTextFiles(string outputPath, List<Student> students)
    {
        // Словарь, распределения студентов по группам (структура формируемых файлов)
        var groupStudents = new Dictionary<string, List<Student>>();

        // Разделим список студетов по группам
        foreach (var student in students)
        {
            if (!groupStudents.ContainsKey(student.Group))
                groupStudents[student.Group] = new List<Student>();

            groupStudents[student.Group].Add(student);
        }

        // Сохранение каждой группы в отдельный файл
        foreach (var group in groupStudents)
        {
            StreamWriter streamWriter = new StreamWriter(Path.Combine(outputPath, $"{group.Key}.txt"));

            foreach (var student in group.Value)
                streamWriter.WriteLine("{0}, {1}, {2}", student.Name, student.DateOfBirth.ToString("yyyy-MM-dd"), student.AverageScore);

            streamWriter.Close();
        }
    }

    /// <summary>
    /// Процедура реализующая основной алгоритм работы программы по формированию файлов на основании банарного файла.
    /// </summary>
    /// <param name="filePath">Путь к бинарному файлу (база данных студентов)</param>
    static void PerformLoad(string filePath)
    {
        // Утверджение пути к бинарному файлу с проверкой переданного/введенного значения на корректность
        while (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            if (!string.IsNullOrEmpty(filePath)) Console.WriteLine("Файл \"{0}\" не найден", filePath);

            filePath = InputStringValue("Укажите путь к файлу базы данных студентов (для отмены введите пустое значение): ");

            if (string.IsNullOrEmpty(filePath))
                return;
        }

        // Каталог на рабочем столе для сохранения тектовых файлов
        string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Students");

        try
        {
            // Если каталога нет, то создаем его
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            // Чтение данных из бинарного файла
            List<Student> students = ReadStudentsFromBinaryFile(filePath);

            // Группировка студентов по группам и сохранение в текстовые файлы
            SaveStudentsToTextFiles(outputPath, students);

            Console.WriteLine("Данные успешно обработаны и сохранены.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    /// <summary>
    /// Главная точка входа приложения
    /// </summary>
    /// <param name="args">Аргументы командной строки при запуске приложения.</param>    
    static void Main(string[] args)
    {
        string folderPath = string.Empty;

        if (args.Length > 0)
            folderPath = args[0];

        PerformLoad(folderPath);

        Console.WriteLine("\nВыполнение программы завершено.");
    }
}
