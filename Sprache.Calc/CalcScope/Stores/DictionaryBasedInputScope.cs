using System.Collections.Generic;
using System.Linq;

namespace Sprache.Calc.CalcScope.Stores
{
    public class DictionaryBasedInputScope: IInputScope
    {
        protected readonly IReadOnlyDictionary<string, object> Values;

        public DictionaryBasedInputScope(IReadOnlyDictionary<string, double> values)
        {
            Values = values.ToDictionary(a=>a.Key, a=>(object)a.Value);
        }

        public DictionaryBasedInputScope(IReadOnlyDictionary<string, object> values)
        {
            Values = values;
        }

        public object GetValue(string key)
        {
            if (Values.TryGetValue(key, out var value1))
            {
                return value1;
            }
            if (Values.TryGetValue(key.TrimStart(':'), out var value))
            {
                return value;
            }

            return null;
        }
    }
}