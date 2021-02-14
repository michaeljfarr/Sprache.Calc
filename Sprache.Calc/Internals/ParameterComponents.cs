using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sprache.Calc.CalcScope;

namespace Sprache.Calc.Internals
{
    internal static class ParameterComponents
    {
        private static readonly MethodInfo GetValueMethod = typeof(IInputScope).GetMethod("GetValue");

        public static ParameterExpression ParameterExpression { get; } =
            Expression.Parameter(typeof(IInputScope), "Parameters");

        public static Expression GetParameterExpression(string name)
        {
            if (name == "false")
            {
                return Expression.Constant(false, typeof(bool));
            }
            if (name == "true")
            {
                return Expression.Constant(true, typeof(bool));
            }
            // try to find a constant in System.Math
            var systemMathConstants = typeof(Math).GetFields(BindingFlags.Public | BindingFlags.Static);
            var constant = systemMathConstants.FirstOrDefault(c => c.Name == name);
            if (constant != null)
            {
                // return System.Math constant value
                return Expression.Constant(constant.GetValue(null));
            }

            // return parameter value: Parameters[name]
            return Expression.Call(ParameterExpression, GetValueMethod, Expression.Constant(name));
        }

        public static Parser<string> Identifier =>
            Parse.Letter.AtLeastOnce().Text().Then(h =>
                Parse.LetterOrDigit.Many().Text().Select(t => h + t)).Token();

        public static Parser<Expression> Parameter =>
            // identifier not followed by a '(' is a parameter reference
            from id in ParameterComponents.Identifier
            from n in Parse.Not(Parse.Char('('))
            select ParameterComponents.GetParameterExpression(id);

    public static Expression MakeTypeAlignedBinary(
            ExpressionType binaryType,
            Expression left,
            Expression right)
        {
            var isComparative = binaryType == ExpressionType.LessThan || binaryType == ExpressionType.LessThanOrEqual ||
                                binaryType == ExpressionType.GreaterThan || binaryType == ExpressionType.GreaterThanOrEqual ||
                                binaryType == ExpressionType.Equal || binaryType == ExpressionType.NotEqual;
            var isLogical = binaryType == ExpressionType.OrElse ||
                            binaryType == ExpressionType.AndAlso;

            if (isLogical)
            {
                if (left.Type == typeof(object))
                {
                    //right = ConvertTo(right, typeof(double));
                    left = Expression.Call(ComparisonComponents.ToBoolMethodInfo, Expression.Convert(left, typeof(object)));
                }
                if (right.Type == typeof(object))
                {
                    //right = ConvertTo(right, typeof(double));
                    right = Expression.Call(ComparisonComponents.ToBoolMethodInfo, Expression.Convert(right, typeof(object)));
                }
                return Expression.MakeBinary(binaryType, left, right, false, (MethodInfo)null, (LambdaExpression)null);
            }
            if (isComparative)
            {
                return Expression.Convert(Expression.MakeBinary(binaryType, Expression.Call(ComparisonComponents.CompareMethodInfo, new[] { Expression.Convert(left, typeof(object)), Expression.Convert(right, typeof(object)) }), Expression.Constant(0)), typeof(object));
            }

            if (binaryType == ExpressionType.Power)
            {
                left = Expression.Convert(left, typeof(double));
                right = Expression.Convert(right, typeof(double));
            }
            else if (binaryType == ExpressionType.MultiplyChecked || binaryType == ExpressionType.AddChecked || binaryType == ExpressionType.SubtractChecked || binaryType == ExpressionType.Divide)
            {
                //if either side is a double, convert the other to double
                if (left.Type == typeof(double) || left.Type == typeof(float))
                {
                    //right = ConvertTo(right, typeof(double));
                    right = Expression.Call(ComparisonComponents.ToDoubleMethodInfo, Expression.Convert(right, typeof(object)));
                }
                else if (right.Type == typeof(double) || right.Type == typeof(float))
                {
                    //left = ConvertTo(left, typeof(double));
                    left = Expression.Call(ComparisonComponents.ToDoubleMethodInfo, Expression.Convert(left, typeof(object)));
                }
                else
                {
                    //otherwise convert both to decimals
                    left = Expression.Convert(left, typeof(double));
                    right = Expression.Convert(right, typeof(double));
                }
            }

            return Expression.Convert(Expression.MakeBinary(binaryType, left, right, false, (MethodInfo)null, (LambdaExpression)null), typeof(object));
        }
    }
}