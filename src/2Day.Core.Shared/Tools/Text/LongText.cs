using System;
using System.Collections.Generic;
using System.Text;

namespace Chartreuse.Today.Core.Shared.Tools.Text
{
    /// <summary>
    /// Represents a very long text. The very long text
    /// is broken into pieces, or more precisely,
    /// a List of TextItems. The List will be bound
    /// to a ListBox, and each TextItem will be bound
    /// to a ListItem.
    /// </summary>
    public class LongText
    {
        /// <summary>
        /// A parameter that specifies how large a block of text,
        /// or TextItem, should be. Play around with this parameter.
        /// The higher this parameter value, the smaller the number of
        /// TextItems, the lower this parameter value, the larger the
        /// number of TextItems. There is an inversely proportional
        /// relationship. If you have a high number of TextItems, a 
        /// lot of ListItem (therefore TextBlocks) will be created,
        /// and the UI will be slow loading. Empirically, setting this
        /// value to 1500 works reasonably well.
        /// </summary>
        private const int MAX = 1500;

        /// <summary>
        /// A List of TextItems.
        /// </summary>
        private List<TextItem> texts;

        /// <summary>
        /// List of TextItems.
        /// </summary>
        public List<TextItem> Texts
        {
            get
            {
                if (null == this.texts)
                    this.texts = new List<TextItem>();
                return this.texts;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="str">The string that will be broken up into TextItems.</param>
        public LongText(string str)
        {
            this.texts = this.ToList(str);
        }

        /// <summary>
        /// Converts a (very long) string to a List of TextItems.
        /// </summary>
        /// <param name="str">A (very long) string.</param>
        /// <returns>List of TextItems.</returns>
        public List<TextItem> ToList(string str)
        {
            List<TextItem> texts = new List<TextItem>();

            string[] tokens = str.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            StringBuilder sb = new StringBuilder();
            foreach (string token in tokens)
            {
                sb.Append(token);

                if (sb.Length >= MAX)
                {
                    string text = sb.ToString();
                    texts.Add(new TextItem(text));
                    sb.Length = 0;
                }
                else
                {
                    sb.Append("\r\n");
                }
            }

            if (sb.Length > 0)
            {
                string text = sb.ToString();
                texts.Add(new TextItem(text));
            }

            return texts;
        }
    }
}
