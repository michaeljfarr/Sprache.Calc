using System.Collections.Generic;
using System.Linq;
using Sprache.Calc.Internals;

namespace Sprache.Calc.Models
{
    public class MetaData : HierarchyNode, IFieldAccessor
    {
        private static readonly StringRangeComparer StringRangeComparer = new StringRangeComparer();

        //<folder name>/<file name>/<meta group>/<field name>/<value>#

        public void Add(string field)
        {
            var components = field.Split('/');
            Add(components, 1);
        }

        public IEnumerable<FieldReference> FieldReferences(string fieldPath)
        {
            var fields = fieldPath.Split(':');
            return FieldReferenceValues(fields, 1).Select(a=>new FieldReference(){FieldName = fieldPath, Value = a});
        }

        public FieldReference FieldReference(string path, string fieldName)
        {
            string fieldPath = $"{path}:{fieldName.Substring(path.Count(c=>c==':')+1)}";
            var fieldReference = FieldReferences(fieldPath).SingleOrDefault();
            if (fieldReference != null)
            {
                fieldReference.FieldName = fieldName;
            }

            return fieldReference;
        }
    }
}