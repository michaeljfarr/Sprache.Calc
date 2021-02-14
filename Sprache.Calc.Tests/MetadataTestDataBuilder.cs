using Sprache.Calc.Models;

namespace Sprache.Calc.Tests
{
    public static class MetadataTestDataBuilder
    {
        public static MetaData CreateSample()
        {
            var testData = new MetaData();
            testData.Add($"/f1/A/1/f1a1");//::A1
            testData.Add($"/f1/A/2/f1a2");//::A2
            testData.Add($"/f1/A/3/f1a3");//::A3
            testData.Add($"/f1/B/1/f1b1");//::B1
            testData.Add($"/f1/C/1/f1c1");//::C1
            testData.Add($"/f1/D/1/f1d1");//::D1
            testData.Add($"/f2/A/1/f2a1");//::A1 (f2)
            testData.Add($"/f3/A/1/f3a1");//::A1 (f3)
            testData.Add($"/f4/A/1/f4a1");//::A1 (f4)

            return testData;
        }
    }
}