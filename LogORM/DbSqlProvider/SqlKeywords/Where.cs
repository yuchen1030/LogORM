using System;
using System.Text;
using SysExp= System.Linq.Expressions;
  
namespace LogORM.DbSqlProvider.SqlKeywords
{
    public class Where:WordsBase
    {
        public Where()
        {

        }

        public string ToString<T>(SysExp.Expression<Func<T, bool>> fun)
        {
            return string.Format("Where {0} ", ExpressionParse<T>(fun));
        }

        protected string ExpressionParse<T>(SysExp.Expression<Func<T, bool>> exp)
        {
            StringBuilder builder = new StringBuilder();
            ExpressionParse(exp.Body, builder);

            return builder.ToString();
        }

        private SysExp.Expression ExpressionParse(SysExp.Expression exp, StringBuilder builder)
        {
            if (exp == null) return null;

            SysExp.BinaryExpression binEx = exp as SysExp.BinaryExpression;
            if (binEx != null) ExpressionParse(binEx.Left, builder);

            switch (exp.NodeType)
            {
                case SysExp.ExpressionType.Parameter:
                    {
                        SysExp.ParameterExpression param = (SysExp.ParameterExpression)exp;
                        builder.Append("(" + param.Name);
                        return null;
                    }
                case SysExp.ExpressionType.MemberAccess:
                    {
                        SysExp.MemberExpression mexp = (SysExp.MemberExpression)exp;
                        builder.Append("(" + mexp.Member.Name);
                        return null;
                    }
                case SysExp.ExpressionType.Constant:
                    {
                        SysExp.ConstantExpression cex = (SysExp.ConstantExpression)exp;
                        if (cex.Value is string) builder.Append("'" + cex.Value.ToString() + "') ");
                        else if (cex.Value is bool) builder.Append("1=1");
                        else builder.Append(cex.Value.ToString() + ")");
                        return null;
                    }
                default:
                    {
                        if (exp.NodeType == SysExp.ExpressionType.Equal) builder.Append("=");
                        else if (exp.NodeType == SysExp.ExpressionType.NotEqual) builder.Append("<>");
                        else if (exp.NodeType == SysExp.ExpressionType.LessThan) builder.Append("<");
                        else if (exp.NodeType == SysExp.ExpressionType.LessThanOrEqual) builder.Append("<=");
                        else if (exp.NodeType == SysExp.ExpressionType.GreaterThan) builder.Append(">");
                        else if (exp.NodeType == SysExp.ExpressionType.GreaterThanOrEqual) builder.Append(">=");
                        else if (exp.NodeType == SysExp.ExpressionType.AndAlso || exp.NodeType == SysExp.ExpressionType.And)
                        {
                            builder.Append("And");
                        }
                        else if (exp.NodeType == SysExp.ExpressionType.OrElse || exp.NodeType == SysExp.ExpressionType.Or)
                        {
                            builder.Append("Or");
                        }
                    }
                    break;
            }

            if (binEx != null) ExpressionParse(binEx.Right, builder);

            return binEx;
        }

    }
}
