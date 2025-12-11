using System.Data;
using System.Reflection;

namespace MyORMLibrary
{
    internal class Support
    {
        public static T CycleforReaders<T>(IDataReader reader, PropertyInfo[] properties)
        {
            T instance = Activator.CreateInstance<T>();
            var columns = Enumerable.Range(0, reader.FieldCount)
                        .Select(reader.GetName)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in properties)
            {
                if (!columns.Contains(prop.Name))
                    continue;

                object value = reader.GetValue(reader.GetOrdinal(prop.Name));

                if (value == DBNull.Value || value == null)
                {
                    value = null;
                    prop.SetValue(instance, value);
                    continue;
                }

                var targetType = prop.PropertyType;
                if (Nullable.GetUnderlyingType(targetType) != null)
                {
                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                if (targetType.IsEnum && Enum.IsDefined(targetType, value))
                {
                    value = Enum.ToObject(targetType, value);
                    prop.SetValue(instance, value);
                    continue;
                }

                value = Convert.ChangeType(value, targetType);

                prop.SetValue(instance, value);
            }
            return instance;
        }
    }
}
