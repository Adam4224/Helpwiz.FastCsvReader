using System;

namespace Helpwiz.FastCsvReader.Internal
{
    internal sealed class AdditionalColumnsAccessExpression<T> : FieldAccessExpression<T>
        where T : IAdditionalColumns
    {
        private readonly Action<T, string> assignAction;

        public AdditionalColumnsAccessExpression(string fieldName)
        {
            assignAction = (t, s) => t.WriteColumn(fieldName, s);
        }

        public override void Assign(T value, string headerName, string csvRecord)
        {
            assignAction(value, csvRecord);
        }
    }
}