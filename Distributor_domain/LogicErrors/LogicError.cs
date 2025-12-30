namespace Distributor_domain.LogicErrors;

public static class LogicError
{
    public static readonly Error CsvReaderError =
        new("CSV_READ_ERROR", "Some exception happend");

    public static readonly Error CsvLoadError =
        new("CSV_LOAD_ERROR", "Something went wrog when data loading");
}
