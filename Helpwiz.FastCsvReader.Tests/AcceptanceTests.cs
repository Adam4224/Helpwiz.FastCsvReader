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

        public class DefaultData2
        {
            public int? Area { get; set; }

            [CsvProperty("Quota, Values and Results")]
            public string QuotaValuesAndResults { get; set; }

            public string Remarks { get; set; }

            public string Notes { get; set; }
        }

        private class DefaultData
        {
            public int? Area { get; set; }

            public string Quota { get; set; }

            public string Remarks { get; set; }

            public string Notes { get; set; }
        }
    }
}