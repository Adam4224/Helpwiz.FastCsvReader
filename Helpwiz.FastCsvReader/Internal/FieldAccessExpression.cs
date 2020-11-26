using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Helpwiz.FastCsvReader.Internal
{
    internal abstract class FieldAccessExpression<T>
    {
        private static readonly FieldAccessExpression<T> empty = new EmptyAccessExpression<T>();
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Lazy<Dictionary<string,PropertyInfo>> typeProperties = new Lazy<Dictionary<string,PropertyInfo>>(GetProperties);

        private static Dictionary<string, PropertyInfo> GetProperties()
        {
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .Where(t => t.GetCustomAttribute<CsvIgnoreAttribute>() == null)
                .Concat(typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                    .Where(t => t.GetCustomAttribute<CsvPropertyAttribute>() != null))
                .Where(t => t.CanWrite)
                .ToArray();
            var ret = new Dictionary<string, PropertyInfo>();
            foreach (var prop in props.Where(t => t.GetCustomAttributes<CsvPropertyAttribute>().Any()))
            {
                foreach (var attribute in prop.GetCustomAttributes<CsvPropertyAttribute>())
                {
                    if (attribute.PropertyName == null)
                    {
                        CheckName(ret, prop.Name);
                        ret[prop.Name] = prop;
                    }
                    else
                    {
                        CheckName(ret, attribute.PropertyName);
                        ret[attribute.PropertyName] = prop;
                    }
                }
            }

            foreach (var prop in props.Where(t => !t.GetCustomAttributes<CsvPropertyAttribute>().Any()))
            {
                CheckName(ret, prop.Name);
                ret[prop.Name] = prop;
            }

            return ret;
        }

        private static void CheckName(Dictionary<string, PropertyInfo> ret, string name)
        {
            if (ret.ContainsKey(name))
            {
                throw new InvalidOperationException($"Duplicate definition in [CsvProperty] attribute for property {name}");
            }
        }

        public abstract void Assign(T value, string csvRecord);

        public static FieldAccessExpression<T> Create(string name, IConverterSpec converterSpec)
        {
            typeProperties.Value.TryGetValue(name, out var member);
            if (member == null)
            {
                if (typeof(IAdditionalColumns).IsAssignableFrom(typeof(T)))
                {
                    var type = typeof(AdditionalColumnsAccessExpression<>);
                    var genericAccess = type.MakeGenericType(typeof(T));
                    var instance = (FieldAccessExpression<T>) Activator.CreateInstance(genericAccess, name);
                    return instance;
                }
                return empty;
            }

            if (!converterSpec.HasConverter(member.PropertyType))
            {
                throw new ArgumentException($"Provided IConverterSpec doesn't implement conversion from string to {member.PropertyType}", nameof(converterSpec));
            }

            var generic = typeof(FieldAccessExpression<,>).MakeGenericType(typeof(T), member.PropertyType);
            return (FieldAccessExpression<T>)Activator.CreateInstance(generic, member, converterSpec);
        }
    }

    internal sealed class FieldAccessExpression<T, TProperty> : FieldAccessExpression<T>
    {
        private readonly Action<T, TProperty> assignAction;
        private readonly Func<string, TProperty> convertAction;

        public FieldAccessExpression(PropertyInfo member, IConverterSpec converterSpec)
        {
            convertAction = converterSpec.GetConverter<TProperty>();
            var instance = Expression.Parameter(typeof(T), "instance");
            var propertyValue = Expression.Parameter(member.PropertyType, "propertyValue");

            var body = Expression.Assign(Expression.Property(instance, member.Name), propertyValue);
            assignAction = Expression.Lambda<Action<T, TProperty>>(body, instance, propertyValue).Compile();
        }

        public override void Assign(T instance, string csvRecord)
        {
            assignAction(instance, convertAction(csvRecord));
        }
    }
}