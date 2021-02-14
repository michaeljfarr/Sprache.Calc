using System.Collections.Generic;

namespace Sprache.Calc.Models
{
    public class CombinationSet<T>
    {
        public int NumCombinations { get; set; }
        public IReadOnlyList<Combination<T>> Combinations { get; set; }
        public IReadOnlyList<string> FieldRefs { get; set; }
    }
}