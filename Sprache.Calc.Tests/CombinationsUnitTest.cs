using System.Linq;
using FluentAssertions;
using Sprache.Calc.CalcScope.Stores;
using Sprache.Calc.Models;
using Xunit;

namespace Sprache.Calc.Tests
{
    public class CombinationsUnitTest
    {
        [Theory]
        [InlineData("::A:1=='f1a1'", true)]
        [InlineData("::A:2=='f1a2'", true)]
        [InlineData("::A:2=='f1a1'", false)]
        [InlineData("(::A:1=='f1a1') or (::A:2=='f1a1')", true)]
        [InlineData("(::A:1=='f1a1') and (::A:2=='f1a2')", true)]
        [InlineData("(::A:1=='f1a1') and (::A:2=='XXa2')", false)]
        [InlineData("((::A:1=='f1a1') and Exists('::A:3')) and (:A:2=='f1a2')", false)]
        [InlineData("Exists('::A:1')", true)]
        [InlineData("!Exists('::A:1')", false)]
        [InlineData("Exists('::X:1')", false)]
        [InlineData("!Exists('::X:1')", true)]
        public void CombinationExpressionTest(string expr, bool expectedResult)
        {
            var metaData = MetadataTestDataBuilder.CreateSample();
            var functionRegister = ExpressionCalculator.CreateFunctionRegister();
            var combinations = CombinationsEnumerator.Create(metaData, new[] {"::A:1", "::A:2"});
            functionRegister.RegisterFunc<string, bool>("Exists",
                fieldRef => { return FieldExists(metaData, fieldRef); });
            var calculator = new ExpressionCalculator(functionRegister);
            var expression = calculator.ParseExpression(expr);
            var func = expression.Compile();
            bool any = false;
            var trueOnce = false;
            foreach (var combination in combinations.GetEnumerator())
            {
                var inputScope = new DictionaryBasedInputScope(
                    combination.Select(refVal => new {refVal, fieldVal = refVal.Value})
                        .ToDictionary(a => a.refVal.FieldName, a => (object) a.fieldVal));
                var result = func.Invoke(inputScope);
                trueOnce = trueOnce || (bool) result;
                any = true;
            }

            any.Should().BeTrue();
            trueOnce.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("1 + 2", 3)]
        [InlineData("1 + 2 * 5", 11)]
        [InlineData("1 + 6 / 2", 4)]
        public void PrecedenceTestDecimal(string expr, decimal expectedResult)
        {
            var calculator = new ExpressionCalculator();
            var expression = calculator.ParseExpression(expr);
            var func = expression.Compile();
            var result = func.Invoke(new EmptyInputScope());
            result.Should().BeEquivalentTo(expectedResult);
        }

        // couldn't get precedence to work with current base of Sprache.Calc
        // [Theory]
        // [InlineData("1 + 2 > 1", true)]
        // [InlineData("1 + 2 * 5 < 21", true)]
        // [InlineData("1 + 6 / 2 < 80 / 8 ", true)]
        // [InlineData("1 + 6 / 2 < 80 / 80 ", false)]
        // public void PrecedenceTestBool(string expr, bool expectedResult)
        // {
        //     var calculator = new ExpressionCalculator();
        //     var expression = calculator.ParseExpression(expr);
        //     var func = expression.Compile();
        //     var result = func.Invoke(new EmptyInputScope());
        //     result.Should().BeEquivalentTo(expectedResult);
        // }

        private static bool FieldExists(IFieldAccessor metaData, object fieldRef)
        {
            //return (fieldRef != null);
            var matches = metaData.MatchStrata(new []{fieldRef.ToString()});
            return matches.Any();
        }
    }
}
