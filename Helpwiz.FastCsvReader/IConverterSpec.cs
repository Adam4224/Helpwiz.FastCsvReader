using System;

namespace Helpwiz.FastCsvReader
{
    /// <summary>
    /// An interface describing string to type conversions for datatypes.
    /// </summary>
    public interface IConverterSpec
    {
        bool HasConverter(Type destinationType);

        Func<string, T> GetConverter<T>();
    }
}