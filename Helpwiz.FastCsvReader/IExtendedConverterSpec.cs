using System;

namespace Helpwiz.FastCsvReader
{
    /// <summary>
    /// Provides an extension to <see cref="IConverterSpec"/> that allows for more
    /// detailed error information.
    /// </summary>
    public interface IExtendedConverterSpec : IConverterSpec
    {
        /// <summary>
        /// Gets a value indicating whether an extended converter is available for the given type.
        /// </summary>
        bool HasExtendedConverter(Type destinationType);

        /// <summary>
        /// A function which takes the column name and the string to convert, and returns the converted type.
        /// </summary>
        Func<string, string, T> GetExtendedConverter<T>();
    }
}