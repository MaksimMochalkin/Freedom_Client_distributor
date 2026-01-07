using Client_distributor;
using Destributor_services.AssigmentLogic;
using Destributor_services.DataReciverServices;
using Destributor_services.GeoServices;
using Destributor_services.PathValidator;
using Destributor_services.PiplineExecutor;
using Destributor_services.PiplineSteps;
using Destributor_services.Staps;
using Distributor_abstractions;
using Distributor_domain;
using Distributor_domain.Dtos;
using Distributor_infrastructure.CsvStructValidator;
using Distributor_infrastructure.DataSources;
using Distributor_infrastructure.DtoMappers;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        Console.WriteLine("Client distribution application");
        Console.WriteLine("Type --help to see usage.");
        Console.WriteLine("Press Ctrl+C to exit.");
        Console.WriteLine();

        // Если аргументы переданы сразу — пробуем выполнить
        if (args.Length > 0)
        {
            await TryRunAsync(args, cts.Token);
        }

        while (!cts.IsCancellationRequested)
        {
            Console.Write("> ");

            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var inputArgs = SplitArgs(line);

            await TryRunAsync(inputArgs, cts.Token);
        }

        Console.WriteLine("\nShutting down...");
        return 0;
    }

    private static async Task TryRunAsync(
        string[] args,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var parseResult = CliStartParserService.Parse(args);

            if (!parseResult.IsSuccess)
            {
                Console.WriteLine(parseResult.Error!.Message);
                return;
            }

            var options = parseResult.Value!;
            var validationResult = InputPathsValidator.ValidateInputParams(options);
            if (!validationResult.IsSuccess)
            {
                Console.WriteLine(validationResult.Error!.Message);
                return;
            }

            var geoService = InitGeoService();
            var dataFactory = InitDataSource();
            var dataReciverService = new CsvDataResiverService(dataFactory);
            var assigmentService = new ClientAssignmentService(geoService);
            var loadDataStep = new LoadDataStep(dataReciverService);
            var validationStep = new ValidationDataStep();
            var assignClientsStep = new ClientAssignmentStep(assigmentService);

            var documentsPath = Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(documentsPath, "assigment_result.json");

            var saveAssigmentResultStep = new SaveResultToJsonStep(filePath);

            var steps = new List<IAsyncPipelineStep<AssignmentContext, AssignmentContext>>
            { 
                loadDataStep, validationStep, assignClientsStep, saveAssigmentResultStep 
            };
            var pipline = new Pipeline<AssignmentContext>(steps);
            var runer = new AssigmentOrchestrator(pipline);
            var piplineResult = await runer.RunAsync(
                options,
                cancellationToken);

            if (piplineResult.IsSuccess)
                Console.WriteLine($"Processing completed successfully. The output json in your {filePath} folder");
            else
                Console.WriteLine($"Processing completed unsuccessfully. Code: {piplineResult.Error!.Code}, Message: {piplineResult.Error!.Message}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation cancelled.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error during processing:");
            Console.Error.WriteLine(ex.Message);
        }
    }

    private static string[] SplitArgs(string input)
    {
        return input
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static IGeoService InitGeoService()
    {
        var key = Environment.GetEnvironmentVariable("YANDEX_API_KEY", EnvironmentVariableTarget.User);
        if (string.IsNullOrWhiteSpace(key))
            throw new Exception("Api key is not found");

        var http = new HttpClient();
        var geoProviderFactory = new GeoProviderFactory();
        geoProviderFactory.Register(() => new MockGeoProvider());
        geoProviderFactory.Register(() => new YandexGeoProvider(http, key));
        var geoService = new GeoService(geoProviderFactory);
        return geoService;
    }

    private static IDataSourceFactory InitDataSource()
    {
        var factory = new DataSourceFactory();
        var mapperProvider = new CsvDtoMapperProvider();
        var csvSchemaValidator = new CsvSchemaValidator();
        factory.Register(new CsvDataSource<EmployeeDto>(mapperProvider, csvSchemaValidator));
        factory.Register(new CsvDataSource<BranchDto>(mapperProvider, csvSchemaValidator));
        factory.Register(new CsvDataSource<ClientDto>(mapperProvider, csvSchemaValidator));
        
        return factory;
    }
}
