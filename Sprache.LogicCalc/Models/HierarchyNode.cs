using System;
using System.Collections.Generic;
using System.Linq;

namespace Sprache.Calc.Models
{
    /// <summary>
    /// In memory hierarchy; suitable for high performance searching of smallish groups of files.
    /// </summary>
    public class HierarchyNode
    {
        public readonly List<object> _value = new List<object>();
        public Dictionary<string, HierarchyNode> Children { get; } = new Dictionary<string, HierarchyNode>();

        public void Add(IReadOnlyList<string> fields, int start)
        {
            if (start > fields.Count)
            {
                return;
            }
            var thisField = fields[start];
            var oneMore = fields.Count > (start + 1);
            if (!oneMore)
            {
                _value.Add(thisField);
                return;
            }
            if (Children.TryGetValue(thisField, out var subNode))
            {
                subNode.Add(fields, start + 1);
            }
            else
            {
                var hierarchyNode = new HierarchyNode();
                Children[thisField] = hierarchyNode;
                hierarchyNode.Add(fields, start + 1);
            }
        }
        //::a:1 => :: (all sections to the 2nd level) && a:1 (that have a value of a:1, where we assume a:1 is unique)
        public IEnumerable<NodeStata> MatchStrata(IEnumerable<string> fieldRefs)
        {
            var fieldRefItems = fieldRefs.Select(fieldRef =>
            {
                var fields = fieldRef.Split(':');
                var strataDepth = fields.TakeWhile(a => a == string.Empty).Count();
                return (fields, strataDepth);
            }).ToList();
            var firstStrataDepth = fieldRefItems.First().strataDepth;
            if (fieldRefItems.Any(a => a.strataDepth != firstStrataDepth))
            {
                throw new Exception($"Strata depth not equal {firstStrataDepth}");
            }
            return SubStrata(firstStrataDepth-1, "").Where(subStrata => fieldRefItems.Any(fieldRefItem => subStrata.Parent.Matches(fieldRefItem.fields, fieldRefItem.strataDepth) > 0));
        }

        //return
        public IEnumerable<NodeStata> SubStrata(int strataDepth, string parentPath)
        {
            if (strataDepth > 0)
            {
                foreach (var child in Children)
                {
                    foreach (var nodeStata in child.Value.SubStrata(strataDepth - 1,
                        $"{parentPath}:{child.Key}"))
                    {
                        yield return nodeStata;
                    }
                }
            }
            else
            {
                yield return new NodeStata{ Path = parentPath, Parent= this };
            }
        }


        public int Matches(IReadOnlyList<string> fields, int start)
        {
            if (start > fields.Count)
            {
                return 0;
            }
            var thisField = fields[start];
            var oneMore = fields.Count > (start + 1);
            if (!string.IsNullOrEmpty(thisField))
            {
                if (Children.TryGetValue(thisField, out var subNode))
                {
                    if (oneMore)
                    {
                        return subNode.Matches(fields, start + 1);
                    }
                    else
                    {
                        return 1;
                    }
                }

                return 0;
            }
            else
            {
                return _value.Count;// Children.Values.Sum(a => a.Matches(fields, start + 1));
            }
        }
        public IEnumerable<object> FieldReferenceValues(IReadOnlyList<string> fields, int start)
        {
            if (start > fields.Count)
            {
                yield break;
            }
            var thisField = fields[start];
            var oneMore = fields.Count > (start + 1);
            //if (!oneMore)
            //{
            //    yield return new FieldReference { Value = this._value };
            //    yield break;
            //}
            if (!string.IsNullOrEmpty(thisField))
            {
                if (Children.TryGetValue(thisField, out var subNode))
                {

                    if (oneMore)
                    {
                        foreach (var fieldReferenceVal in subNode.FieldReferenceValues(fields, start + 1))
                        {
                            yield return fieldReferenceVal;
                        }
                    }
                    else
                    {
                        foreach (var val in subNode._value)
                        {
                            yield return val;
                        }
                    }
                }

                yield break;
            }
            else
            {
                foreach (var child in Children.Values)
                {
                    foreach (var fieldReferenceVal in child.FieldReferenceValues(fields, start + 1))
                    {
                        yield return fieldReferenceVal;
                    }
                }
            }
        }
    }
}