using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chartreuse.Today.App.Tools.Behavior
{
    public static class FilterAutoSuggestBox
    {
        public static IEnumerable<string> GetSuggestionsSource(DependencyObject obj)
        {
            return (IEnumerable<string>)obj.GetValue(SuggestionsSourceProperty);
        }

        public static void SetSuggestionsSource(DependencyObject obj, IEnumerable<string> value)
        {
            obj.SetValue(SuggestionsSourceProperty, value);
        }

        public static readonly DependencyProperty SuggestionsSourceProperty = DependencyProperty.RegisterAttached(
            "SuggestionsSource",
            typeof(IEnumerable<string>),
            typeof(FilterAutoSuggestBox),
            new PropertyMetadata(null, PropertyChangedCallback));
        
        private static void PropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var autoSuggestBox = (AutoSuggestBox)sender;

            autoSuggestBox.TextChanged += OnTextChanged;
        }
        
        private static void OnTextChanged(AutoSuggestBox autoSuggestBox, AutoSuggestBoxTextChangedEventArgs e)
        {
            IEnumerable<string> suggestions = GetSuggestionsSource(autoSuggestBox);
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput && suggestions != null)
            {
                if (!string.IsNullOrWhiteSpace(autoSuggestBox.Text))
                    autoSuggestBox.ItemsSource = suggestions.Where(i => i.ToLowerInvariant().Contains(autoSuggestBox.Text.ToLowerInvariant()));
                else
                    autoSuggestBox.ItemsSource = Enumerable.Empty<string>();
            }
        }
    }
}
