using System;
using System.Globalization;
using System.Linq;

namespace Sprache.Calc.Internals
{
    internal static class NumericComponents
    {

        public static Parser<string> Exponent =>
            Parse.Chars("Ee").Then(e => Parse.Number.Select(n => "e+" + n).XOr(
                Parse.Chars("+-").Then(s => Parse.Number.Select(n => "e" + s + n))));

    }
}