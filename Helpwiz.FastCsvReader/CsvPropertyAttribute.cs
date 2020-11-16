using System;

namespace Helpwiz.FastCsvReader
{
    /// <summary>
    /// Indicates that this property should be matched to the field with the given name in the csv file.
    /// Required for private properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CsvPropertyAttribute : Attribute
    {
        public string PropertyName { get; }

        public CsvPropertyAttribute(string propertyName = null)
        {
            PropertyName = propertyName;
        }
    }
}