using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpwiz.FastCsvReader.Internal;

namespace Helpwiz.FastCsvReader
{
    /// <summary>
    /// <para>
    /// This class is used to quickly read csv or other delimited text into .Net CLR objects. It creates
    /// precompiled expressions to access the properties of the returned objects, avoiding reflection.
    /// </para>
    /// <para>
    /// To read a Csv, call <see cref="FastCsvReader"/>.<see cref="ReadAs{T}()"/>
    /// </para>
    /// </summary>
    public sealed class FastCsvReader
    {
        private readonly IConverterSpec converter;
        private string firstLine;
        private bool isStarted;
        private readonly LineSplitter splitter;
        private readonly IEnumerator<string> enumerator;
        private readonly Dictionary<Type, object> lineAccessDictionary = new Dictionary<Type, object>();

        private FastCsvReader(IEnumerable<string> file, char separator = ',', IConverterSpec converter = null)
        {
            this.converter = converter ?? new DefaultConverterSpec();
            enumerator = file.GetEnumerator();
            splitter = new LineSplitter(separator);
        }

        /// <summary>
        /// Reads a csv file to an enumeration of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file">The enumeration of strings representing the csv file.</param>
        /// <param name="separator">The separator character (default is ,)</param>
        /// <param name="converter">The type converter specification (default is null)</param>
        /// <returns>An enumeration of type <typeparamref name="T"/></returns>
        public static IEnumerable<T> ReadAs<T>(IEnumerable<string> file, char separator = ',', IConverterSpec converter = null)
            where T: new()
        {
            return new FastCsvReader(file, separator, converter).ReadAsImpl<T>();
        }

        public static IEnumerable<T> ReadAs<T>(IEnumerable<string> file, Func<T> creatorFunc, char separator = ',', IConverterSpec converter = null)
        {
            return new FastCsvReader(file, separator, converter).ReadAsImpl<T>(creatorFunc);
        }

        private LineAccess<T> GetLineAccess<T>()
        {
            if (lineAccessDictionary.TryGetValue(typeof(T), out var ret)) return (LineAccess<T>)ret;
            var first = FirstLine;
            var headerSplit = splitter.Split(firstLine).Select(t => t.Trim()).ToArray();
            var access = new LineAccess<T>(headerSplit, converter);
            lineAccessDictionary.Add(typeof(T), access);
            return access;
        }

        private string FirstLine
        {
            get
            {
                if (firstLine == null)
                {
                    if (!enumerator.MoveNext())
                    {
                        return null;
                    }
                    firstLine = enumerator.Current;
                }

                return firstLine;
            }
        }

        /// <summary>
        /// Reads the file as an enumeration of type <typeparamref name="T"/>,
        /// where <typeparamref name="T"/> is instantiated using the default constructor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private IEnumerable<T> ReadAsImpl<T>() where T : new()
        {
            return ReadAsImpl<T>(() => new T());
        }

        /// <summary>
        /// Reads the file as an enumeration of type <typeparamref name="T"/>,
        /// where each T instance is created by invoking the supplied creation function.
        /// </summary>
        /// <param name="creationFunc">The function to create new instances of type <typeparamref name="T"/></param>
        private IEnumerable<T> ReadAsImpl<T>(Func<T> creationFunc)
        {
            if (creationFunc == null) throw new ArgumentNullException(nameof(creationFunc));
            if (isStarted) throw new InvalidOperationException("Cannot enumerate multiple times");
            isStarted = true;

            var access = GetLineAccess<T>();
            if (access == null) yield break;

            while (enumerator.MoveNext())
            {
                var split = splitter.Split(enumerator.Current);
                if (!access.CanRead(split)) continue;
                
                yield return access.Read(split, creationFunc);
            }
        }
    }
}
