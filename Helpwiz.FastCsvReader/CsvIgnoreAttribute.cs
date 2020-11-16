using System;

namespace Helpwiz.FastCsvReader
{
    /// <summary>
    /// Indicates that a public property should be ignored by the reader.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CsvIgnoreAttribute : Attribute
    {
    }
}