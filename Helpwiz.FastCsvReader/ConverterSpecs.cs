namespace Helpwiz.FastCsvReader
{
    /// <summary>
    /// Enumeration of converter specs for converting datatypes in csv files.
    /// </summary>
    public static class ConverterSpecs
    {
        public static IConverterSpec Default = new DefaultConverterSpec();
        public static IConverterSpec StringTrim = new DefaultConverterSpec().SetConverter(t => t?.Trim());
    }
}