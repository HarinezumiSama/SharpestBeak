using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Size = System.Drawing.Size;

namespace SharpestBeak.UI
{
    public sealed class SizeObject : DependencyObject, ICustomTypeDescriptor
    {
        #region Constants and Fields

        public static readonly DependencyProperty WidthProperty =
            Helper.RegisterDependencyProperty(
                (SizeObject control) => control.Width,
                new PropertyMetadata(OnWidthChanged));

        public static readonly DependencyProperty HeightProperty =
            Helper.RegisterDependencyProperty(
                (SizeObject control) => control.Height,
                new PropertyMetadata(OnHeightChanged));

        private static readonly DependencyPropertyKey AsStringKey =
            Helper.RegisterReadOnlyDependencyProperty((SizeObject control) => control.AsString);

        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess",
            Justification = "Such field order is required in this case (AsStringProperty depends on AsStringKey).")]
        public static readonly DependencyProperty AsStringProperty = AsStringKey.DependencyProperty;

        private static readonly string[] VisibleNames = { WidthProperty.Name, HeightProperty.Name };

        #endregion

        #region Constructors

        public SizeObject(Size size)
        {
            SetSize(size);
        }

        #endregion

        #region Public Properties

        [PropertyOrder(1)]
        public int Width
        {
            get
            {
                return (int)GetValue(WidthProperty);
            }

            set
            {
                SetValue(WidthProperty, value);
            }
        }

        [PropertyOrder(2)]
        public int Height
        {
            get
            {
                return (int)GetValue(HeightProperty);
            }

            set
            {
                SetValue(HeightProperty, value);
            }
        }

        [Browsable(false)]
        public string AsString
        {
            get
            {
                return (string)GetValue(AsStringProperty);
            }

            private set
            {
                SetValue(AsStringKey, value);
            }
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} x {1}", this.Width, this.Height);
        }

        public Size ToSize()
        {
            return new Size(this.Width, this.Height);
        }

        public void SetSize(Size size)
        {
            this.Width = size.Width;
            this.Height = size.Height;
        }

        #endregion

        #region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            // Ignoring attributes
            return GetProperties();
        }

        public PropertyDescriptorCollection GetProperties()
        {
            var properties = TypeDescriptor.GetProperties(this, true);
            var filteredProperties = properties
                .OfType<PropertyDescriptor>()
                .Where(descriptor => VisibleNames.Contains(descriptor.Name))
                .ToArray();
            return new PropertyDescriptorCollection(filteredProperties);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion

        #region Private Methods

        private static void OnWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var self = (obj as SizeObject).EnsureNotNull();
            self.UpdateAsStringProperty();
        }

        private static void OnHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var self = (obj as SizeObject).EnsureNotNull();
            self.UpdateAsStringProperty();
        }

        private void UpdateAsStringProperty()
        {
            this.AsString = ToString();
        }

        #endregion
    }
}