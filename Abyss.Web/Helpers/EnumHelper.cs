using System.ComponentModel;

namespace Abyss.Web.Helpers;

public static class EnumHelper
{
    public static string GetEnumDescription(Enum value)
    {
        var fi = value.GetType().GetField(value.ToString());
        if (fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Length != 0)
        {
            return attributes.First().Description;
        }

        return value.ToString();
    }
}
