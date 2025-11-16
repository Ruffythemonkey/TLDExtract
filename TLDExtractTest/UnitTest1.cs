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
    }
}
