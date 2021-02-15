using System;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Sprache.Calc.Internals
{
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