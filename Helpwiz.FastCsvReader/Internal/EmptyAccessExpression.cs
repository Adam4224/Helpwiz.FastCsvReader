namespace Helpwiz.FastCsvReader.Internal
{
    internal sealed class EmptyAccessExpression<T> : FieldAccessExpression<T>
    {
        public EmptyAccessExpression()
        {
        }

        public override void Assign(T value, string headerName, string csvRecord)
        {
        }
    }
}