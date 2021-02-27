using System.Collections.Generic;

namespace Sprache.Calc.CalcScope.Stores
{
    public class DictionaryBasedMutableStore : IMutableScope
    {
        protected readonly IInputScope ReadonlyValues;
        protected readonly Dictionary<string, object> WritableValues = new Dictionary<string, object>();

        public DictionaryBasedMutableStore(IInputScope readonlyValues)
        {
            ReadonlyValues = readonlyValues;
        }

        public object GetValue(string key)
        {
            if (WritableValues.TryGetValue(key, out var value))
            {
                return value;
            }
            return ReadonlyValues.GetValue(key);
        }

        public void StoreValue(string key, object value)
        {
            WritableValues[key] = value;
        }
    }
}