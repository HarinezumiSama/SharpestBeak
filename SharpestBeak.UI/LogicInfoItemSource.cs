using System;
using System.Globalization;
using System.Linq;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace SharpestBeak.UI
{
    public sealed class LogicInfoItemSource : IItemsSource
    {
        #region IItemsSource Members

        ItemCollection IItemsSource.GetValues()
        {
            var result = new ItemCollection();

            result.AddRange(
                LogicManager.Instance.Logics.Select(
                    info => new Item
                    {
                        DisplayName = string.Format(
                            CultureInfo.InvariantCulture,
                            "{0} ({1})",
                            info.Caption,
                            info.Type.Name),
                        Value = info
                    }));

            return result;
        }

        #endregion
    }
}