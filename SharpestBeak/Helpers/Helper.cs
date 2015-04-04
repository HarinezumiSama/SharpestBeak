using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using Omnifactotum;

//// The type is placed intentionally in the root namespace to ease access from other projects and namespaces
//// ReSharper disable once CheckNamespace
namespace SharpestBeak
{
    public static class Helper
    {
        #region Constants and Fields

        public static readonly bool DefaultInheritAttributeParameter = true;

        private static readonly IValueConverter DebugConverterInstanceField = new DebugConverter();

        #endregion

        #region Public Properties

        public static IValueConverter DebugConverterInstance
        {
            [DebuggerStepThrough]
            get
            {
                return DebugConverterInstanceField;
            }
        }

        #endregion

        #region Public Methods

        public static DependencyProperty RegisterDependencyProperty<TObject, TProperty>(
            Expression<Func<TObject, TProperty>> propertyGetterExpression,
            PropertyMetadata typeMetadata = null,
            ValidateValueCallback validateValueCallback = null)
        {
            var propertyInfo = Factotum.GetPropertyInfo(propertyGetterExpression);

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

        public static DependencyPropertyKey RegisterReadOnlyDependencyProperty<TObject, TProperty>(
            Expression<Func<TObject, TProperty>> propertyGetterExpression,
            PropertyMetadata typeMetadata = null,
            ValidateValueCallback validateValueCallback = null)
        {
            var propertyInfo = Factotum.GetPropertyInfo(propertyGetterExpression);

            if (propertyInfo.DeclaringType != typeof(TObject))
            {
                throw new ArgumentException(
                    @"Inconsistency between property expression and declaring object type.",
                    "propertyGetterExpression");
            }

            return DependencyProperty.RegisterReadOnly(
                propertyInfo.Name,
                propertyInfo.PropertyType,
                propertyInfo.DeclaringType.EnsureNotNull(),
                typeMetadata,
                validateValueCallback);
        }

        #endregion

        #region DebugConverter Class

        private sealed class DebugConverter : IValueConverter
        {
            #region IValueConverter Members

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }

            #endregion
        }

        #endregion
    }
}