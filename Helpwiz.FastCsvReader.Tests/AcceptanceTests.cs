using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Helpwiz.FastCsvReader.Tests
{
    public class AcceptanceTests
    {
        private readonly string[] testData1 =
        {
            "Area, Quota, Remarks, Notes",
            "01, Full Time,,",
            "02, Part-Time,,This is a note"
        };

        private readonly string[] testData2 =
        {
            "Area,\"Quota, Values and Results\",Remarks,Notes",
            "01, \"Full Time, Part-Time and Zero-hour\"",
            "02,Part-Time,,This is a note"
        };

        private readonly string[] testData3 =
        {
            "Area¬\"Quota, Values and Results\"¬Remarks¬Notes",
            "01¬\"Full Time, Part-Time and Zero-hour\"",
            "02¬Part-Time¬¬",
            "NotAnInt¬Part-Time¬¬",
            "¬Part-Time¬¬",
        };

        private readonly string[] testData4 =
        {
            "Code2,Name1,Notes",
            "AAA,BBB,CCC"
        };

        private readonly string[] escapedDoubleQuotes =
        {
            "BUA13CD,BUA13NM",
            "E34005174,\"My heart says \"\"I am a heart!\"\"\"",
        };

        private readonly string[] unquotedEscapedDoubleQuotes =
        {
            "BUA13CD,BUA13NM",
            "E34005174,My heart says \"\"\"\" \"\"I am a heart!\"\"",
        };

        private readonly string[] singleQuotes =
        {
            "BUA13CD,BUA13NM",
            "E34005173,\"Hart's Lane, nr Langham (Colchester) BUA\"",
        };

        private readonly string[] backslashEscapedQuotes =
        {
            "BUA13CD,BUA13NM",
            "E34005174,\"My heart says \\\"I am a heart!\\\""
        };

        private readonly string[] emptyQuotedCell =
        {
            "BUA13CD,BUA13NM",
            "\"\",\"Value\""
        };

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestReadWithCommasAllFieldsPresent()
        {
            var result = FastCsvReader.ReadAs<DefaultData>(testData1).ToArray();
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0].Area, Is.EqualTo(1));
            Assert.That(result[0].Quota, Is.EqualTo(" Full Time"));
            Assert.That(result[0].Remarks, Is.Empty);
            Assert.That(result[0].Notes, Is.Empty);
            Assert.That(result[1].Quota, Is.EqualTo(" Part-Time"));
            Assert.That(result[1].Notes, Is.EqualTo("This is a note"));
        }

        [Test]
        public void TestReadWithTrimmingConverter()
        {
            var result = FastCsvReader.ReadAs<DefaultData>(testData1, converter: ConverterSpecs.StringTrim).ToArray();
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0].Area, Is.EqualTo(1));
            Assert.That(result[0].Quota, Is.EqualTo("Full Time"));
            Assert.That(result[0].Remarks, Is.Empty);
            Assert.That(result[0].Notes, Is.Empty);
            Assert.That(result[1].Quota, Is.EqualTo("Part-Time"));
            Assert.That(result[1].Notes, Is.EqualTo("This is a note"));
        }

        [Test]
        public void TestWithContainedQuotedCommas()
        {
            var result = FastCsvReader.ReadAs<DefaultData2>(testData2, converter: ConverterSpecs.Default).ToArray();
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0].Area, Is.EqualTo(1));
            Assert.That(result[0].QuotaValuesAndResults, Is.EqualTo(" Full Time, Part-Time and Zero-hour"));
            Assert.That(result[0].Remarks, Is.Empty);
            Assert.That(result[0].Notes, Is.Empty);
            Assert.That(result[1].QuotaValuesAndResults, Is.EqualTo("Part-Time"));
            Assert.That(result[1].Notes, Is.EqualTo("This is a note"));
        }

        [Test]
        public void TestWithContainedQuotedCommasAndTrimming()
        {
            var result = FastCsvReader.ReadAs<DefaultData2>(testData2, converter: ConverterSpecs.StringTrim).ToArray();
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0].Area, Is.EqualTo(1));
            Assert.That(result[0].QuotaValuesAndResults, Is.EqualTo("Full Time, Part-Time and Zero-hour"));
            Assert.That(result[0].Remarks, Is.Empty);
            Assert.That(result[0].Notes, Is.Empty);
            Assert.That(result[1].QuotaValuesAndResults, Is.EqualTo("Part-Time"));
            Assert.That(result[1].Notes, Is.EqualTo("This is a note"));
        }

        [Test]
        public void TestWithDifferentDelimiterMissingValuesAndQuotedCommasAndBadData()
        {
            Assert.That(() => FastCsvReader.ReadAs<DefaultData2>(testData3, '¬', ConverterSpecs.StringTrim).ToArray(), Throws.Exception);
        }

        [Test]
        public void TestBadDataWithCustomParser()
        {
            var converter = new DefaultConverterSpec();
            converter.SetConverter<int?>(s =>
            {
                if (string.IsNullOrWhiteSpace(s)) return null;
                if (!int.TryParse(s, out int value)) return null;
                return value;
            });
            var result = FastCsvReader.ReadAs<DefaultData2>(testData3, '¬', converter).ToArray();
            Assert.That(result.Length, Is.EqualTo(4));
            Assert.That(result[0].Area, Is.EqualTo(1));
            Assert.That(result[0].QuotaValuesAndResults, Is.EqualTo("Full Time, Part-Time and Zero-hour"));
            Assert.That(result[0].Remarks, Is.Empty);
            Assert.That(result[0].Notes, Is.Empty);
            Assert.That(result[1].QuotaValuesAndResults, Is.EqualTo("Part-Time"));
            Assert.That(result[1].Notes, Is.Empty);
            Assert.That(result[2].Area, Is.Null);
            Assert.That(result[2].QuotaValuesAndResults, Is.Not.Null.Or.Empty);
            Assert.That(result[3].Area, Is.Null);
        }

        [Test]
        public void TestMultiAttribute()
        {
            var result = FastCsvReader.ReadAs<MultiAttr>(testData4).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0].Code, Is.EqualTo("AAA"));
            Assert.That(result[0].Name, Is.EqualTo("BBB"));
            Assert.That(result[0].Notes, Is.EqualTo("CCC"));
        }

        [Test]
        public void TestAdditionalColumns()
        {
            var result = FastCsvReader.ReadAs<AdditionalColumnsData>(testData4).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0].Code, Is.EqualTo("AAA"));
            Assert.That(result[0].Columns.Keys.Count, Is.EqualTo(2));
            Assert.That(result[0].Columns.ContainsKey("Name1"), Is.EqualTo(true));
            Assert.That(result[0].Columns.ContainsKey("Notes"), Is.EqualTo(true));
            Assert.That(result[0].Columns["Name1"], Is.EqualTo("BBB"));
            Assert.That(result[0].Columns["Notes"], Is.EqualTo("CCC"));
        }

        [Test]
        public void TestMixedQuotesDoesntThrow()
        {
            Assert.That(() => FastCsvReader.ReadAs<BuaData>(escapedDoubleQuotes).ToArray(), Throws.Nothing);
        }

        [Test]
        public void TestSingleQuotes()
        {
            var result = FastCsvReader.ReadAs<BuaData>(singleQuotes).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Hart's Lane, nr Langham (Colchester) BUA"));
            
        }

        [Test]
        public void TestDoubleEscapedQuotes()
        {
            var result = FastCsvReader.ReadAs<BuaData>(escapedDoubleQuotes).ToArray();
            Assert.That(result[0].Name, Is.EqualTo("My heart says \"I am a heart!\""));
        }

        [Test]
        public void TestUnquotedDoubleEscapedQuotes()
        {
            var result = FastCsvReader.ReadAs<BuaData>(unquotedEscapedDoubleQuotes).ToArray();
            Assert.That(result[0].Name, Is.EqualTo("My heart says \"\" \"I am a heart!\""));
        }

        [Test]
        public void TestBackslashEscapedQuotes()
        {
            var result = FastCsvReader.ReadAs<BuaData>(backslashEscapedQuotes).ToArray();
            Assert.That(result[0].Name, Is.EqualTo("My heart says \"I am a heart!\""));
        }

        [Test]
        public void TestEmptyQuotedCell()
        {
            var result = FastCsvReader.ReadAs<BuaData>(emptyQuotedCell).ToArray();
            Assert.That(result[0].Code, Is.EqualTo(string.Empty));
            Assert.That(result[0].Name, Is.EqualTo("Value"));
        }

        public class MultiAttr
        {
            [CsvProperty("Code1")]
            [CsvProperty("Code2")]
            [CsvProperty("Code3")]
            public string Code { get; set; }

            [CsvProperty("Name1")]
            public string Name { get; set; }

            public string Notes { get; set; }
        }

        public class DefaultData2
        {
            public int? Area { get; set; }

            [CsvProperty("Quota, Values and Results")]
            public string QuotaValuesAndResults { get; set; }

            public string Remarks { get; set; }

            public string Notes { get; set; }
        }

        public class AdditionalColumnsData : IAdditionalColumns
        {
            public readonly Dictionary<string, string> Columns = new Dictionary<string, string>();

            [CsvProperty("Code1")]
            [CsvProperty("Code2")]
            [CsvProperty("Code3")]
            public string Code { get; set; }

            public void WriteColumn(string header, string value)
            {
                Columns.Add(header, value);
            }
        }

        private class DefaultData
        {
            public int? Area { get; set; }

            public string Quota { get; set; }

            public string Remarks { get; set; }

            public string Notes { get; set; }
        }

        private class BuaData
        {
            [CsvProperty("BUA13CD")]
            public string Code { get; set; }

            [CsvProperty("BUA13NM")]
            public string Name { get; set; }
        }
    }
}