using System.Collections.Generic;
using System.Linq;

namespace Helpwiz.FastCsvReader.Internal
{
    internal sealed class LineSplitter
    {
        private readonly char separator;

        public LineSplitter(char separator)
        {
            this.separator = separator;
        }

        public string[] Split(string lineInput)
        {
            var charLists = new List<char[]>();
            var current = new List<char>();
            var quoteOpen = false;
            char symmetricQuote = (char)0;
            var quotes = lineInput.Any(t => t == '\"' || t == '\'');
            foreach (var ch in lineInput)
            {
                var matchesQuote = quotes && ((!quoteOpen && (ch == '\'' || ch == '\"')) || (quoteOpen && ch == symmetricQuote));
                if (ch == separator && (!quotes || !quoteOpen))
                {
                    charLists.Add(current.ToArray());
                    current.Clear();
                }
                else
                {
                    if (!matchesQuote)
                    {
                        current.Add(ch);
                    }
                    else if (!quoteOpen)
                    {
                        quoteOpen = true;
                        symmetricQuote = ch;
                    }
                    else
                    {
                        quoteOpen = false;
                    }
                }
            }

            charLists.Add(current.ToArray());

            return charLists.Select(t => new string(t)).ToArray();
        }
    }
}