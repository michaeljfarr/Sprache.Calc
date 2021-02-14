using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Sprache.Calc.Internals
{
    //https://github.com/sprache/Sprache/blob/master/samples/LinqyCalculator/ExpressionParser.cs
    //https://raw.githubusercontent.com/yallie/Sprache.Calc/master/Sprache.Calc/SimpleCalculator.cs
    /// <summary>
	/// Simple calculator grammar.
	/// Supports arithmetic operations and parentheses.
	/// </summary>
    internal abstract partial class SimpleCalculator
    {
        protected readonly IFunctionRegister FunctionRegister;

        protected SimpleCalculator(IFunctionRegister functionRegister)
        {
            FunctionRegister = functionRegister;
        }

        private Parser<Expression> ConstantA => ParserComponents.Decimal.Select(x => Expression.Constant(double.Parse(x, CultureInfo.InvariantCulture))).Named("Constant");

        private Parser<Expression> Constant =>
            NumericComponents.TrueConstant.Select(a => Expression.Constant(true, typeof(bool)))
                .Or(NumericComponents.FalseConstant.Select(a => Expression.Constant(false, typeof(bool)))).Or(NumericComponents.Hexadecimal.Select(x => Expression.Constant((double)NumericComponents.ConvertHexadecimal(x))))
                .Or(NumericComponents.Binary.Select(b => Expression.Constant((double)NumericComponents.ConvertBinary(b))))
                .Or(ConstantA);

        private Parser<Expression> ValueReference => ParserComponents.ValueReferenceString.Select(ParameterComponents.GetParameterExpression).Named("ValueReference");

        private readonly Parser<Expression> QuotedText =
            from open in Parse.Char('"')
            from content in Parse.CharExcept('"').Many().Text()
            from close in Parse.Char('"')
            select Expression.Constant(content);

        private readonly Parser<Expression> SingleQuotedText =
            from open in Parse.Char('\'')
            from content in Parse.CharExcept('\'').Many().Text()
            from close in Parse.Char('\'')
            select Expression.Constant(content);

        public Parser<Expression> ObjectFactorA =>
            ExpressionInParentheses.XOr(ValueReference).XOr(Constant).XOr(QuotedText).XOr(SingleQuotedText);

        /// <summary>
        /// This is an extension point for the function calculator
        /// </summary>
        private Parser<Expression> TypedFactorA =>
            from factor in ObjectFactorA
            select factor;

        //FunctionCalculator
        private Parser<Expression> ObjectFactor =>
            TypedFactorA.XOr(FunctionCall);

        protected Parser<Expression> TypedFactor => ParameterComponents.Parameter.Or(ObjectFactor);


        private Parser<Expression> NegativeFactor =>
            from sign in Parse.Char('-')
            from factor in TypedFactor
            select Expression.NegateChecked(factor);

        private Parser<Expression> Operand =>
            (NegativeFactor.XOr(NotTerm).XOr(TypedFactor)).Token();

        //private Parser<Expression> InnerTerm =>
        //    Parse.ChainRightOperator(ParserComponents.Power, Operand, ParameterComponents.MakeTypeAlignedBinary);

        //private Parser<Expression> Term =>
        //    Parse.ChainOperator(ParserComponents.Multiply.Or(ParserComponents.Divide).Or(ParserComponents.Modulo), InnerTerm, ParameterComponents.MakeTypeAlignedBinary);

        private static readonly Parser<ExpressionType> MathOperators = ParserComponents.Add.Or(ParserComponents.Subtract).Or(ParserComponents.Multiply).Or(ParserComponents.Divide).
            Or(ParserComponents.GreaterThan).Or(ParserComponents.LessThan).Or(ParserComponents.Equal).Or(ParserComponents.NotEqual).Or(ParserComponents.GreaterThanOrEqual).Or(ParserComponents.LessThanOrEqual).
            Or(OpOr).Or(OpAnd);

        public static readonly Parser<IOption<IEnumerable<char>>> OptionalWhiteSpace =
            Parse.WhiteSpace.Many().Optional();

        private Parser<Expression> FormulaMathsValuesA =>
            ValueReference
                .Or(Operand)
                .Or(NotTerm)
                .Or(ConstantA)
                .Or(Constant)
                .Or(ExpressionInParentheses)
                .Or(FunctionCall);

        public Parser<Expression> FunctionCall =>
            from name in ParameterComponents.Identifier
            from lparen in Parse.Char('(')
            from expr in TreeTop.DelimitedBy(Parse.Char(',').Token())
            from rparen in Parse.Char(')')
            select FunctionRegister.CallFunction(name, expr.ToArray());

        public static Expression CallMathFunction(string name, params Expression[] parameters)
        {
            var methodInfo = typeof(Math).GetMethod(name, parameters.Select(e => e.Type).ToArray());
            if (methodInfo == null)
            {
                throw new ParseException(
                    $"Function '{name}({string.Join(",", parameters.Select(e => e.Type.Name))})' does not exist.");
            }

            return Expression.Call(methodInfo, parameters);
        }


        private Parser<Expression> FormulaMath =>
            from value1 in FormulaMathsValuesA
            from parts in (
                from sign in MathOperators.Contained(OptionalWhiteSpace, OptionalWhiteSpace)
                from value2 in FormulaMathsValuesA
                select (sign, value2)
            ).Many()
            select MakeExpression(value1, parts.ToList());

        private static readonly ExpressionType[] OperatorPrecedence =
        {
            ExpressionType.Divide, ExpressionType.MultiplyChecked, 
            ExpressionType.Add, ExpressionType.AddChecked, ExpressionType.Subtract, ExpressionType.SubtractChecked,
            ExpressionType.GreaterThan, ExpressionType.LessThan, ExpressionType.Equal, ExpressionType.NotEqual,
            ExpressionType.GreaterThanOrEqual, ExpressionType.LessThanOrEqual, ExpressionType.Not, ExpressionType.OrElse,
            ExpressionType.AndAlso
        };

        //i || ((a + b + (c / d) * (e / f)) >= g) || h
        public class Node
        {
            public ExpressionType OperatorType { get; set; }
            public Expression Left { get; set; }
            public Expression Right { get; set; }
        }
        private Expression MakeExpression(Expression value1, IReadOnlyList<(ExpressionType sign, Expression value2)> parts)
        {
            if (!parts.Any())
            {
                return value1;
            }
            var nodes = new List<Node>();
            var firstNode = new Node()
            {
                Left = value1,
                Right = parts[0].value2,
                OperatorType = parts[0].sign
            };
            nodes.Add(firstNode);
            for (int i = 1; i < parts.Count; i++)
            {
                var node = new Node()
                {
                    Left = nodes[i - 1].Right,
                    Right = parts[i].value2,
                    OperatorType = parts[i].sign
                };
                nodes.Add(node);
            }

            foreach (var operatorType in OperatorPrecedence)
            {
                if (nodes.Count <= 1)
                {
                    break;
                }
                for (var i = 0; i < nodes.Count;)
                {
                    var node = nodes[i];
                    if (node.OperatorType == operatorType)
                    {
                        var expression = ParameterComponents.MakeTypeAlignedBinary(node.OperatorType, node.Left, node.Right);
                        if (i > 0)
                        {
                            nodes[i - 1].Right = expression;
                        }

                        if (i < (nodes.Count - 1))
                        {
                            nodes[i + 1].Left = expression;
                        }

                        nodes.RemoveAt(i);
                        if (nodes.Count <= 1)
                        {
                            break;
                        }

                    }
                    else
                    {
                        i++;
                    }
                }
            }

            //the last thing to return should be a logical operator with all the other types nested inside
            var lastNode = nodes.First();
            return ParameterComponents.MakeTypeAlignedBinary(lastNode.OperatorType, lastNode.Left, lastNode.Right);
        }


        public Parser<Expression> ExpressionInParentheses =>
            from lparen in Parse.Char('(')
            from expr in TreeTop
            from rparen in Parse.Char(')')
            select expr;

        //public Parser<Expression> ExprArithmetic =>
        //    Parse.ChainOperator(ParserComponents.Add.Or(ParserComponents.Subtract), Term, ParameterComponents.MakeTypeAlignedBinary);

        //public Parser<Expression> BoolExpr =>
        //    Parse.ChainOperator(ParserComponents.GreaterThan.Or(ParserComponents.LessThan).Or(ParserComponents.Equal).Or(ParserComponents.NotEqual).Or(ParserComponents.GreaterThanOrEqual).Or(ParserComponents.LessThanOrEqual), FormulaTop, ParameterComponents.MakeTypeAlignedBinary);

        protected Parser<Expression> FormulaTop => ExpressionInParentheses.XOr(TypedFactor);
        //protected Parser<Expression> FormulaTop => TreeTop.XOr(ExpressionInParentheses).XOr(TypedFactor);
        //protected Parser<Expression> FormulaTop => Parse.ChainOperator(Add.Or(Subtract), Term, Expression.MakeBinary);

        private Parser<LambdaExpression> LambdaAny =>
            TreeTop.End().Select(body => Expression.Lambda<Func<bool>>(body));

        private Parser<LambdaExpression> LambdaDoubleOnly =>
            FormulaTop.End().Select(body => Expression.Lambda<Func<double>>(body));

        private Expression<Func<bool>> ParseBoolExpression(string text) =>
            LambdaAny.Parse(text) as Expression<Func<bool>>;

        public virtual Expression<Func<double>> ParseExpression(string text) => LambdaDoubleOnly.Parse(text) as Expression<Func<double>>;
    }

    // <summary>
}

