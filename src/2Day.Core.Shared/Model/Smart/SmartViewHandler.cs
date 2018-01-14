using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public class SmartViewHandler
    {
        public List<SmartViewBlockRule> Blocks { get; }

        public SmartViewMatchType Match { get; }

        public bool ShowCompletedTasks { get; }

        public SmartViewHandler(SmartViewMatchType match, IEnumerable<SmartViewBlockRule> blocks)
        {
            if (blocks == null)
                throw new ArgumentNullException("blocks");

            this.Match = match;
            this.Blocks = new List<SmartViewBlockRule>(blocks);

            this.ShowCompletedTasks = this.Blocks.Any(b => b.Rules.Any(r => r.Field == SmartViewField.Completed));
        }

        public bool IsMatch(ITask task)
        {
            // might happen when a task is deleted
            if (task.Folder == null)
                return false;

            bool isMatch = false;
            switch (this.Match)
            {
                case SmartViewMatchType.All:
                    isMatch = true;
                    // match AND (match OR match) AND mach
                    foreach (var blockRule in this.Blocks)
                        isMatch &= blockRule.IsMatch(task);
                    break;
                case SmartViewMatchType.Any:
                    // match OR (match AND match) OR match
                    foreach (var blockRule in this.Blocks)
                        isMatch |= blockRule.IsMatch(task);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return isMatch;
        }

        public static SmartViewHandler FromString(string content)
        {
            if (content == null)
                throw new ArgumentNullException("content");
            if (!content.StartsWith("(") || !content.EndsWith(")"))
                throw new ArgumentException("Content must start with '(' end end with ')'");

            List<SmartViewBlockRule> blocks = new List<SmartViewBlockRule>();
            SmartViewMatchType? match = content.Contains(") AND (") ? SmartViewMatchType.All : SmartViewMatchType.Any;

            int currentPosition = 0;
            int blockLength = content.IndexOf(")");

            while (currentPosition < content.Length && blockLength > 0)
            {
                List<SmartViewRule> rules = new List<SmartViewRule>();
                SmartViewMatchType blockMatch;

                string blockContent = content.Substring(currentPosition + 1, blockLength - 1);
                string[] rulesContent;

                if (blockContent.Contains("OR") && blockContent.Contains("AND"))
                {
                    throw new ArgumentException("Block cannot contains both OR and AND");
                }
                else if (blockContent.Contains("OR"))
                {
                    blockMatch = SmartViewMatchType.Any;
                    rulesContent = blockContent.Split(new[]{ "OR" }, StringSplitOptions.None);
                }
                else if (blockContent.Contains("AND"))
                {
                    blockMatch = SmartViewMatchType.All;
                    rulesContent = blockContent.Split(new[] {"AND"}, StringSplitOptions.None);
                }
                else
                {
                    rulesContent = new[] {blockContent};
                    blockMatch = SmartViewMatchType.Any;
                }

                for (int i = 0; i < rulesContent.Length; i++)
                {
                    string ruleContent = rulesContent[i];
                    var inner = ruleContent.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    if (inner.Length < 3)
                        throw new ArgumentException("Invalid rule");

                    string field = inner[0];
                    string filter = inner[1];
                    string value = inner[2];
                    for (int j = 3; j < inner.Length; j++)
                    {
                        value += " " + inner[j];
                    }

                    var smartViewField = field.ParseAsEnum<SmartViewField>();
                    var smartViewFilter = filter.ParseAsEnum<SmartViewFilter>();

                    rules.Add(SmartViewHelper.GetSupportedRule(smartViewField).Read(smartViewFilter, smartViewField, value));
                }

                blocks.Add(new SmartViewBlockRule(rules, blockMatch));

                // ( ) AND ( )
                currentPosition += blockLength + 1;
                currentPosition += content.Substring(currentPosition).IndexOf("(");
                blockLength = content.Substring(currentPosition).IndexOf(")");
            }

            return new SmartViewHandler(match.Value, blocks);
        }

        public string AsString()
        {
            // (xx OR yy) AND ()
            var builder = new StringBuilder();

            for (int i = 0; i < this.Blocks.Count; i++)
            {
                var block = this.Blocks[i];
                builder.Append("(");

                for (int j = 0; j < block.Rules.Count; j++)
                {
                    var rule = block.Rules[j];
                    builder.Append(rule.AsString());

                    if (j < block.Rules.Count - 1)
                    {
                        if (block.Match == SmartViewMatchType.All)
                            builder.Append(" AND ");
                        else if (block.Match == SmartViewMatchType.Any)
                            builder.Append(" OR ");
                    }
                }

                builder.Append(")");

                if (i < this.Blocks.Count - 1)
                {
                    if (this.Match == SmartViewMatchType.All)
                        builder.Append(" AND ");
                    else if (this.Match == SmartViewMatchType.Any)
                        builder.Append(" OR ");
                }
            }

            return builder.ToString();

        }
    }
}