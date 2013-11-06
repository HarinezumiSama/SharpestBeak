using System;
using System.Linq;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace SharpestBeak.UI
{
    public sealed class LogicInfoItemSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            var result = new ItemCollection();
            result.AddRange(
                LogicManager.Instance.Logics.Select(info => new Item { DisplayName = info.Caption, Value = info }));
            return result;
        }
    }
}