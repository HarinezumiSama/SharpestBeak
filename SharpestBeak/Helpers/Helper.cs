using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using Omnifactotum;

//// The type is placed intentionally in the root namespace to ease access from other projects and namespaces
//// ReSharper disable once CheckNamespace
namespace SharpestBeak;

public static class Helper
{
    public static readonly bool DefaultInheritAttributeParameter = true;

    public static IValueConverter DebugConverterInstance { get; } = new DebugConverter();

    public static DependencyProperty RegisterDependencyProperty<TObject, TProperty>(
        Expression<Func<TObject, TProperty>> propertyGetterExpression,
        PropertyMetadata typeMetadata = null,
        ValidateValueCallback validateValueCallback = null)
    {
        var propertyInfo = Factotum.GetPropertyInfo(propertyGetterExpression);

        if (propertyInfo.DeclaringType != typeof(TObject))
        {
            throw new ArgumentException(
                "Inconsistency between property expression and declaring object type.",
                nameof(propertyGetterExpression));
        }

        return DependencyProperty.Register(
            propertyInfo.Name,
            propertyInfo.PropertyType,
            propertyInfo.DeclaringType.EnsureNotNull(),
            typeMetadata,
            validateValueCallback);
    }

    public static DependencyPropertyKey RegisterReadOnlyDependencyProperty<TObject, TProperty>(
        Expression<Func<TObject, TProperty>> propertyGetterExpression,
        PropertyMetadata typeMetadata = null,
        ValidateValueCallback validateValueCallback = null)
    {
        var propertyInfo = Factotum.GetPropertyInfo(propertyGetterExpression);

        if (propertyInfo.DeclaringType != typeof(TObject))
        {
            throw new ArgumentException(
                "Inconsistency between property expression and declaring object type.",
                nameof(propertyGetterExpression));
        }

        return DependencyProperty.RegisterReadOnly(
            propertyInfo.Name,
            propertyInfo.PropertyType,
            propertyInfo.DeclaringType.EnsureNotNull(),
            typeMetadata,
            validateValueCallback);
    }

    private sealed class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}