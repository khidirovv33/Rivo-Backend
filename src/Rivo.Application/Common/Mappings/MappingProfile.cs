using Mapster;

namespace Rivo.Application.Common.Mappings;

/// <summary>Точка регистрации Mapster-конфигов. Каждый модуль при необходимости добавляет свой IRegister.</summary>
public static class MappingProfile
{
    public static void Configure()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(MappingProfile).Assembly);
    }
}
