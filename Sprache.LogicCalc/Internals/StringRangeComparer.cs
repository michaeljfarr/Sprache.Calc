using System;
using System.Collections.Generic;

namespace Sprache.Calc.Internals
{
    public class StringRangeComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x is null || y is null)
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            }
            if (x.Length < y.Length)
            {
                return -1;
            }
            return string.Compare(x.Substring(0, y.Length), y, StringComparison.Ordinal);
        }
    }
    //folder matcher - must have a certain number of files that match
    // /folder/imagemeta/size>1024 && folder/filemeta.extension=='cr2'
    // //imagemeta <= any file with any image meta
    // ///size <= any file with any meta called size
    // //imagemeta/size
}
