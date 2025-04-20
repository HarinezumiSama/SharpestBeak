using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Omnifactotum.Annotations;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Size = System.Drawing.Size;

namespace SharpestBeak.UI;

public sealed class SizeObject : DependencyObject, ICustomTypeDescriptor
{
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

    private static readonly string[] VisibleNames = [WidthProperty.Name, HeightProperty.Name];

    public SizeObject(Size size) => SetSize(size);

    [DebuggerNonUserCode]
    public static DependencyProperty AsStringProperty { get; } = AsStringKey.DependencyProperty;

    [PropertyOrder(1)]
    public int Width
    {
        get => (int)GetValue(WidthProperty);

        [UsedImplicitly]
        set => SetValue(WidthProperty, value);
    }

    [PropertyOrder(2)]
    public int Height
    {
        get => (int)GetValue(HeightProperty);

        [UsedImplicitly]
        set => SetValue(HeightProperty, value);
    }

    [Browsable(false)]
    public string AsString
    {
        get => (string)GetValue(AsStringProperty);

        private set => SetValue(AsStringKey, value);
    }

    public override string ToString() => $"{Width} x {Height}";

    public Size ToSize() => new(Width, Height);

    public void SetSize(Size size)
    {
        Width = size.Width;
        Height = size.Height;
    }

    public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(this, true);

    public string GetClassName() => TypeDescriptor.GetClassName(this, true);

    public string GetComponentName() => TypeDescriptor.GetComponentName(this, true);

    public TypeConverter GetConverter() => TypeDescriptor.GetConverter(this, true);

    public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(this, true);

    public PropertyDescriptor GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(this, true);

    public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(this, editorBaseType, true);

    public EventDescriptorCollection GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(this, attributes, true);

    public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(this, true);

    //// Ignoring attributes
    public PropertyDescriptorCollection GetProperties(Attribute[] attributes) => GetProperties();

    public PropertyDescriptorCollection GetProperties()
    {
        var properties = TypeDescriptor.GetProperties(this, true);

        var filteredProperties = properties
            .OfType<PropertyDescriptor>()
            .Where(descriptor => VisibleNames.Contains(descriptor.Name))
            .ToArray();

        return new PropertyDescriptorCollection(filteredProperties);
    }

    public object GetPropertyOwner(PropertyDescriptor pd) => this;

    private static void OnWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        => (obj as SizeObject).EnsureNotNull().UpdateAsStringProperty();

    private static void OnHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        => (obj as SizeObject).EnsureNotNull().UpdateAsStringProperty();

    private void UpdateAsStringProperty() => AsString = ToString();
}