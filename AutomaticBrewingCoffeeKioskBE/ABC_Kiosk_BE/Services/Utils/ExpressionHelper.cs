using System.Linq.Expressions;
using CouchDb.Domain;
using Domain.Models;
using DynamicExpresso;

namespace Services.Utils
{
    public static class ExpressionHelper
    {
        public static Expression<Func<T, bool>> CombineExpressions<T>(
            Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            if (first == null) return second;
            if (second == null) return first;

            var parameter = Expression.Parameter(typeof(T));

            var combined = Expression.AndAlso(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter)
            );

            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }

        public static bool EvaluateExpressionConditions(object obj, List<StepConditionRaw> conditions)
        {
            if (conditions.Count == 0) return true;

            var type = obj.GetType();
            var interpreter = new Interpreter();

            foreach (var condition in conditions)
            {
                var prop = type.GetProperty(condition.Name);
              
                if (prop == null)
                {
                    return false;
                }

                var value = prop.GetValue(obj);
                interpreter.SetVariable(condition.Name, value, prop.PropertyType);
            }
            foreach (var condition in conditions)
            {
                try
                {
                    var result = interpreter.Eval(condition.Expression);
                    if (result is not bool b || b == false)
                    {
                        return false;
                    }
                }
                catch (Exception )
                {
                    return false;
                }
            }
            return true;
        }
    }
}
