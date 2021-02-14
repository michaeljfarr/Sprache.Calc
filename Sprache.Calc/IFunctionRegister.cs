using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Sprache.Calc.Internals;

namespace Sprache.Calc
{
    public interface IFunctionRegister
    {
        Expression CallFunction(string name, params Expression[] parameters);
        IFunctionRegister RegisterExpression(string name, Func<Expression[], object> function);
        IFunctionRegister RegisterExpression2(string name, ObjectFunc function);
        IFunctionRegister RegisterFunc<T1, TResult>(string name, Func<T1, TResult> function);
        IFunctionRegister RegisterFunc<T1, T2, TResult>(string name, Func<T1, T2, TResult> function);
        IFunctionRegister RegisterFunctionEnumerable<T>(string name, Func<IEnumerable<T>, object> function);
    }
}