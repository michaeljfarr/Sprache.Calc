using System.Collections.Generic;
using System.Linq;
using Sprache.Calc.CalcScope;
using Sprache.Calc.CalcScope.Stores;
using Sprache.Calc.Models;

namespace Sprache.Calc
{
    public static class InputExtensions
    {
        public static IEnumerable<IInputSlotAndValue> WithValues(this IEnumerable<IInputSlot> inputSlots)
        {
            return inputSlots.Where(a => a is IInputSlotAndValue).Cast<IInputSlotAndValue>();
        }

        public static IEnumerable<IInputSlot> NonConstants(this IEnumerable<IInputSlot> inputSlots)
        {
            return inputSlots.Where(a => !(a is IInputSlotAndValue));
        }

        public static IInputScope ToInputScope(this IEnumerable<IInputSlotAndValue> inputs)
        {
            return new DictionaryBasedInputScope(inputs.ToDictionary(a=>a.Name, a=>a.Value));
        }

        public static IMutableScope WrapWithMutableScope(this IInputScope inputScope)
        {
            return new DictionaryBasedMutableStore(inputScope);
        }
    }
}