using System;
using System.Collections.Generic;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public class SmartViewBlockRule
    {
        private readonly SmartViewMatchType match;
        private readonly List<SmartViewRule> rules;

        public List<SmartViewRule> Rules
        {
            get { return this.rules; }
        }

        public SmartViewMatchType Match
        {
            get { return this.match; }
        }

        public SmartViewBlockRule(IEnumerable<SmartViewRule> rules, SmartViewMatchType match)
        {
            if (rules == null)
                throw new ArgumentNullException("rules");

            this.match = match;
            this.rules = new List<SmartViewRule>(rules);
        }

        public bool IsMatch(ITask task)
        {
            bool isMatch;
            switch (this.Match)
            {
                case SmartViewMatchType.All:
                    isMatch = true;
                    foreach (var rule in this.Rules )
                        isMatch &= rule.IsMatch(task);
                    break;
                case SmartViewMatchType.Any:
                    isMatch = false;
                    foreach (var rule in this.Rules)
                        isMatch |= rule.IsMatch(task);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("match");
            }

            return isMatch;
        }
    }
}