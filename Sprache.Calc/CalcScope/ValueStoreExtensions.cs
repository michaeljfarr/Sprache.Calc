using System;

namespace Sprache.Calc.CalcScope
{
    public static class ValueStoreExtensions
    {
        public static void StoreValue(this IMutableScope mutables, Uri key, object value)
        {
            mutables.StoreValue(key.ToString(), value);
        }
        public static object GetValue(this IInputScope values, Uri key)
        {
            return values.GetValue(key.ToString());
        }
    }
}