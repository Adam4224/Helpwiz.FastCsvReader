namespace Helpwiz.FastCsvReader
{
    /// <summary>
    /// Implement this interface to have additional data (not modeled by properties)
    /// written to your classes.
    /// </summary>
    public interface IAdditionalColumns
    {
        void WriteColumn(string header, string value);
    }
}