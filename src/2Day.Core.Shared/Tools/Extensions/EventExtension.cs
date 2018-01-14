using System;
using System.ComponentModel;

namespace Chartreuse.Today.Core.Shared.Tools.Extensions
{
    /// <summary>
    /// Provides extension methods to facilitate raising events
    /// </summary>
    public static class EventExtension
    {
        /// <summary>
        /// Raise an event with a given EventArgs
        /// </summary>
        /// <param name="handler">EventHandler to raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Argument of the event</param>
        public static void Raise(this EventHandler handler, object sender, EventArgs e)
        {
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        /// <summary>
        /// Raise an event with empty EventArgs
        /// </summary>
        /// <param name="handler">EventHandler to raised</param>
        /// <param name="sender">Sender of the event</param>
        public static void Raise(this EventHandler handler, object sender)
        {
            if (handler != null)
            {
                handler(sender, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raise an event with a generic EventHandler
        /// </summary>
        /// <typeparam name="T">Type of EventArgs to use</typeparam>
        /// <param name="handler">EventHandler to raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Argument of the event</param>
        public static void Raise<T>(this EventHandler<T> handler, object sender, T e)
            where T : EventArgs
        {
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        /// <summary>
        /// Raise an event with a generic EventHandler
        /// </summary>
        /// <param name="handler">EventHandler to raised</param>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e"></param>
        public static void Raise(this PropertyChangedEventHandler handler, object sender, PropertyChangedEventArgs e)
        {
            if (handler != null)
            {
                handler(sender, e);
            }
        }

    }
}
