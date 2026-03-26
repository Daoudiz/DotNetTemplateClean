using System.ComponentModel.DataAnnotations;

namespace DotNetTemplateClean.Application;

public static class EnumHelper
{
    public static IEnumerable<EnumItemDto> GetEnumWithDisplayNames<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => new EnumItemDto
            {
                Value = e.ToString(),
                DisplayName = e.GetType()
                    .GetMember(e.ToString())
                    .First()
                    .GetCustomAttributes(typeof(DisplayAttribute), false)
                    .Cast<DisplayAttribute>()
                    .FirstOrDefault()?.Name ?? e.ToString()
            });
    }
}
