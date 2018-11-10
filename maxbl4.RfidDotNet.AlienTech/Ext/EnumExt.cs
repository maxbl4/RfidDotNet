using System;
using System.ComponentModel;
using System.Linq;

namespace maxbl4.RfidDotNet.AlienTech.Ext
{
    public static class EnumExt
    {
        public static string ToStringDescriptive(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name != null)
            {
                var field = type.GetField(name);
                if (field != null)
                {
                    if (Attribute.GetCustomAttribute(field,
                        typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                    {
                        return attr.Description;
                    }
                }
            }
            return name;
        }

        public static T ParseEnum<T>(string value)
        {
            var type = typeof(T);
            var names = Enum.GetNames(type);
            var exactName = names.FirstOrDefault(x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (exactName != null)
                return (T)Enum.Parse(typeof(T), exactName);
            var fields = type.GetFields();
            foreach (var f in fields)
            {
                if (Attribute.GetCustomAttribute(f, typeof(DescriptionAttribute)) is DescriptionAttribute attr
                    && string.Equals(attr.Description, value, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)Enum.Parse(typeof(T), f.Name);
                }
            }
            return (T)(object)-1;
        }
    }
}