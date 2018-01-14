using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Collection;

namespace Chartreuse.Today.App.Tools.Converter
{
    public class CollectionViewConverter : IValueConverter
    {
        private static readonly Dictionary<object, ICollectionView> cache;

        static CollectionViewConverter()
        {
            cache = new Dictionary<object, ICollectionView>();
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (cache.ContainsKey(value))
                return cache[value];

            var collectionView = new CollectionViewSource
            {
                IsSourceGrouped = true,
            };

            var collection = value as SmartCollection<ITask>;
            if (collection != null)
                collectionView.Source = collection.Items;

            cache.Add(value, collectionView.View);

            return collectionView.View;            
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
