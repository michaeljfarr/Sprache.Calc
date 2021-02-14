using System;
using System.Globalization;
using System.Linq;

namespace Sprache.Calc.Internals
{
    internal static class NumericComponents
    {
        public static Parser<string> Binary =>
            Parse.IgnoreCase("0b").Then(x =>
                Parse.Chars("01").AtLeastOnce().Text()).Token();

        public static Parser<string> Hexadecimal =>
            Parse.IgnoreCase("0x").Then(x =>
                Parse.Chars("0123456789ABCDEFabcdef").AtLeastOnce().Text()).Token();

        public static Parser<string> Exponent =>
            Parse.Chars("Ee").Then(e => Parse.Number.Select(n => "e+" + n).XOr(
                Parse.Chars("+-").Then(s => Parse.Number.Select(n => "e" + s + n))));

        public static Parser<string> FalseConstant => Parse.Regex("false").Text().Token();
        public static Parser<string> TrueConstant => Parse.Regex("true").Text().Token();

        public static ulong ConvertBinary(string bin)
        {
            return bin.Aggregate(0ul, (result, c) =>
            {
                if (c < '0' || c > '1')
                {
                    throw new ParseException(bin + " cannot be parsed as binary number");
                }

                return result * 2 + c - '0';
            });
        }

        public static ulong ConvertHexadecimal(string hex)
        {
            var result = 0ul;
            if (UInt64.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            throw new ParseException(hex + " cannot be parsed as hexadecimal number");
        }
    }
}