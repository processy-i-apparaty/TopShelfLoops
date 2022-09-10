# TopShelf Loops

Шаблон проекта для разработки Windows-сервисов.
Консольное приложение .NET Framework 4.6.2

## Цель

Создавать сервисы, работающие по сценарию: работать в цикле вплоть до остановки, выполнять повторяющуюся задачу, ожидать заданный интервал времени. Также задача может быть разделена на экземпляры подзадач, выполняющихся параллельно. При параллельной работе, в случае возникновения исключения внутри одной из подзадач, должен присутствовать выбор: останавливать все подзадачи либо продолжать их выполнение. Каждая подзадача получает объект с входными данными. По окончании выполнения каждой подзадачи возвращать объект-результат выполнения, включающий в себя объект выходных данных. Должно быть настраиваемое логирование в файлы и другие источники. При остановки сервиса, оный обязан среагировать на запрос в минимальный срок и завершить работу.

## Библиотеки

- [Topshelf v4.3.0](https://topshelf.readthedocs.io/en/latest/)
- [Topshelf.Serilog v4.3.0](https://www.nuget.org/packages/Topshelf.Serilog/)
- [Serilog v2.11.0](https://serilog.net/)
- [Serilog.Sinks.Console v4.1.0](https://github.com/serilog/serilog-sinks-console)
- [Serilog.Sinks.File v5.0.0](https://github.com/serilog/serilog-sinks-file)
- [Serilog.Sinks.Notepad v2.1.0](https://github.com/serilog-contrib/serilog-sinks-notepad)
- [Westwind.Utilities v3.1.14](https://github.com/RickStrahl/Westwind.Utilities)

# Реализация

Для удобства разработки и отладки сервиса используется библиотека `TopShelf`. Создан набор шаблонов и абстракций для реализации кастомной логики.

## Класс `TopShelfLoops.Service.CustomService`

При старте сервиса выполняется метод `Start`. В качестве параметров метод принимает интерфейс кастомной логики и параметры приложения. Внутри метода создается токен отмены, обертка выполнения кастомной логики. Создается `Task` который запускает логику. 

```c#
public void Start(ICustomLogic customLogic, ApplicationConfiguration configuration)
{
    var logicWrapper = new CustomLogicWrapper(customLogic, configuration);
    _cancellationTokenSource = new CancellationTokenSource();
    _customTask = new Task(() => logicWrapper.Run(_cancellationTokenSource.Token));
    _log.Info("Starting custom logic");
    _customTask.Start();
}
```

При остановке сервиса выполняется метод `Stop()`. Метод запрашивает остановку при помощи токена отмены и ожидает завершение выполнения задачи.

```c#
public void Stop()
{
    _log.Warn("Stopping service");
    _cancellationTokenSource.Cancel();
    _customTask.Wait();
    _cancellationTokenSource.Dispose();
    _log.Info("Service stopped");
}
```

Также класс слдержит статический метод `RunService`, который конфигурирует и запускает сервис. Метод получает названия и описание сервиса, кастомную логику и конфигурацию. Данный метод вызывается из метода `Main` класса `Program`.

```c#
public static TopshelfExitCode RunService(string serviceName, string displayName, string description, ICustomLogic customLogic, ApplicationConfiguration configuration){...}
```

## Метод `TopShelfLoops.Program.Main`

Создается экземпляр кастомной логики, реализующий интерфейс `ICustomLogic`.

С помощью библиотеки `Westwind.Utilities` инициализируется конфигурация приложения. Если параметры конфигурации присутствуют в файле конфигурации, то они берутся оттуда, иначе инициализируются параметры по-умолчанию. Отстутствующие параметры автоматически записываются в файл конфигурации.

Запускается сервис и блокирует основной поток до своего завершения.

```c#
internal class Program
{
    private static void Main(string[] args)
    {
        var customLogic = new CustomLogicSample();
        var configuration = new ApplicationConfiguration();
        configuration.Initialize();
        configuration.Write();
        TopshelfExitCode rc = CustomService.RunService(
            "CustomService69",
            "CustomService69",
            "CustomService69",
            customLogic, configuration);
        var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
        Environment.ExitCode = exitCode;
    }
}
```

## Интерфейс `TopShelfLoops.Service.ICustomLogic`

Свойство `Name` хранит название, которое используется при создании Log-файлов. если свойство не инициализировано, то файлы будут иметь название по-умолчанию.

Метод `GetCustomAction` возвращает делегат, который будет вызываться в основном цикле сервиса. Входящиме параметры делегируемого метода: конфигурация приложения и токен отмены.

```c#
internal interface ICustomLogic
{
    string Name { get; }
    Action<ApplicationConfiguration, CancellationToken> GetCustomAction();
}
```


## Класс `TopShelfLoops.Service.CustomLogicWrapper`

Является оберткой для логики сервиса работает в соответствие с требованиями: получает делегат на метод кастомной логики, выполняет его и ожидает заданный в параметрах интервал времени. При запросе отмены через токен, производится выход из цикла. Для ожидания используется `ServiceHelper.Wait`. Через короткие промежутки времени он проверяет время ожидания и не произошел ли запрос отмены. Класс `TopShelfLoops.Service.ServiceHelper` также содержит другие аналогичные методы, которые, например, бросают исключение или проверяют несколько токенов отмены.

```c#
internal class CustomLogicWrapper
{
    private readonly ApplicationConfiguration _configuration;
    private readonly Action<ApplicationConfiguration, CancellationToken> _customAction;
    public CustomLogicWrapper(ICustomLogic customLogic, ApplicationConfiguration configuration)
    {
        _configuration = configuration;
        _customAction = customLogic.GetCustomAction();
    }
    public void Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _customAction.Invoke(_configuration, cancellationToken);
                ServiceHelper.Wait(_configuration.TimeToWait, cancellationToken);
            }
            catch (Exception e)
            {
                Log.Logger.Error("{0}; exception; {1}",
                    nameof(CustomLogicWrapper), e.ToString());
            }
        }
    }
}
```

## Абстрактный класс `TopShelfLoops.ParallelJob.ParallelJobBase<TIn, TOut>`

Данный класс осуществляет сценарий параллельного выполнения однотипных подзадач. Присутствует два generic-параметра: входящий(`TIn`) и исходящий(`TOut`) объекты. Класс получает через конструктор ID для идентификации подзадачи в лог-отчете и Func-делегат на метод, который будет выполнен параллельно. Данный метод в качестве входящих параметров принимает generic-объект(`TIn`), локальный токен отмены(используется для отмены подзадач) и глобальный токен отмены(для остановки сервиса). Выполнение инициируется методом `RunJob`, который получает вышеописанные параметры. Метод возвращает объект-результат выполнения `ParallelResult<TOut>` Чтобы воспользоваться логикой данного класса, нужно написать свою реализацию с конкретными типами входящего и исходящего параметров. Если входящих или исходящих параметров будет больше чем один, рекомендуется использовать объекты, которые будут содержать в себе данные параметры.

Пример реализации. Входящий параметр типа `int`, исходящий типа `string`

```c#
internal class SampleParallelJob : ParallelJobBase<int, string>
{
    public SampleParallelJob(ulong id, Func<int, CancellationToken, CancellationToken, string> job) : base(id, job)
    {
    }
}
```

## Класс `TopShelfLoops.ParallelJob.ParallelWrapper<TIn, TOut>`

Данный класс реализует механизм параллельного выполнения подзадач. В конструктор получает коллекцию объектов `ParallelJobBase<TIn, TOut>`, коллекцию входящих параметров(размеры коллекций должны быть идентичны иначе будет брошено исключение), глобальный токен отмены(сигнал на завершение работы сервиса) и флаг отмены всех подзадач при исключении хотя бы в одной из них.

Запуск подзадач инициируется методом `Start()`. По завершению всех подзадач метод возвращает коллекцию объектов-результатов `IEnumerable<ParallelResult<TOut>>`


## Класс `TopShelfLoops.Logic.CustomLogicSample`

Содержит пример работы кастомной логики. Использует `ParallelWrapper` для запуска параллельных подзадач. Их создается столько, сколько потоков указано в конфигурацции. Каждая подзадача принимает `int` и возвращает `string`. Каждая подзадача ждет случайный интервал времени (0-10 сек) и с вероятностью 1 из 10 бросает исключение. Если исключение брошено, то (в зависимости от параметра `CancelAllOnFirstFault` из конфигурации) обертка может отменить все подзадачи.

## Параметр `LogNotepad`

При реальной работе сервиса отсутствует консоль. Логи пишутся в файл и, чтобы их наблюдать в реальном времени, необходим текстовый редактор с возможностью чтения обновлений в реальном времени. Чтобы облегчить данный момент есть возможность установить параметр настроек приложения `LogNotepad` в значение `True`, открыть окно приложения Блокнот, запустить сервис и наблюдать логи в реальном времени.


# GLHF