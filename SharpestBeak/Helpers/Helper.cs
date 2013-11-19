using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// The type is placed intentionally in the root namespace to ease access from other projects and namespaces
using System.Windows;

// ReSharper disable once CheckNamespace
namespace SharpestBeak
{
    public static class Helper
    {
        #region Constants and Fields

        public static readonly bool DefaultInheritAttributeParameter = true;

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

        public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array)
        {
            #region Argument Check

            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            #endregion

            return new ReadOnlyCollection<T>(array);
        }

        public static IEnumerable<T> AsCollection<T>(this T value)
        {
            yield return value;
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

        public static void EnsureDefined(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (!Enum.IsDefined(value.GetType(), value))
            {
                throw new ArgumentOutOfRangeException("value", value, "Invalid enumeration value.");
            }
        }

        public static void Exchange<T>(ref T value1, ref T value2)
        {
            var temp = value1;
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
            return (propertyInfo.DeclaringType ?? propertyInfo.ReflectedType).Name
                + Type.Delimiter
                + propertyInfo.Name;
        }

        public static TAttribute GetSoleAttribute<TAttribute>(
            this ICustomAttributeProvider attributeProvider,
            bool inherit)
        {
            #region Argument Check

            if (attributeProvider == null)
            {
                throw new ArgumentNullException("attributeProvider");
            }

            #endregion

            return GetSoleAttributeInternal<TAttribute>(attributeProvider, inherit, Enumerable.SingleOrDefault);
        }

        public static TAttribute GetSoleAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider)
        {
            return GetSoleAttribute<TAttribute>(attributeProvider, DefaultInheritAttributeParameter);
        }

        public static TAttribute GetSoleAttributeStrict<TAttribute>(
            this ICustomAttributeProvider attributeProvider,
            bool inherit)
        {
            #region Argument Check

            if (attributeProvider == null)
            {
                throw new ArgumentNullException("attributeProvider");
            }

            #endregion

            return GetSoleAttributeInternal<TAttribute>(attributeProvider, inherit, Enumerable.Single);
        }

        public static TAttribute GetSoleAttributeStrict<TAttribute>(this ICustomAttributeProvider attributeProvider)
        {
            return GetSoleAttributeStrict<TAttribute>(attributeProvider, DefaultInheritAttributeParameter);
        }

        public static DependencyProperty RegisterDependencyProperty<TObject, TProperty>(
            Expression<Func<TObject, TProperty>> propertyGetterExpression,
            PropertyMetadata typeMetadata = null,
            ValidateValueCallback validateValueCallback = null)
        {
            var propertyInfo = GetPropertyInfo(propertyGetterExpression);

            if (propertyInfo.DeclaringType != typeof(TObject))
            {
                throw new ArgumentException(
                    @"Inconsistency between property expression and declaring object type.",
                    "propertyGetterExpression");
            }

            return DependencyProperty.Register(
                propertyInfo.Name,
                propertyInfo.PropertyType,
                propertyInfo.DeclaringType.EnsureNotNull(),
                typeMetadata,
                validateValueCallback);
        }

        #endregion

        #region Private Methods

        private static TAttribute GetSoleAttributeInternal<TAttribute>(
            this ICustomAttributeProvider attributeProvider,
            bool inherit,
            Func<IEnumerable<TAttribute>, TAttribute> getter)
        {
            #region Argument Check

            if (attributeProvider == null)
            {
                throw new ArgumentNullException("attributeProvider");
            }

            #endregion

            var attributes = attributeProvider.GetCustomAttributes(typeof(TAttribute), inherit).OfType<TAttribute>();
            return getter(attributes);
        }

        #endregion
    }
}