using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Chartreuse.Today.App.Tools.UI
{
    /// <summary>
    /// Helper methods to go through the logical and the visual tree
    /// </summary>
    public static class TreeHelper
    {
        public static T FindParent<T>(this FrameworkElement element) where T : FrameworkElement
        {
            while (true)
            {
                if (element is T)
                {
                    return (T) element;
                }
                else if (element.Parent is FrameworkElement)
                {
                    element = (FrameworkElement) element.Parent;
                }
                else
                {
                    var visualParent = VisualTreeHelper.GetParent(element) as FrameworkElement;
                    if (visualParent != null)
                        element = visualParent;
                    else
                        return null;
                }
            }
        }

        public static TPage TryGetPageFromRootFrame<TPage>() where TPage : Page
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null)
            {
                TPage mainPage = rootFrame.Content as TPage;
                if (mainPage != null)
                    return mainPage;                
            }

            return default(TPage);
        }

        /// <summary>
        /// Gets the visual parent of the element
        /// </summary>
        /// <param name="node">The element to check</param>
        /// <returns>The visual parent</returns>
        public static FrameworkElement GetVisualParent(this FrameworkElement node)
        {
            return VisualTreeHelper.GetParent(node) as FrameworkElement;
        }
        
        /// <summary>
        /// Find all visual children of a given type
        /// </summary>
        /// <typeparam name="T">Type of the children to find</typeparam>
        /// <param name="obj">Source dependency object</param>
        /// <returns>A list of T objects that are visual children of the source dependency object</returns>
        public static List<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
        {
            List<T> matches = new List<T>();
            return FindVisualChildren(obj, matches);
        }

        /// <summary>
        /// Find the first visual child of a given type
        /// </summary>
        /// <typeparam name="T">Type of the visual child to retrieve</typeparam>
        /// <param name="obj">Source dependency object</param>
        /// <returns>First child that matches the given type</returns>
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }

                T childOfchild = FindVisualChild<T>(child);
                if (childOfchild != null)
                {
                    return childOfchild;
                }
            }

            return null;
        }

        /// <summary>
        /// Find the first visual ancestor of a given type
        /// </summary>
        /// <typeparam name="T">Type of the ancestor</typeparam>
        /// <param name="element">Source visual</param>
        /// <returns>First ancestor that matches the type in the visual tree</returns>
        public static T FindVisualAncestor<T>(UIElement element) where T : class
        {
            while (element != null && !(element is T))
            {
                element = (UIElement)VisualTreeHelper.GetParent(element);
            }

            return element as T;
        }
        
        public static T GetVisualAncestor<T>(FrameworkElement element, string name, int maxLevel) where T : class
        {
            int currentLevel = 0;
            while (element != null && currentLevel < maxLevel && element.Name != name)
            {
                element = (FrameworkElement)VisualTreeHelper.GetParent(element);

                if (element is T && element.Name == name)
                    break;

                currentLevel++;
            }

            if (currentLevel < maxLevel && element != null && element.Name == name)
                return element as T;
            else
                return default(T);
        }
        
        /// <summary>
        /// Find all visual children of a given type
        /// </summary>
        /// <typeparam name="T">Type of the children to find</typeparam>
        /// <param name="obj">Source dependency object</param>
        /// <param name="matches">List of matches</param>
        /// <returns>A list of T objects that are visual children of the source dependency object</returns>
        private static List<T> FindVisualChildren<T>(DependencyObject obj, List<T> matches) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject children = VisualTreeHelper.GetChild(obj, i);
                if (children != null && children is T)
                {
                    matches.Add((T)children);
                }
                FindVisualChildren(children, matches);
            }

            return matches;
        }        
    }
}
