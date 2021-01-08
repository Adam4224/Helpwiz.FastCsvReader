using System;
using System.Collections.Generic;
using System.Globalization;

namespace Helpwiz.FastCsvReader
{
    /// <summary>
    /// A default implementation of <see cref="IConverterSpec"/> that deals with string, int, double, Datetime, int?, double? and DateTime?
    /// Can be modified or extended by calling <see cref="SetConverter{T}"/>.
    /// </summary>
    public sealed class DefaultConverterSpec : IExtendedConverterSpec
    {
        private readonly Dictionary<Type, object> converterDictionary = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> extendedConverterDictionary = new Dictionary<Type, object>();

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


        /// <summary>
        /// Sets the converter for a given type - The function takes the value to convert and returns the typed target value.
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="converter">A function taking the value string and returning the converted element of the target type <typeparamref name="TTarget"/></param>
        /// <returns></returns>
        public DefaultConverterSpec SetConverter<TTarget>(Func<string, TTarget> converter)
        {
            var tType = typeof(TTarget);
            converterDictionary[tType] = converter;
            if (extendedConverterDictionary.ContainsKey(tType))
            {
                extendedConverterDictionary.Remove(tType);
            }
            return this;
        }

        /// <summary>
        /// Sets the converter for a given target type - The function takes the column name, the value to convert, and returns the typed target value.
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="converter">a function that takes a column name, the value to be converted, and returns an element of the target type <typeparamref name="TTarget"/></param>
        /// <returns></returns>
        public DefaultConverterSpec SetConverter<TTarget>(Func<string, string, TTarget> converter)
        {
            var tType = typeof(TTarget);
            extendedConverterDictionary[tType] = converter;
            if (converterDictionary.ContainsKey(tType))
            {
                converterDictionary.Remove(tType);
            }
            return this;
        }

        internal void RegisterDefaultConverters()
        {
            SetConverter((c, t) => t);   //String
            SetConverter((c, t) =>       //Integer
            {
                if (!int.TryParse(t, out var ret)) throw new ArgumentException($"Value {t} in Column {c} does not parse to an integer");
                return ret;
            });
            SetConverter((c, t) =>       //Double
            {
                if (!double.TryParse(t, NumberStyles.Any, CultureInfo.CurrentCulture, out var ret)) throw new ArgumentException($"Value {t} in Column {c} does not parse to a double");
                return ret;
            });
            SetConverter((c, t) =>       //DateTime
            {
                if (!DateTime.TryParse(t, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var ret)) throw new ArgumentException($"Value {t} in Column {c} does not parse to a DateTime in the current culture");
                return ret;
            });
            SetConverter((c, t) =>       //int?
            {
                if (string.IsNullOrWhiteSpace(t)) return (int?) null;
                if (!int.TryParse(t, out var ret)) throw new ArgumentException($"Value {t} in Column {c} does not parse to an integer");
                return ret;
            });
            SetConverter((c, t) =>       //double?
            {
                if (string.IsNullOrWhiteSpace(t)) return (double?)null;
                if (!double.TryParse(t, NumberStyles.Any, CultureInfo.CurrentCulture, out var ret)) throw new ArgumentException($"Value {t} in Column {c} does not parse to a double");
                return ret;
            });
            SetConverter((c, t) =>       //DateTime?
            {
                if (string.IsNullOrWhiteSpace(t)) return (DateTime?)null;
                if (!DateTime.TryParse(t, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var ret)) throw new ArgumentException($"Value {t} in Column {c} does not parse to a DateTime in the current culture");
                return ret;
            });
        }

        public bool HasExtendedConverter(Type destinationType)
        {
            return extendedConverterDictionary.ContainsKey(destinationType);
        }

        public Func<string, string, T> GetExtendedConverter<T>()
        {
            return (Func<string, string, T>) extendedConverterDictionary[typeof(T)];
        }
    }
}