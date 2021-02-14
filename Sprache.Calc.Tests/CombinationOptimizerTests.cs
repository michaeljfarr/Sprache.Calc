using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Sprache.Calc.Models;
using Xunit;

namespace Sprache.Calc.Tests
{
    public class CombinationOptimizerTests
    {
        [Fact]
        public void TestCombinationsEnumerator()
        {
            var metaData = MetadataTestDataBuilder.CreateSample();
            var functionRegister = ExpressionCalculator.CreateFunctionRegister();
            var combinations = CombinationsEnumerator.Create(metaData, new[] { "::A:1", "::A:2" });
            combinations.GetEnumerator().Count().Should().Be(4);
            var a1Values = combinations.GetEnumerator().First().Select(a => a.Value).Cast<string>().ToList();
            a1Values.Should().BeEquivalentTo(new[] { "f1a1", "f1a2" });
            a1Values.Count.Should().BeGreaterThan(0);
            var a2Values = combinations.GetEnumerator().Skip(1).First().Select(a => a.Value).Cast<string>().ToList();
            a2Values.Should().BeEquivalentTo(new[] { "f2a1" });
        }

        /// <summary>
        /// Note: The actual optimizer is still in progress - back soon.
        /// </summary>
        [Fact]
        public void TestOptimizer()
        {
            var testData = MetadataTestDataBuilder.CreateSample();

            var orCombinations = new string[] { "A1", "B1" };

            //this method checks that the fields exist in the data source, and tells us how many field loops we need to consider
            //if it is just 1 field loop, we do not need to optimise anything - but there is more than 1 in this case.
            var combinationSource = CombinationsEnumerator.Create(testData, orCombinations);
            var numCombinations = combinationSource.GetEnumerator().Count();
            numCombinations.Should().Be(combinationSource.NumCombinations);


            //or if we had (C1 == 1 || C2==2)  && B1 == 1, we need to find all the combinations of B1 (assuming C1/C2 had the most combinations) and then C1/C2:
            //so if there are 50 combinations of C1/C2 and 5 Combination of B1, we need a total of 55 combinations, not 250.
            //to achieve that, we need to evaluate the combinations of B1 in isolation first - than then evaluate the combinations of C1/C2 in isolation.

            var andCombinations = new List<string[]> { new string[] { "C1", "C2" }, new string[] { "B1" } };
            var combinationSources = CombinationsEnumerator.CreateMany(testData, andCombinations);

            foreach (var combinationSource2 in combinationSources)
            {
                var numCombinations2 = combinationSource2.GetEnumerator().Count();
                numCombinations2.Should().Be(combinationSource2.NumCombinations);
            }
        }

        
    }
}
