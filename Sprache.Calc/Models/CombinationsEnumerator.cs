using System.Collections.Generic;
using System.Linq;

namespace Sprache.Calc.Models
{
    //https://github.com/blehnen/DotNetWorkQueue
    public class CombinationsEnumerator
    {
        private readonly IFieldAccessor _fieldAccessor;

        private CombinationSet<NodeStata> _combinationSet;

        private CombinationsEnumerator(IFieldAccessor fieldAccessor)
        {
            _fieldAccessor = fieldAccessor;
        }

        public int NumCombinations => _combinationSet.NumCombinations;

        public static CombinationsEnumerator Create(IFieldAccessor fieldAccessor, IReadOnlyList<string> fieldRefs)
        {
            var matchingStrata = fieldAccessor.MatchStrata(fieldRefs);

            var combinations = matchingStrata.Select(strata => 
                    new Combination<NodeStata>
                    {
                        //FieldRefs = fieldRefs, //"::A:1" =>
                        Context = strata
                    }).ToList();
            var refCombinations = new CombinationSet<NodeStata>
            {
                Combinations = combinations,
                NumCombinations = combinations.Count(),
                FieldRefs = fieldRefs
            };
            return new CombinationsEnumerator(fieldAccessor) { _combinationSet = refCombinations };
        }

        /// <summary>
        /// A combination set, returns a list of specific field references
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IReadOnlyList<FieldReference>> GetEnumerator()
        {
            return _combinationSet.Combinations.Select(combination =>
            {
                return _combinationSet.FieldRefs.Select(fieldRef =>
                {
                    var fieldReference = _fieldAccessor.FieldReference(combination.Context.Path, fieldRef);
                    return fieldReference;
                }).Where(a=>a!=null).ToList();
            });
        }

        public static IReadOnlyList<CombinationsEnumerator> CreateMany(IFieldAccessor fieldAccessor, IEnumerable<string[]> mandatoryCombinations)
        {
            return mandatoryCombinations.Select(combination => CombinationsEnumerator.Create(fieldAccessor, combination)).ToList();
        }
    }
}