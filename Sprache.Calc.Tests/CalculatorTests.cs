using System.Collections.Generic;
using FluentAssertions;
using Sprache.Calc.CalcScope;
using Sprache.Calc.CalcScope.Stores;
using Sprache.Calc.ValueScopes;
using Xunit;

namespace Sprache.Calc.Tests
{
    public class CalculatorTests
    {
        private const string ConstantName = "foo";
        private readonly ExpressionCalculator _calc;
        private readonly IMutableScope _parameterSet = new DictionaryBasedMutableStore(new DictionaryBasedInputScope(new Dictionary<string, double> { { "x", 5 }, { "y", 4 } }));

        public CalculatorTests()
        {
            var functionRegister = ExpressionCalculator.CreateFunctionRegister();
            functionRegister.RegisterFunc<double, double, double>("Foo", (x, y) => x * y);
            functionRegister.RegisterFunc<string, object>("GetVar", key => _parameterSet.GetValue(key));
            _calc = new ExpressionCalculator(functionRegister);
        }

        [Fact]
        public void BasicCalc()
        {
            var expression = "Foo(GetVar('x'), :y)";
            var function = _calc.ParseExpression(expression).Compile();
            var value = function(_parameterSet);
            value.Should().Be(20);
        }

        [Fact]
        public void ResolveNumericValue()
        {
            var mutableScope = new MutableScope(new List<Models.IInput>{ } , _parameterSet);
            var xres = mutableScope.GetValue("x");
            xres.Should().Be(5);
        }

        [Fact]
        public void ResolveStringValue()
        {
            var stringValue = "adfasfertsdbsdegt;";
            var mutableScope = new MutableScope(new [] { new Models.Input(ConstantName, stringValue) }, _parameterSet);
            var basePath = (string)mutableScope.GetValue(ConstantName);
            basePath.Should().Be(stringValue);
        }

        [Fact]
        public void StoreAndRetrieve()
        {
            var stringValue = "adfasfertsdbsdegt;";
            var mutableScope = new MutableScope(new [] { new Models.Input(ConstantName, stringValue) }, _parameterSet);
            mutableScope.GetValue(ConstantName).Should().NotBeNull();

            //push folder values
            mutableScope.StoreValue("NumNewlyFoundFiles", 5);

            //Process Folder
            mutableScope.GetValue("NumNewlyFoundFiles").Should().Be(5);
        }

        [Fact]
        public void CommandModelCalculate()
        {
            var mutableScope = new MutableScope(new Models.IInput[0], _parameterSet);

            //push folder values
            mutableScope.StoreValue("NumNewlyFoundFiles", 5);
            mutableScope.StoreValue("NumUploadableFiles", 2);
            mutableScope.StoreValue("NumUploadableFiles/My", 1);

            var expression = "GetVar('NumUploadableFiles/My') + 3";
            var function = _calc.ParseExpression(expression).Compile();
            var value = function(mutableScope);
            value.Should().Be(4);
        }

        [Fact]
        public void CommandModelCalculateBool()
        {
            var mutableScope = new MutableScope(new Models.IInput[0], _parameterSet);

            //push folder values
            mutableScope.StoreValue("NumNewlyFoundFiles", 5.0);
            mutableScope.StoreValue("NumUploadableFiles", 2.0);
            mutableScope.StoreValue("NumUploadableFiles/My", 1.0);

            var expression = "(GetVar('NumUploadableFiles/My') + 3) == 4";
            var function = _calc.ParseExpression(expression).Compile();
            var value = function(mutableScope);
            value.Should().Be(true);
        }

    }
}
