using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rack.Reflection
{
    public static class ExpressionEx
    {
        /// <summary>
        /// Возвращает имя свойства, указанного в <see cref="property" />.
        /// </summary>
        /// <typeparam name="TProperty">Тип свойства.</typeparam>
        /// <param name="property">Выражение, возвращающее информацию о свойстве.</param>
        /// <returns></returns>
        public static string GetPropertyName<TParent, TProperty>(
            this Expression<Func<TParent, TProperty>> property)
        {
            if (!(property.Body is MemberExpression member))
                throw new ArgumentException($"В выражении '{property}' задан метод, а не свойство.");
            if (!(member.Member is PropertyInfo propertyInfo))
                throw new ArgumentException($"В выражении '{property}' задано поле, а не свойство.");
            return propertyInfo.Name;
        }
    }
}
