using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Tools
{
    public static class EnumHelper
    {
        private static readonly Dictionary<Type, object> cache;

        static EnumHelper()
        {
            cache = new Dictionary<Type, object>();
        }

        public static T ParseAsEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static IEnumerable<T> GetAllValues<T>()
        {
            var result = new List<T>();
            foreach (var item in Enum.GetValues(typeof(T)))
                result.Add((T)item);

            return result;
        }

        public static EnumLocalizedValue GetLocalizedValue(Type enumType, object enumValue)
        {
            if (!cache.ContainsKey(enumType))
                GetLocalizedValues(enumType);

            var content = (List<EnumLocalizedValue>)cache[enumType];
            return content.FirstOrDefault(c => c.Value.Equals(enumValue));
        }

        public static List<EnumLocalizedValue> GetLocalizedValues(Type enumType)
        {
            List<EnumLocalizedValue> result;

            if (cache.ContainsKey(enumType))
            {
                result = (List<EnumLocalizedValue>) cache[enumType];
            }
            else
            {
                result = new List<EnumLocalizedValue>();
                foreach (var item in Enum.GetValues(enumType))
                    result.Add(new EnumLocalizedValue(item, GetDisplayValue(item)));

                cache[enumType] = result;
            }

            return result;
        }

        private static string GetDisplayValue(object value)
        {
            var member = value.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == value.ToString());
            if (member != null)
            {
                var attribute = member.GetCustomAttribute<EnumDisplayAttribute>();
                if (attribute != null && !string.IsNullOrEmpty(attribute.ResourceKey))
                {
                    return StringResources.ResourceManager.GetString(attribute.ResourceKey);
                }
            }

            return value.ToString();
        }
    }
}
