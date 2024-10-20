//Первое задание
//Реализовать приложение, позволяющее искать некоторый набор запрещенных слов в файлах.
//Пользовательский интерфейс приложения должен позволять ввести или
//загрузить из файла набор запрещенных слов. При нажатии на кнопку «Старт»,
//приложение должно начать искать эти слова на всех доступных накопителях
//информации (жесткие диски, флешки).
//Файлы, содержащие запрещенные слова, должны быть скопированы в
//заданную папку.
//Кроме оригинального файла, нужно создать новый файл с содержимым
//оригинального файла, в котором запрещенные слова заменены на 7 повторяющихся звезд (*******).
//Также нужно создать файл отчета. Он должен содержать информацию о
//всех найденных файлах с запрещенными словами, пути к этим файлам, размер
//файлов, информацию о количестве замен и так далее. В файле отчета нужно
//также отобразить топ-10 самых популярных запрещенных слов.
//Интерфейс программы должен показывать прогресс работы приложения
//с помощью индикаторов (progress bars). Пользователь через интерфейс приложения может приостановить работу алгоритма, возобновить, полностью
//остановить.
//По итогам работы программы необходимо вывести результаты работы в
//элементы пользовательского интерфейса (нужно продумать, какие элементы
//управления понадобятся).
//Программа обязательно должна использовать механизмы многопоточности и синхронизации!
//Программа может быть запущена только в одной копии. Предусмотреть
//возможность запуска приложения из командной строки без отображения
//визуального интерфейса.

using static System.Net.Mime.MediaTypeNames;
using System.IO;
using System.Text;
using System;
using System.Threading;

class Program
{

    static List<string> words = new List<string>()
        {
            "qqq",
            "www"
        };
    static char[] symb = { ' ', ',', '.', ';', ':', '-', '!', '?' };
    record class FileReplaceCount(string path, int count);
    static List<FileReplaceCount> fileReplaceCount = new List<FileReplaceCount>();
    private static EventWaitHandle waitHandle = new ManualResetEvent(initialState: true);

    static bool flagPause;
    static async Task Main(string[] arg)
    {
        //using (CancellationTokenSource cts = new CancellationTokenSource())
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            try
            {
                Task.Run(() => Menu(cts, cts.Token));
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Поиск файлов был отменен.");
            }
        }

        GetCommand();
    }

    private static async Task GetCommand()
    {
        while (true)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.P:
                        OnPauseClick();
                        flagPause = true;
                        break;
                    case ConsoleKey.R:
                        OnResumeClick();
                        flagPause = false;
                        break;
                    default:
                        break;
                }
            }
        }
    }
    public static void OnPauseClick()
    {
        waitHandle.Reset();
    }

    public static void OnResumeClick()
    {
        waitHandle.Set();
    }


    static async Task Menu(CancellationTokenSource cts, CancellationToken cancellationToken)
    {
        Console.WriteLine("Выберите:");
        int action;


        Console.WriteLine("1 - ввести запрещенные слова");
        Console.WriteLine("2 - загрузить из файла набор запрещенных слов");
        
        Console.WriteLine("0 - выход");

      

        Console.Write("действие - ");
        while (!Int32.TryParse(Console.ReadLine(), out action) || action < 0 || action > 2)
        {
            Console.WriteLine("Не верный ввод.Введите число:");
            Console.Write("действие - ");
        }

        switch (action)
        {
            case 0:
                break;
            case 1:
                Console.WriteLine("Введите список запрещенных слов.");
                words = Console.ReadLine().Split(symb).ToList();

                Console.Clear();
                Console.WriteLine("Выберите:");
                int start;
                Console.WriteLine("1 - Старт (поиск файлов...)");
                Console.WriteLine("0 - выход");



                Console.Write("действие - ");
                while (!Int32.TryParse(Console.ReadLine(), out start) || action < 0 || action > 1)
                {
                    Console.WriteLine("Не верный ввод.Введите число:");
                    Console.Write("действие - ");
                }

                switch (action)
                {
                    case 0:
                        break;
                    case 1:
                        Console.Clear();                        
                        await GetFiles(cts, cts.Token);
                        await FileReport();
                        break;
                }
                break;


            case 2:
                Console.WriteLine("Введите путь у файлу с запрещенными словами");
                string filePath = Console.ReadLine();

                if (File.Exists(filePath))
                {
                    string[] text = File.ReadAllLines(filePath);
                    foreach (string line in text)
                    {
                        var loadedWords = line.Split(symb).ToList();
                        words.AddRange(loadedWords);


                    }
                }

                Console.Clear();
                Console.WriteLine("Выберите:");
                Console.WriteLine("1 - Старт (поиск файлов...)");
                Console.WriteLine("0 - выход");



                Console.Write("действие - ");
                while (!Int32.TryParse(Console.ReadLine(), out start) || action < 0 || action > 1)
                {
                    Console.WriteLine("Не верный ввод.Введите число:");
                    Console.Write("действие - ");
                }


                switch (action)
                {
                    case 0:
                        break;
                    case 1:
                        Console.Clear();
                        await GetFiles(cts, cts.Token);
                        await FileReport();
                        break;
                }
                break;
        }
    }




    static async Task GetFiles(CancellationTokenSource cts, CancellationToken cancellationToken)
    {
       
        var progressTask = ProgressBar(cts);
        //await Task.Delay(20000);

        var drives = DriveInfo.GetDrives();
        var  E = drives.FirstOrDefault(x => x.Name == "E:\\");
        Console.SetCursorPosition(1, 8);
        Console.CursorVisible = false;
        Console.WriteLine("Идет поиск текстовых файлов по диску E...");
       
        await Proccess(E, progressTask, cts, cancellationToken);
        //foreach (var drive in drives)
        //{
        //Console.WriteLine($"Идет поиск текстовых файлов по диску {drive}...");
        //    if (drive.IsReady == false)
        //    {
        //        continue;
        //    }
        //   await Proccess(drive, progressTask, cts, cancellationToken);
        //}
    }

    static async Task Proccess(DriveInfo drive, Task progressTask, CancellationTokenSource cts, CancellationToken cancellationToken)
    {
        List<string> files = Directory.GetFiles(drive.Name, "*.txt", new EnumerationOptions
        {
            AttributesToSkip = FileAttributes.Normal,
            IgnoreInaccessible = true,
            RecurseSubdirectories = true,
            ReturnSpecialDirectories = true,
        }).ToList();


        var fitFiles = new List<string>();


        Console.SetCursorPosition(1, 8);
        Console.WriteLine("");
        Console.WriteLine($"Нaйдено файлов:{files.Count} ");
        Console.WriteLine("Идет поиск запрещенных слов....");

        Console.WriteLine("При необходимости, можете выполнить действия: пауза(P), продолжить выполнение работы(R)");


        bool isPaused = false;


        foreach (var fn in files)
        {
            waitHandle.WaitOne();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var flag = false;
                string[] lines = File.ReadAllLines(fn);

                foreach (var line in lines)
                {
                    var allWords = line.Split(symb, StringSplitOptions.RemoveEmptyEntries);
                    if (allWords.Any(w => words.Contains(w)))
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag) fitFiles.Add(fn);
            }
            catch (Exception ex) { }
            Task.Delay(10).Wait();
        }

        try
        {
            cts.Cancel(true);
        }
        catch(Exception ex) 
        { }

        await progressTask;
        Console.Clear();


        foreach (var file in fitFiles)
        {
            await ProcessFile(file);
        }
    }

    static async Task ProcessFile(string filePath)
    {
        int count = await CountForbiddenWordsInFile(filePath);
        if (count > 0)
        {
            string[] lines = await File.ReadAllLinesAsync(filePath);
            var newLines = lines.Select(line =>
            {
                foreach (var word in words)
                {
                    if (line.Contains(word))
                    {
                        line = line.Replace(word, "*******");
                    }
                }
                return line;
            }).ToArray();

            
            var copyDir = "E:\\STEP\\Системное программирование\\examFirstTask\\CopyFiles";
            Directory.CreateDirectory(copyDir);
            string originalCopyPath = Path.Combine(copyDir, Path.GetFileName(filePath));
            string replacedCopyPath = Path.Combine(copyDir, Path.GetFileNameWithoutExtension(filePath) + "Copy" + Path.GetExtension(filePath));

            
            await File.WriteAllLinesAsync(originalCopyPath, lines);
            await File.WriteAllLinesAsync(replacedCopyPath, newLines);

            fileReplaceCount.Add(new FileReplaceCount(replacedCopyPath, count));
        }
    }

    static async Task<int> CountForbiddenWordsInFile(string filePath)
    {
        string[] lines = await File.ReadAllLinesAsync(filePath);
        int totalCount = 0;

        foreach (var line in lines)
        {
            var wordsInLine = line.Split(symb, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in wordsInLine)
            {
                if (words.Contains(word))
                {
                    totalCount++;
                }
            }
        }

        return totalCount;
    }


    static async Task FileReport()
    {
        var filePath = "E:\\STEP\\Системное программирование\\examFirstTask\\CopyFiles\\FileReport.txt";


        using (var writer = new StreamWriter(filePath, append: true))
        {
            foreach (var f in fileReplaceCount)
            {
                FileInfo fileInfo = new FileInfo(f.path);
                string info = $"Name = {f.path}, размер файла = {fileInfo.Length}, количество замененных слов = {f.count}";

                await writer.WriteLineAsync(info);
                Console.WriteLine(info);
            }


            var topWords = words.GroupBy(w => w).OrderByDescending(g => g.Count()).Take(10);
            await writer.WriteLineAsync("Топ-10 запрещенных слов:");
            Console.WriteLine("---------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("Топ-10 запрещенных слов:");
            foreach (var topWord in topWords)
            {
                await writer.WriteLineAsync($"{topWord.Key}: {topWord.Count()}");
                Console.WriteLine($"{topWord.Key}: {topWord.Count()}");
            }
        }
       
    }

    static async Task ProgressBar(CancellationTokenSource cts)
    {

        int progress = 0;
        int maxProgress = 18;

        while (!cts.IsCancellationRequested)
        {
            Console.SetCursorPosition(15, 2);
                Console.WriteLine(flagPause ? "     Пауза..." : "     Загрузка...");
                Console.SetCursorPosition(15, 3);
                Console.WriteLine("--------------------");
                Console.SetCursorPosition(15, 4);
                Console.Write("|");
                Console.Write(new string('*', progress));
                Console.SetCursorPosition(15 + maxProgress + 1, 4);
                Console.WriteLine("|");
                Console.SetCursorPosition(15, 5);
                Console.WriteLine("--------------------");

                if(!flagPause)
                    progress = (progress + 1) % (maxProgress + 1);
                await Task.Delay(500);


                Console.SetCursorPosition(15, 4);
                Console.Write("|" + new string(' ', maxProgress) + "|");
                Console.SetCursorPosition(15, 3);
                Console.WriteLine("--------------------");
                Console.SetCursorPosition(15, 5);
                Console.WriteLine("--------------------");
        }
    }
}