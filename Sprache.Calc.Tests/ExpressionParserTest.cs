using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Sprache.Calc.CalcScope;
using Sprache.Calc.CalcScope.Stores;
using Xunit;

namespace Sprache.Calc.Tests
{
    public class ExpressionParserTest
    {
        private static readonly IInputScope ExampleParameterSet = new DictionaryBasedInputScope(new Dictionary<string, double> { { "x", 5 }, { "y", 4 } });
        private static readonly Dictionary<string, Dictionary<string, object>> Lookup = new Dictionary<string, Dictionary<string, object>> { { "list1", new Dictionary<string, object> { { "key1", "val1" }, { "key2", "4" } } }, { "list2", new Dictionary<string, object>() } };

        [Theory]
        [InlineData("1", 1)]
        [InlineData("(1+1)", 2)]
        [InlineData("125 / ((1+4)^(1*2))", 5)]
        [InlineData("5+125/((1+4)^(1*2))", 10)]
        public void ArithmeticInBrackets(string expression, double expectedValue)
        {
            var calc = new LogicCalculator();
            var invokableExpression = calc.ParseBoolExpression(expression, new Dictionary<string, object>());
            var value = invokableExpression.Compile()(new EmptyInputScope());
            value.Should().Be(expectedValue);
        }

        [Fact]
        public void BasicFunction()
        {
            var funcs = ExpressionCalculator.CreateFunctionRegister();
            funcs.RegisterFunc<double, double, double>("Foo", (x, y) => x * y);
            var calc = new ExpressionCalculator(funcs);
            var function = calc.ParseExpression("Foo(:x, :y)").Compile();
            function(ExampleParameterSet).Should().Be(20);
        }

        [Theory]
        [InlineData("5", 5)]
        [InlineData("5+1", 6)]
        [InlineData("5+4/2", 7)]
        [InlineData("6+6/(2+1)", 8)]
        [InlineData("Foo(x, y)", 20)]
        [InlineData("5 + Foo(x, y)*6.25 / ((1+4)^(1*2))", 10)]
        [InlineData("5 + Foo(1, 25)*5 / ((1+4)^(1*2))", 10)]
        [InlineData("5 + Foo(Foo(5, 5), 5) / ((1+4)^(1*2))", 10)]
        public void ComplexFunction(string expression, double expectedValue)
        {
            var funcs = ExpressionCalculator.CreateFunctionRegister();
            funcs.RegisterFunc<object, object, object>("Foo", (x, y)=>ToDouble(x)*ToDouble(y));
            
            var calc = new LogicCalculator(funcs);
            //var values = new Dictionary<string, object>() {{"x", 4}, {"y", 5}};
            var values = new Dictionary<string, object>() {{"x", 4}, {"y", 5}};
            var vals = new DictionaryBasedInputScope(values);
            var expr = calc.ParseBoolExpression(expression, values);
            var func = expr.Compile();
            var val = func(vals);
            ((double)val).Should().BeApproximately(expectedValue, 0.001);
        }

        double ToDouble(object v)
        {
            return ((IConvertible) v).ToDouble(CultureInfo.InvariantCulture);
        }

        [Theory]
        [InlineData("TrueConst && true", true)]
        [InlineData("false && FalseConst", false)]
        [InlineData("false && true", false)]
        [InlineData("false || true", true)]
        [InlineData("5 > 4", true)]
        [InlineData("5 < 4", false)]
        [InlineData("5 == 4", false)]
        [InlineData("5 == 5", true)]
        [InlineData("(5 == 5) || (5 == 4)", true)]
        [InlineData("(5 == 5) && (5 == (2*2))", false)]
        public void SimpleBoolean(string expression, bool expectedValue)
        {
            var calc = new LogicCalculator();
            var values = new Dictionary<string, object>() {{"TrueConst", true}, {"FalseConst", false}};
            var invokableExpression = calc.ParseBoolExpression(expression, values);
            var value = invokableExpression.Compile()(null);
            value.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("IsIn('list1', 'key1') && IsIn('list1', 'key2')", true)]
        [InlineData("IsIn('list1', 'key5') || IsIn('list1', 'key2')", true)]
        [InlineData("IsIn('list1', 'key5') && IsIn('list1', 'key2')", false)]
        public void ComplexBoolean(string expression, bool expectedValue)
        {
            var funcs = ExpressionCalculator.CreateFunctionRegister();
            funcs.RegisterFunc<string, string, bool>("IsIn", (listName, key) => Lookup[listName].ContainsKey(key));
            funcs.RegisterFunc<string, string, object>("ValOf", (listName, key) => Lookup[listName][key]);
            var calc = new ExpressionCalculator(funcs);
            

            var function = calc.ParseExpression(expression).Compile();
            function(ExampleParameterSet).Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("ValOfLookup('list1', 'key1')", "val1")]
        public void ValOfLookup(string expression, string expectedValue)
        {
            var funcs = ExpressionCalculator.CreateFunctionRegister();
            funcs.RegisterFunc<string, string, object>("ValOfLookup", (listName, key) => Lookup[listName][key]);
            var calc = new LogicCalculator(funcs);
            var invokableExpression = calc.ParseBoolExpression(expression, new Dictionary<string, object>());
            var value = invokableExpression.Compile()(null);
            value.Should().Be(expectedValue);


            // var function = calc.ParseObjectFunc(expression).Compile();
            // function(ExampleParameterSet).Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("IsParam('a', 'b', 'a')", true)]
        [InlineData("IsParam('a', 'b', 'c')", false)]
        [InlineData("IsParam(ValOfLookup('list1', 'key1'), 'b', 'val1')", true)]
        [InlineData("IsParam(6, 7, 2*3)", true)]
        [InlineData("IsParam(6, 7, 2*4)", false)]
        public void ParameterExpression(string expression, bool expectedValue)
        {

            var funcs = ExpressionCalculator.CreateFunctionRegister();
            funcs.RegisterFunc<string, string, object>("ValOfLookup", (listName, key) => Lookup[listName][key]);
            funcs.RegisterExpression2("IsParam", (parameters) =>
            {
                var values = parameters;
                var isVal = values.Take(values.Count() - 1).Any(val => object.Equals(val, values.Last()));
                return isVal;
            });
            var calc = new LogicCalculator(funcs);
            var invokableExpression = calc.ParseBoolExpression(expression, new Dictionary<string, object>());
            var value = invokableExpression.Compile()(null);
            value.Should().Be(expectedValue);

        }

        public static object ToValue(Expression expression)
        {
            var val = Expression.Lambda(expression).Compile().DynamicInvoke();
            return val;
        }

        [Theory]
        [InlineData(":List1:key1", "val1")]
        [InlineData(":List1:key1 == 'val1'", true)]
        [InlineData(":List1:FiveAsDec * 5", 25)]
        [InlineData("'cat' < 'dog'", true)]
        [InlineData("'cat' > 'dog'", false)]
        [InlineData("'cat' == 'dog'", false)]
        [InlineData("'cat' == 'cat'", true)]
        [InlineData(":List1:SixAsInt < :List1:SevenAsDouble", true)]
        [InlineData(":List1:SixAsInt * 5", 30)]
        [InlineData(":List1:SevenAsDouble * 5", 35)]
        [InlineData("(:List1:SevenAsDouble * 5) > (:List1:SixAsInt * 5)", true)]
        [InlineData("NumElements(:List1:OneTwoThree)", 3)]
        public void TestTypeCoersionAndReferences(string expression, object expectedValue)
        {
            var vals = new FuncBasedInputScope(a =>
            {
                if (a == ":List1:key1")
                    return "val1";
                else if (a == ":List1:FiveAsDec")
                {
                    return (Decimal)5;
                }
                else if (a == ":List1:SixAsInt")
                {
                    return (int)6;
                }
                else if (a == ":List1:SevenAsDouble")
                {
                    return (double)7;
                }
                else if (a == ":List1:OneTwoThree")
                {
                    return new[] { 1, 2, 3 };
                }
                else
                {
                    return ExampleParameterSet.GetValue(a);
                }
            });
            var funcs = ExpressionCalculator.CreateFunctionRegister();
            funcs.RegisterFunctionEnumerable<int>("NumElements", list => list.Count());
            funcs.RegisterFunc<string, string, object>("ValOfLookup", (listName, key) => Lookup[listName][key]);
            funcs.RegisterExpression("IsParam", (parameters) =>
            {
                var values = parameters.Select(ToValue).ToList();
                var isVal = values.Take(values.Count - 1).Any(a => object.Equals(a, values.Last()));
                return isVal;
            });

            var calc = new LogicCalculator(funcs);
            var invokableExpression = calc.ParseBoolExpression(expression, new Dictionary<string, object>());
            var value = invokableExpression.Compile()(vals);
            value.Should().Be(expectedValue);

        }




    }
}
