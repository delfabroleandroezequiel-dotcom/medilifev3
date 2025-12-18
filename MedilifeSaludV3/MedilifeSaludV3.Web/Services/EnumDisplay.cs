using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MedilifeSaludV3.Web.Services;

public static class EnumDisplay
{
    public static string GetDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var attr = member?.GetCustomAttribute<DisplayAttribute>();
        return attr?.GetName() ?? value.ToString();
    }
}
