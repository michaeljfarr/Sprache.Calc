using System;
using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Sprache.Calc.Internals
{

    internal abstract partial class SimpleCalculator
    {
        // lowest priority first
        Parser<Expression> OrTerm =>
            Parse.ChainOperator(OpOr, AndTerm, ParameterComponents.MakeTypeAlignedBinary);

        static Parser<ExpressionType> OpOr => MakeOperator("or", ExpressionType.OrElse).Or(MakeOperator("||", ExpressionType.OrElse));

        Parser<Expression> AndTerm =>
            Parse.ChainOperator(OpAnd, NotTerm, ParameterComponents.MakeTypeAlignedBinary);

        static Parser<ExpressionType> OpAnd => MakeOperator("and", ExpressionType.AndAlso).Or(MakeOperator("&&", ExpressionType.AndAlso));


        protected Parser<Expression> TreeTop => FormulaMath;//.XOr(FormulaMathsValuesA);

        Parser<Expression> NotFactor =>
            from negate in Parse.IgnoreCase("!").Token()
            from expr in FormulaTop
            select Expression.Not(expr);
        
        Parser<Expression> NotTerm => NotFactor.Or(FormulaTop);
    

        static Parser<Expression> BooleanLiteral =>
            Parse.IgnoreCase("true").Or(Parse.IgnoreCase("false"))
            .Text().Token()
            .Select(value => Expression.Constant(bool.Parse(value)));

        static Parser<ExpressionType> MakeOperator(string token, ExpressionType type)
            => Parse.IgnoreCase(token).Token().Return(type);
    }
    internal static class ComparisonComponents
    {
        private static bool ToBool(object value)
        {
            if (value is IConvertible convertible)
            {
                return (bool)convertible.ToType(typeof(bool), CultureInfo.InvariantCulture.NumberFormat);
            }

            throw new Exception($"failed to convert ({value?.GetType()}){value} to a bool");
        }
        
        private static double ToDouble(object value)
        {
            if (value is IConvertible convertible)
            {
                return (double)convertible.ToType(typeof(double), CultureInfo.InvariantCulture.NumberFormat);
            }

            throw new Exception($"failed to convert ({value?.GetType()}){value} to a double");
        }

        private static double ToDecimal(object value)
        {
            if (value is IConvertible convertible)
            {
                return (double)convertible.ToType(typeof(double), CultureInfo.InvariantCulture.NumberFormat);
            }

            throw new Exception($"failed to convert ({value?.GetType()}){value} to a double");
        }

        private static int Compare(object left, object right)
        {
            if (left is double || left is float)
            {
                if (right == null)
                {
                    return 1;
                }
                right = ToDouble(right);
            }
            else if (right is double || right is float)
            {
                if (left == null)
                {
                    return -1;
                }
                left = ToDouble(left);
            }
            else if (left is decimal)
            {
                if (right == null)
                {
                    return 1;
                }
                right = ToDecimal(right);
            }
            else if (right is decimal)
            {
                if (left == null)
                {
                    return -1;
                }
                left = ToDecimal(left);
            }
            return Comparer.Default.Compare(left, right);
        }

        public static MethodInfo ToBoolMethodInfo { get; } = typeof(ComparisonComponents).GetMethod(nameof(ToBool), BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.Any, new[] { typeof(object) }, null);
        public static MethodInfo ToDoubleMethodInfo { get; } = typeof(ComparisonComponents).GetMethod(nameof(ToDouble), BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.Any, new[] { typeof(object) }, null);
        public static MethodInfo CompareMethodInfo { get; } = typeof(ComparisonComponents).GetMethod(nameof(Compare), BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.Any, new[] { typeof(object), typeof(object) }, null);

    }
}