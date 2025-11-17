using TLDExtract;

namespace TLDExtractTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void UriExtract()
        {
            ExtractResult x = TLDExtractor.Extract(new Uri("https://www.seraphim.escsoft.de"));
            if (x is ExtractResult result)
            {
                Assert.Pass(result.ToString());
            }
            Assert.Fail();
        }

        [Test]
        public void UrlExtract()
        {
            ExtractResult x = TLDExtractor.Extract("seraphim.escsoft.de");
            if (x is ExtractResult result)
            {
                Assert.Pass(result.ToString());
            }
            Assert.Fail();
        }

        [Test]
        public void FailTestUriNotConform()
        {
            Assert.Throws<UriFormatException>(() =>
            {
                ExtractResult x = TLDExtractor.Extract(new Uri("seraphim.escsoft.de"));
            });
        }

        [Test]
        public void FailTestUrlNotConform()
        {
            Assert.Throws<TLDExtractException>(() =>
            {
                ExtractResult x = TLDExtract.TLDExtractor.Extract(new Uri("https://escsoft.deeee"));
            });
        }

        [Test]
        public void TryExtractUrlStringTest()
        {
            var x = TLDExtract.TLDExtractor.TryExtract("escsoft.de", out var y);

            if (x && y is ExtractResult)
                Assert.Pass(y.ToString());

            Assert.Fail();
        }

        [Test]
        public void TryExtractFailString()
        {
            var x = TLDExtract.TLDExtractor.TryExtract("def.url1", out var y);

            if (!x && y is null)
                Assert.Pass();

            Assert.Fail();
        }

        [Test]
        public void TryExtractUriTest()
        {
            var x = TLDExtract.TLDExtractor.TryExtract(new Uri("https://escsoft.co.uk"), out var y);
            if (x && y is ExtractResult)
            {
                Assert.Pass(y.ToString());
            }
            Assert.Fail();
        }
    }
}
