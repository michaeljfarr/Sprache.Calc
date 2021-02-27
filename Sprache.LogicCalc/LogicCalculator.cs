using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sprache.Calc.CalcScope;
using Sprache.Calc.Internals;

namespace Sprache.Calc
{
    public class LogicCalculator : Sprache.Calc.XtensibleCalculator 
    {
        private readonly IFunctionRegister _functionRegister;

        public LogicCalculator(IFunctionRegister functionRegister = null)
        {
            _functionRegister = functionRegister;
        }

        static Parser<ExpressionType> MakeOperator(string token, ExpressionType type)
            => Parse.IgnoreCase(token).Token().Return(type);

        static Parser<ExpressionType> OpOr => MakeOperator("or", ExpressionType.OrElse).Or(MakeOperator("||", ExpressionType.OrElse));
        static Parser<ExpressionType> OpAnd => MakeOperator("and", ExpressionType.AndAlso).Or(MakeOperator("&&", ExpressionType.AndAlso));
        

        
        private Parser<Expression> ValueReference => ParserComponents.ValueReferenceString.Select(ParameterComponents.GetParameterExpression).Named("ValueReference");

        protected internal override Parser<Expression> Constant => ParserComponents.QuotedText.XOr(ParserComponents.SingleQuotedText).XOr(base.Constant).XOr(ValueReference);
        
        Parser<Expression> NotFactor =>
            from negate in Parse.IgnoreCase("!").Token()
            from expr in Factor
            select Expression.Not(expr);
        
        protected internal override Parser<Expression> InnerTerm =>
            Parse.ChainRightOperator(Power, Operand, ParameterComponents.MakeTypeAlignedBinary);
        protected internal override Parser<Expression> Operand =>
            (NegativeFactor.XOr(Factor).XOr(NotFactor)).Token();
        
        protected internal override Parser<Expression> Term =>
            Parse.ChainOperator(Multiply.Or(Divide).Or(Modulo).Or(OpOr).Or(OpAnd).Or(ParserComponents.GreaterThan).Or(ParserComponents.LessThan).Or(ParserComponents.Equal).Or(ParserComponents.NotEqual).Or(ParserComponents.GreaterThanOrEqual).Or(ParserComponents.LessThanOrEqual).
                Or(OpOr).Or(OpAnd), InnerTerm, ParameterComponents.MakeTypeAlignedBinary);
        
        protected internal override ParameterExpression ParameterExpression { get; } = Expression.Parameter(typeof (Dictionary<string, object>), "Parameters");
        
        protected internal override Expression GetParameterExpression(string name)
        {
            if (name == "false")
            {
                return Expression.Constant(false, typeof(bool));
            }
            if (name == "true")
            {
                return Expression.Constant(true, typeof(bool));
            }
            FieldInfo fieldInfo = ((IEnumerable<FieldInfo>) typeof (Math).GetFields(BindingFlags.Static | BindingFlags.Public)).FirstOrDefault<FieldInfo>((Func<FieldInfo, bool>) (c => c.Name == name));
            if (fieldInfo != (FieldInfo) null)
                return (Expression) Expression.Constant(fieldInfo.GetValue((object) null));
            return (Expression) Expression.Call((Expression) this.ParameterExpression, typeof (Dictionary<string, object>).GetMethod("get_Item"), (Expression) Expression.Constant((object) name));
        }
        // public Parser<Expression> FunctionCall =>
        //     from name in ParameterComponents.Identifier
        //     from lparen in Parse.Char('(')
        //     from expr in Expr.DelimitedBy(Parse.Char(',').Token())
        //     from rparen in Parse.Char(')')
        //     select _functionRegister.CallFunction(name, expr.ToArray());
        
        protected internal override Expression CallFunction(
            string name,
            params Expression[] parameters)
        {
            if (_functionRegister != null)
            {
                return _functionRegister.CallFunction(name, parameters);
            }
            else
            {
                string key = this.MangleName(name, parameters.Length);
                if (!this.CustomFuctions.ContainsKey(key))
                    return base.CallFunction(name, parameters);
                MethodInfo methodInfo = new Func<string, double[], double>(this.CallCustomFunction).GetMethodInfo();
                return (Expression) Expression.Call((Expression) Expression.Constant((object) this), methodInfo,
                    new List<Expression>()
                    {
                        (Expression) Expression.Constant((object) key),
                        (Expression) Expression.NewArrayInit(typeof(double), parameters)
                    }.ToArray());
            }

        }
        
        protected override Parser<Expression> ExpressionInParentheses => Parse.Char('(').SelectMany((Func<char, Parser<Expression>>) (lparen => this.ObjExpr), (lparen, expr) => new
        {
            lparen = lparen,
            expr = expr
        }).SelectMany(_param1 => Parse.Char(')'), (_param1, rparen) => _param1.expr);

        private Parser<Expression> ObjExpr => Parse.ChainOperator<Expression, ExpressionType>(this.Add.Or<ExpressionType>(this.Subtract), this.Term, new Func<ExpressionType, Expression, Expression, Expression>(ParameterComponents.MakeTypeAlignedBinary));

        protected virtual Parser<LambdaExpression> LambdaBool =>
            ObjExpr.End().Select(body => Expression.Lambda<Func<Dictionary<string, object>, IInputScope, object>>(Expression.Convert(body, typeof(object)), this.ParameterExpression, ParameterComponents.ParameterExpression));
        
        public virtual Expression<Func<Dictionary<string, object>, IInputScope, object>> ParseFunctionBool(
            string text)
        {
            var lambdaExpression = this.LambdaBool.Parse<LambdaExpression>(text);
            return (Expression<Func<Dictionary<string, object>, IInputScope, object>>)lambdaExpression;
        }
        
        //protected internal override Parser<LambdaExpression> Lambda => (Parser<LambdaExpression>) this.Expr.End<Expression>().Select<Expression, Expression<Func<Dictionary<string, double>, double>>>((Func<Expression, Expression<Func<Dictionary<string, double>, double>>>) (body => Expression.Lambda<Func<Dictionary<string, double>, double>>(body, this.ParameterExpression)));
        // public virtual Expression<Func<Dictionary<string, double>, double>> ParseFunction(
        //     string text)
        // {
        //     return this.Lambda.Parse<LambdaExpression>(text) as Expression<Func<Dictionary<string, double>, double>>;
        // }
        //
        // public virtual Expression<Func<double>> ParseExpression(
        //     string text,
        //     Dictionary<string, double> parameters)
        // {
        //     return Expression.Lambda<Func<double>>((Expression) Expression.Invoke((Expression) this.ParseFunction(text), (Expression) Expression.Constant((object) parameters)));
        // }
        
        public virtual Expression<Func<IInputScope, object>> ParseBoolExpression(string text, Dictionary<string, object> parameters)
        {
            var invocationExpression = Expression.Invoke(ParseFunctionBool(text), (Expression) Expression.Constant(parameters), ParameterComponents.ParameterExpression);
            return Expression.Lambda<Func<IInputScope, object>>(invocationExpression, ParameterComponents.ParameterExpression);
        }
    }
}