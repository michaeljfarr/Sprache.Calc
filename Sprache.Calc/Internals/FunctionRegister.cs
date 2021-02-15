using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sprache.Calc.Internals
{
    internal class FunctionRegister : IFunctionRegister
    {
        private Dictionary<string, object> CustomFunctions { get; } = new Dictionary<string, object>();
        private Dictionary<string, ObjectFunc> CustomExpressions2 { get; } = new Dictionary<string, ObjectFunc>();

        private static string MangleName(string name, int numParameters)
        {
            //returnType = returnType ?? (parameterTypes.All(a => a == typeof(double)) ? typeof(double) : typeof(bool));
            var mangledName = $"{name}(@{numParameters})";
            return mangledName;
        }

        private static IEnumerable<Expression> TryCast(Expression[] parameters, Type[] targetTypes)
        {
            return parameters.Select(((expression, i) =>
            {
                var targetType = targetTypes[i];
                // if (targetType.IsAssignableFrom(expression.Type))
                // {
                //     return expression;
                // }

                return Expression.Convert(expression, targetType);
            }));
        }

        public Expression CallFunction(string name, params Expression[] parameters)
        {
            // first look up a custom expression (expressions must have a unique name and must accept an array of expressions its only parameter)
            if (CustomExpressions2.ContainsKey(name))
            {
                //"expressions" in this context do not allow for Polymorphism
                var customExpression = CustomExpressions2[name];
                var method = (MethodInfo)customExpression.GetType().GetProperty("Method").GetValue(customExpression);
                var @object = customExpression.GetType().GetProperty("Target").GetValue(customExpression);
                Expression[] objectConversions = parameters.Select(param => Expression.ConvertChecked(param, typeof(object))).ToArray();
                var paramArray = Expression.NewArrayInit(typeof(object), objectConversions);
                return Expression.Call(Expression.Constant(@object), method, paramArray);
            }

            // now look up a custom function 
            var mangledName = MangleName(name, parameters.Length);
            if (CustomFunctions.ContainsKey(mangledName))
            {
                var customFunc = CustomFunctions[mangledName];
                var method = (MethodInfo)customFunc.GetType().GetProperty("Method").GetValue(customFunc);
                var @object = customFunc.GetType().GetProperty("Target").GetValue(customFunc);

                return Expression.Call(Expression.Constant(@object), method, TryCast(parameters, method.GetParameters().Select(a => a.ParameterType).ToArray()));
            }

            // fall back to System.Math functions
            return CallMathFunction(name, parameters);
        }
        public static Expression CallMathFunction(string name, params Expression[] parameters)
        {
            var methodInfo = typeof(Math).GetMethod(name, parameters.Select(e => e.Type).ToArray());
            if (methodInfo == null)
            {
                throw new ParseException(
                    $"Function '{name}({string.Join(",", parameters.Select(e => e.Type.Name))})' does not exist.");
            }

            return Expression.Call(methodInfo, parameters);
        }

        public IFunctionRegister RegisterExpression2(string name, ObjectFunc function)
        {
            CustomExpressions2[name] = function;
            return this;
        }

        public IFunctionRegister RegisterFunc<T1, TResult>(string name, Func<T1, TResult> function)
        {
            CustomFunctions[MangleName(name, function.GetMethodInfo().GetParameters().Count())] = function;
            return this;
        }

        public IFunctionRegister RegisterFunc<T1, T2, TResult>(string name, Func<T1, T2, TResult> function)
        {
            CustomFunctions[MangleName(name, function.GetMethodInfo().GetParameters().Count())] = function;
            return this;
        }

        public IFunctionRegister RegisterFunctionEnumerable<T>(string name, Func<IEnumerable<T>, object> function)
        {
            CustomFunctions[MangleName(name, 1)] = function;
            return this;
        }

    }
}