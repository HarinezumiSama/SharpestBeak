using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Omnifactotum;

namespace SharpestBeak.UI;

public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void RaisePropertyChanged<TObject, TProperty>(
        Expression<Func<TObject, TProperty>> propertyExpression)
        where TObject : NotifyPropertyChangedBase
    {
        var propertyChanged = PropertyChanged;
        if (propertyChanged is null)
        {
            return;
        }

        var propertyName = Factotum.For<TObject>.GetPropertyName(propertyExpression);
        propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
}