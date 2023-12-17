using System.Data;
using System.Reflection;

namespace TestLibrary.Data.Ext
{
    public static class Mapper
    {
        public static T Map<T>(this IDataReader reader) where T : class, new()
        {
            var entity = new T();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (props.TryGetValue(reader.GetName(i), out var property))
                {
                    if (property != null && property.CanWrite && !reader.IsDBNull(i))
                    {
                        property.SetValue(entity, reader.GetValue(i));
                    }
                }
            }
            return entity;
        }
    }
}
