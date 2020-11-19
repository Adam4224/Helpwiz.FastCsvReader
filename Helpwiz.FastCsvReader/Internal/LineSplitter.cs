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

        public string[] Split(string lineInput, List<char[]> charLists, List<char> current)
        {
            var quoteOpen = false;
            var countBetweenQuotes = int.MinValue;
            var lastCh = (char) 0;
            foreach (var ch in lineInput)
            {
                var matchesQuote = ch == '\"';
                if (ch == separator && !quoteOpen)
                {
                    charLists.Add(current.ToArray());
                    current.Clear();
                    lastCh = (char) 0;
                    countBetweenQuotes = int.MinValue;
                }
                else
                {
                    if (!matchesQuote)
                    {
                        current.Add(ch);
                        lastCh = ch;
                        countBetweenQuotes++;
                    }
                    //Allow \" as a quote character.
                    else if (lastCh == '\\')
                    {
                        current[current.Count - 1] = ch;
                    }
                    else if (!quoteOpen)
                    {
                        if (countBetweenQuotes == 0)
                        {
                            current.Add('\"');
                            countBetweenQuotes = int.MinValue;
                        }
                        else
                        {
                            quoteOpen = true;
                            countBetweenQuotes = 0;
                        }
                    }
                    else
                    {
                        //Allow "" as a " character, but prevent ,"", from being interpreted as ,",...
                        if (countBetweenQuotes == 0 && current.Count > 0)
                        {
                            current.Add('\"');
                            countBetweenQuotes = int.MinValue;
                        }
                        else
                        {
                            quoteOpen = false;
                            countBetweenQuotes = 0;
                        }
                    }
                }
            }

            charLists.Add(current.ToArray());

            return charLists.Select(t => new string(t)).ToArray();
        }
    }
}