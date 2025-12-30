using CsvHelper.Configuration;

namespace Distributor_abstractions;

public interface ICsvDtoMapperProvider
{
    /// <summary>
    /// Метод получения маппера для парсинга данных на основе dto модели
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Конкретный инстанс маппера</returns>
    ClassMap GetMap(Type type);
}
