using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

// The type is placed intentionally in the root namespace to ease access from other projects and namespaces
namespace SharpestBeak
{
    public static class Helper
    {
        #region Constants

        private const string InvalidExpressionMessageFmt =
            "Invalid expression (must be a getter of a property of the type '{0}'): {{ {1} }}.";
        private const string InvalidExpressionMessageAutoFmt =
            "Invalid expression (must be a getter of a property of some type): {{ {0} }}.";

        #endregion

        #region Public Methods

        public static void DisposeAndNull<T>(ref T disposable)
            where T : class, IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }
        }

        public static string ToFixedString(this DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public static List<TEnum> GetEnumValues<TEnum>()
            where TEnum : struct
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
        }

        public static void Exchange<T>(ref T value1, ref T value2)
        {
            T temp = value1;
            value1 = value2;
            value2 = temp;
        }

        /// <summary>
        ///     Gets the <see cref="PropertyInfo"/> of the property specified by the lambda expression.
        /// </summary>
        /// <typeparam name="TObject">
        ///     The type containing the property.
        /// </typeparam>
        /// <typeparam name="TProperty">
        ///     The type of the property.
        /// </typeparam>
        /// <param name="propertyGetterExpression">
        ///     The lambda expression in the following form: <c>(SomeType x) => x.Property</c>.
        /// </param>
        /// <returns>
        ///     The <see cref="PropertyInfo"/> containing information about the required property.
        /// </returns>
        public static PropertyInfo GetPropertyInfo<TObject, TProperty>(
            Expression<Func<TObject, TProperty>> propertyGetterExpression)
        {
            #region Argument Check

            if (propertyGetterExpression == null)
            {
                throw new ArgumentNullException("propertyGetterExpression");
            }

            #endregion

            var objectType = typeof(TObject);

            var memberExpression = propertyGetterExpression.Body as MemberExpression;
            if ((memberExpression == null) || (memberExpression.NodeType != ExpressionType.MemberAccess))
            {
                throw new ArgumentException(
                    string.Format(InvalidExpressionMessageFmt, objectType.FullName, propertyGetterExpression),
                    "propertyGetterExpression");
            }

            var result = memberExpression.Member as PropertyInfo;
            if (result == null)
            {
                throw new ArgumentException(
                    string.Format(InvalidExpressionMessageFmt, objectType.FullName, propertyGetterExpression),
                    "propertyGetterExpression");
            }

            if ((result.DeclaringType == null) || !result.DeclaringType.IsAssignableFrom(objectType))
            {
                throw new ArgumentException(
                    string.Format(InvalidExpressionMessageFmt, objectType.FullName, propertyGetterExpression),
                    "propertyGetterExpression");
            }

            if (memberExpression.Expression == null)
            {
                var accessor = result.GetGetMethod(true) ?? result.GetSetMethod(true);
                if ((accessor == null) || !accessor.IsStatic || (result.ReflectedType != objectType))
                {
                    throw new ArgumentException(
                        string.Format(InvalidExpressionMessageFmt, objectType.FullName, propertyGetterExpression),
                        "propertyGetterExpression");
                }
            }
            else
            {
                var parameterExpression = memberExpression.Expression as ParameterExpression;
                if ((parameterExpression == null) || (parameterExpression.NodeType != ExpressionType.Parameter) ||
                    (parameterExpression.Type != typeof(TObject)))
                {
                    throw new ArgumentException(
                        string.Format(InvalidExpressionMessageFmt, objectType.FullName, propertyGetterExpression),
                        "propertyGetterExpression");
                }
            }

            return result;
        }

        /// <summary>
        ///     Gets the name of the property specified by the lambda expression.
        /// </summary>
        /// <typeparam name="TObject">
        ///     The type containing the property.
        /// </typeparam>
        /// <typeparam name="TProperty">
        ///     The type of the property.
        /// </typeparam>
        /// <param name="propertyGetterExpression">
        ///     The lambda expression in the following form: <c>(SomeType x) => x.Property</c>.
        /// </param>
        /// <returns>
        ///     The name of the property.
        /// </returns>
        public static string GetPropertyName<TObject, TProperty>(
            Expression<Func<TObject, TProperty>> propertyGetterExpression)
        {
            var propertyInfo = GetPropertyInfo(propertyGetterExpression);
            return propertyInfo.Name;
        }

        /// <summary>
        ///     Gets the type-qualified name of the property specified by the lambda expression.
        /// </summary>
        /// <typeparam name="TObject">
        ///     The type containing the property.
        /// </typeparam>
        /// <typeparam name="TProperty">
        ///     The type of the property.
        /// </typeparam>
        /// <param name="propertyGetterExpression">
        ///     The lambda expression in the following form: <c>(SomeType x) => x.Property</c>.
        /// </param>
        /// <returns>
        ///     The name of the property in the following form: <c>SomeType.Property</c>.
        /// </returns>
        public static string GetQualifiedPropertyName<TObject, TProperty>(
            Expression<Func<TObject, TProperty>> propertyGetterExpression)
        {
            var propertyInfo = GetPropertyInfo(propertyGetterExpression);
            return typeof(TObject).Name + Type.Delimiter + propertyInfo.Name;
        }

        /// <summary>
        ///     Gets the <see cref="PropertyInfo"/> of the static property specified by the lambda expression.
        /// </summary>
        /// <typeparam name="TProperty">
        ///     The type of the static property.
        /// </typeparam>
        /// <param name="propertyGetterExpression">
        ///     The lambda expression in the following form: <c>() => propertyExpression</c>.
        /// </param>
        /// <returns>
        ///     The <see cref="PropertyInfo"/> containing information about the required static property.
        /// </returns>
        public static PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<TProperty>> propertyGetterExpression)
        {
            #region Argument Check

            if (propertyGetterExpression == null)
            {
                throw new ArgumentNullException("propertyGetterExpression");
            }

            #endregion

            var memberExpression = propertyGetterExpression.Body as MemberExpression;
            if ((memberExpression == null) || (memberExpression.NodeType != ExpressionType.MemberAccess))
            {
                throw new ArgumentException(
                    string.Format(InvalidExpressionMessageAutoFmt, propertyGetterExpression),
                    "propertyGetterExpression");
            }

            var result = memberExpression.Member as PropertyInfo;
            if (result == null)
            {
                throw new ArgumentException(
                    string.Format(InvalidExpressionMessageAutoFmt, propertyGetterExpression),
                    "propertyGetterExpression");
            }

            if (result.DeclaringType == null)
            {
                throw new ArgumentException(
                    string.Format(InvalidExpressionMessageAutoFmt, propertyGetterExpression),
                    "propertyGetterExpression");
            }

            if (memberExpression.Expression == null)
            {
                var accessor = result.GetGetMethod(true) ?? result.GetSetMethod(true);
                if ((accessor == null) || !accessor.IsStatic)
                {
                    throw new ArgumentException(
                        string.Format(InvalidExpressionMessageAutoFmt, propertyGetterExpression),
                        "propertyGetterExpression");
                }
            }

            return result;
        }

        /// <summary>
        ///     Gets the name of the static property specified by the lambda expression.
        /// </summary>
        /// <typeparam name="TProperty">
        ///     The type of the static property.
        /// </typeparam>
        /// <param name="propertyGetterExpression">
        ///     The lambda expression in the following form: <c>() => propertyExpression</c>.
        /// </param>
        /// <returns>
        ///     The name of the static property.
        /// </returns>
        public static string GetPropertyName<TProperty>(Expression<Func<TProperty>> propertyGetterExpression)
        {
            var propertyInfo = GetPropertyInfo(propertyGetterExpression);
            return propertyInfo.Name;
        }

        /// <summary>
        ///     Gets the name of the static property specified by the lambda expression.
        /// </summary>
        /// <typeparam name="TProperty">
        ///     The type of the static property.
        /// </typeparam>
        /// <param name="propertyGetterExpression">
        ///     The lambda expression in the following form: <c>() => propertyExpression</c>.
        /// </param>
        /// <returns>
        ///     The name of the static property.
        /// </returns>
        public static string GetQualifiedPropertyName<TProperty>(
            Expression<Func<TProperty>> propertyGetterExpression)
        {
            var propertyInfo = GetPropertyInfo(propertyGetterExpression);
            return propertyInfo.DeclaringType.Name + Type.Delimiter + propertyInfo.Name;
        }

        #endregion
    }
}