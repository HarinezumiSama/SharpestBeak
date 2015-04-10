using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Omnifactotum;

namespace SharpestBeak.UI
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Protected Methods

        protected void RaisePropertyChanged<TObject, TProperty>(
            Expression<Func<TObject, TProperty>> propertyExpression)
            where TObject : NotifyPropertyChangedBase
        {
            var propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
            {
                return;
            }

            var propertyName = Factotum.For<TObject>.GetPropertyName(propertyExpression);
            propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}