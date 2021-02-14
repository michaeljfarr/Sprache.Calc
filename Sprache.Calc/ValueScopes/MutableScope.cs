using System.Collections.Generic;
using System.Linq;
using Sprache.Calc.CalcScope;

namespace Sprache.Calc.ValueScopes
{
    public class MutableScope : IMutableScope
    {
        private readonly IMutableScope _mutableScope;
        private readonly Dictionary<string, object> _inputs;

        public MutableScope(IEnumerable<Models.IInput> inputs, IMutableScope mutableScope)
        {
            _mutableScope = mutableScope;
            _inputs = inputs.ToDictionary(a=>a.Name, a=>a.Value);
        }
        
        public object GetValue(string reference)
        {
            if (_inputs.TryGetValue(reference, out var val))
            {
                return val;
            }
            return _mutableScope.GetValue(reference);
        }

        public void StoreValue(string reference, object value)
        {
            _mutableScope.StoreValue(reference, value);
        }
    }
}