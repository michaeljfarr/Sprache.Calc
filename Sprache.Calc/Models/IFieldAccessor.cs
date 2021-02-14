using System.Collections.Generic;

namespace Sprache.Calc.Models
{
    /// <summary>
    /// Handle
    /// </summary>
    public interface IFieldAccessor
    {
        IEnumerable<NodeStata> MatchStrata(IEnumerable<string> fieldRefs);
        IEnumerable<FieldReference> FieldReferences(string fieldPath);
        FieldReference FieldReference(string path, string fieldName);
    }
}