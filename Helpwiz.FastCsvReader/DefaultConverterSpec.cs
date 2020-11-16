using System;
using System.Collections.Generic;
using System.Globalization;

namespace Helpwiz.FastCsvReader
{
    /// <summary>
    /// A default implementation of <see cref="IConverterSpec"/> that deals with string, int, double, Datetime, int?, double? and DateTime?
    /// Can be modified or extended by calling <see cref="SetConverter{T}"/>.
    /// </summary>
    public sealed class DefaultConverterSpec : IConverterSpec
    {
        private readonly Dictionary<Type, object> converterDictionary = new Dictionary<Type, object>();

        public DefaultConverterSpec()
        {
            RegisterDefaultConverters();
        }

        public bool HasConverter(Type destinationType)
        {
            return converterDictionary.ContainsKey(destinationType);
        }

        public Func<string, T> GetConverter<T>()
        {
            if (!converterDictionary.TryGetValue(typeof(T), out var ret))
            {
                throw new InvalidOperationException($"No Converter Registered for Type {typeof(T).Name}");
            }

            return (Func<string, T>) ret;
        }

        public DefaultConverterSpec SetConverter<T>(Func<string, T> converter)
        {
            converterDictionary[typeof(T)] = converter;
            return this;
        }

        internal void RegisterDefaultConverters()
        {
            SetConverter(t => t);   //String
            SetConverter(t =>       //Integer
            {
                if (!int.TryParse(t, out var ret)) throw new ArgumentException($"Value {t} does not parse to an integer");
                return ret;
            });
            SetConverter(t =>       //Double
            {
                if (!double.TryParse(t, NumberStyles.Any, CultureInfo.CurrentCulture, out var ret)) throw new ArgumentException($"Value {t} does not parse to a double");
                return ret;
            });
            SetConverter(t =>       //DateTime
            {
                if (!DateTime.TryParse(t, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var ret)) throw new ArgumentException($"Value {t} does not parse to a DateTime in the current culture");
                return ret;
            });
            SetConverter(t =>       //int?
            {
                if (string.IsNullOrWhiteSpace(t)) return (int?) null;
                if (!int.TryParse(t, out var ret)) throw new ArgumentException($"Value {t} does not parse to an integer");
                return ret;
            });
            SetConverter(t =>       //double?
            {
                if (string.IsNullOrWhiteSpace(t)) return (double?)null;
                if (!double.TryParse(t, NumberStyles.Any, CultureInfo.CurrentCulture, out var ret)) throw new ArgumentException($"Value {t} does not parse to a double");
                return ret;
            });
            SetConverter(t =>       //DateTime?
            {
                if (string.IsNullOrWhiteSpace(t)) return (DateTime?)null;
                if (!DateTime.TryParse(t, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var ret)) throw new ArgumentException($"Value {t} does not parse to a DateTime in the current culture");
                return ret;
            });
        }
    }
}