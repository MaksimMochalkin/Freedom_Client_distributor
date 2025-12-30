namespace Distributor_abstractions;

public interface ICsvSchemaValidator
{
    /// <summary>
    /// Метод валидации структуры источника данных
    /// </summary>
    /// <param name="dtoType"></param>
    /// <param name="header"></param>
    void Validate(Type dtoType, string[] header);
}
