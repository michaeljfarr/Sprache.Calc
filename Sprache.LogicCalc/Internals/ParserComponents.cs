using System;
using System.Linq.Expressions;

namespace Sprache.Calc.Internals
{
    public static class ParserComponents
    {
        public static Parser<ExpressionType> Operator(string op, ExpressionType opType) =>
            Parse.String(op).Token().Return(opType);

        public static Parser<ExpressionType> GreaterThan =>
            Operator(">", ExpressionType.GreaterThan);

        public static Parser<ExpressionType> GreaterThanOrEqual =>
            Operator(">=", ExpressionType.GreaterThanOrEqual);

        public static Parser<ExpressionType> LessThan =>
            Operator("<", ExpressionType.LessThan);

        public static Parser<ExpressionType> LessThanOrEqual =>
            Operator("<=", ExpressionType.LessThanOrEqual);

        public static Parser<ExpressionType> Equal =>
            Operator("==", ExpressionType.Equal);

        public static Parser<ExpressionType> NotEqual =>
            Operator("!=", ExpressionType.NotEqual);


        public static Parser<Expression> QuotedText =
            from open in Parse.Char('"')
            from content in Parse.CharExcept('"').Many().Text()
            from close in Parse.Char('"')
            select Expression.Constant(content);

        public static Parser<Expression> SingleQuotedText =
            from open in Parse.Char('\'')
            from content in Parse.CharExcept('\'').Many().Text()
            from close in Parse.Char('\'')
            select Expression.Constant(content);
        
        public static Parser<string> ValueReferenceString => 
            Parse.Char(':').AtLeastOnce().Text().Then(f => Parse.Letter.AtLeastOnce().Text().Then(h =>
                Parse.LetterOrDigit.Or(Parse.Char(':')).Or(Parse.Char('_')).Or(Parse.Char('.')).Many().Text().Select(t => f + h + t))).Token();


    }
}
