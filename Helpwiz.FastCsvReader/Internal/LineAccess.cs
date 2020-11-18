using System;
using System.Linq;

namespace Helpwiz.FastCsvReader.Internal
{
    internal sealed class LineAccess<T>
    {
        private readonly FieldAccessExpression<T>[] fields;

        public LineAccess(string[] headerFields, IConverterSpec spec)
        {
            fields = headerFields.Select(t => FieldAccessExpression<T>.Create(t, spec)).ToArray();
        }

        public bool CanRead(string[] line)
        {
            return true;
        }

        public T Read(string[] line, Func<T> createFunc)
        {
            if (line.Length > fields.Length)
            {
                throw new InvalidOperationException($"Line is longer than header: {string.Join(",", line)}");
            }
            var ret = createFunc();
            for (var i = 0; i < line.Length; i++)
            {
                fields[i].Assign(ret, line[i]);
            }

            for (var i = line.Length; i < fields.Length; i++)
            {
                fields[i].Assign(ret, "");
            }

            return ret;
        }
    }
}