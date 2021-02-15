using System;
using System.Linq.Expressions;
using Sprache.Calc.CalcScope;
using Sprache.Calc.Internals; //using NumberParameterList = System.Collections.Generic.Dictionary<string, double>;
using ObjectParameterList = System.Collections.Generic.Dictionary<string, object>;
namespace Sprache.Calc
{
    public class ExpressionCalculator
    {
        private readonly LogicCalculator _functionCalculator;

        public ExpressionCalculator(IFunctionRegister functionRegister = null)
        {
            _functionCalculator = new LogicCalculator(functionRegister ?? CreateFunctionRegister());
        }

        public static IFunctionRegister CreateFunctionRegister()
        {
            return new FunctionRegister();
        }

        // public Expression<Func<IInputScope, object>> ParseObjectFunc(string expression)
        // {
        //     return _functionCalculator.ParseObjectFunc(expression);
        // }
        //
        // public Expression<Func<IInputScope, double>> ParseDoubleFunc(string expression)
        // {
        //     return _functionCalculator.ParseDoubleFunc(expression);
        // }

        public Expression<Func<IInputScope, object>> ParseExpression(string expression)
        {
            return _functionCalculator.ParseBoolExpression(expression, new ObjectParameterList());
        }
    }
}
