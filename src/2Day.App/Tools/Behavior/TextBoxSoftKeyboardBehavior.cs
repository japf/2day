using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Chartreuse.Today.App.Tools.UI;
using Chartreuse.Today.Core.Universal.Tools;

namespace Chartreuse.Today.App.Tools.Behavior
{
    /// <summary>
    /// A dirty behavior which uses a hack to hide the virtual soft keyboard (if present) when the 
    /// Enter key is pressed on a textbox. It does so by inserting a dummy 0-sized button in the parent
    /// panel which owns the textbox.
    /// </summary>
    public static class TextBoxSoftKeyboardBehavior
    {
        private static readonly Dictionary<TextBox, ButtonPanel> PanelsCache = new Dictionary<TextBox, ButtonPanel>();

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled", 
            typeof(bool), 
            typeof(TextBoxSoftKeyboardBehavior), new PropertyMetadata(false, OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textbox = d as TextBox;
            if (textbox == null)
                throw new NotSupportedException("This attached property must be set on a TextBox control");

            if ((bool) e.NewValue)
            {
                textbox.KeyDown += OnTextboxKeyDown;
                textbox.Unloaded += OnTextboxUnloaded;
                textbox.GotFocus += OnTextboxGotFocus;
            }
            else
            {
            }
        }

        private static void OnTextboxGotFocus(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;

            if (PanelsCache.ContainsKey(textbox))
                return;

            // when textbox got focus insert a zero sized button right after it
            // we will move the focus to this hidden button in order to hide the
            // virtual keyboard
            Panel panel = TreeHelper.FindVisualAncestor<Panel>(textbox);
            if (panel == null)
                return;

            int childIndex = panel.Children.IndexOf(textbox);
            var dummyButton = new Button { Width = 0, Height = 0 };
            dummyButton.GotFocus += OnDummyButtonGotFocus;

            panel.Children.Insert(childIndex + 1, dummyButton);
            PanelsCache.Add(textbox, new ButtonPanel { Button = dummyButton, Panel = panel });
        }

        private static void OnTextboxUnloaded(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;

            // when the textbox is unloaded, cleanup event handlers
            textbox.KeyDown -= OnTextboxKeyDown;
            textbox.Unloaded -= OnTextboxUnloaded;
            textbox.GotFocus -= OnTextboxGotFocus;

            TryRemoveDummyButton(textbox);
        }

        private static void OnTextboxKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter || e.Key == VirtualKey.Tab)
            {
                // Ctrl + Enter is a special keyboard shortcut, so it's ignored here
                if (WinKeyboardHelper.IsKeyDown(VirtualKey.Control))
                    return;

                var textbox = (TextBox)sender;

                Panel panel = TreeHelper.FindVisualAncestor<Panel>(textbox);
                if (panel == null)
                    throw new NotSupportedException("Could not find parent panel");

                // move the focus to the dummy button so that it hides the virtual keyboard
                var entry = PanelsCache[textbox];
                entry.Button.Focus(FocusState.Keyboard);
            }
        }

        private static void OnDummyButtonGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            var button = (Button)sender;

            Panel panel = TreeHelper.FindVisualAncestor<Panel>(button);
            if (panel == null)
                throw new NotSupportedException("Could not find parent panel");

            // asynchronously moves the focus to the element right after the dummy button
            int index = panel.Children.IndexOf(button);
            if (index >= 0 && (index + 1) < panel.Children.Count)
            {
                var target = panel.Children[index + 1] as Control;
                if (target != null)
                {
                    target.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => target.Focus(FocusState.Keyboard));
                }
            }

            // and remove it
            foreach (var kvp in PanelsCache.ToList())
            {
                if (kvp.Value.Button == sender)
                    TryRemoveDummyButton(kvp.Key);
            }
        }

        private static void TryRemoveDummyButton(TextBox textbox)
        {
            if (PanelsCache.ContainsKey(textbox))
            {
                var entry = PanelsCache[textbox];
                entry.Panel.Children.Remove(entry.Button);
                PanelsCache.Remove(textbox);
            }
        }

        private struct ButtonPanel
        {
            public Button Button { get; set; }
            public Panel Panel { get; set; }
        }
    }
}
