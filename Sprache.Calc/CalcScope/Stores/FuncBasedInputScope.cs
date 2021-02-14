using System;

namespace Sprache.Calc.CalcScope.Stores
{
    public class FuncBasedInputScope : IInputScope
    {
        private readonly Func<string, object> _func;

        public FuncBasedInputScope(Func<string, object> func)
        {
            _func = func;
        }

        public object GetValue(string key)
        {
            return _func(key);
        }
    }
}