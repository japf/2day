using System;
using System.Collections.Generic;
using System.Linq;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public class SmartViewHelper
    {
        private static readonly List<SmartViewRule> supportedRules = new List<SmartViewRule>
        {
            new SmartViewPriorityRule(),
            new SmartViewContextRule(),
            new SmartViewFolderRule(),
            new SmartViewStringRule(),
            new SmartViewDateRule(),
            new SmartViewBoolRule(),
            new SmartViewTagRule(),
            new SmartViewProgressRule()
        };

        public static SmartViewRule GetSupportedRule(SmartViewField field)
        {
            var supportedRule = supportedRules.FirstOrDefault(r => r.KnowsField(field));

            if (supportedRule == null)
                throw new NotSupportedException(string.Format("No rule found for field: {0}", field));

            return supportedRule;
        }

        public static IEnumerable<SmartViewFilter> GetCompatibleFilter(SmartViewField field)
        {
            var supportedFilters = GetSupportedRule(field).SupportedFilters.ToList();

            if (field == SmartViewField.Added || field == SmartViewField.Modified)
            {
                supportedFilters.Remove(SmartViewFilter.Yes);
                supportedFilters.Remove(SmartViewFilter.No);
                supportedFilters.Remove(SmartViewFilter.Exists);
                supportedFilters.Remove(SmartViewFilter.DoesNotExist);
            }

            return supportedFilters;
        }

        public static SmartViewRule BuildRule(SmartViewFilter filter, SmartViewField field, object param)
        {
            var rule = GetSupportedRule(field);
            if (rule is SmartViewPriorityRule)
            {
                TaskPriority priority;
                if (param is TaskPriority)
                    priority = (TaskPriority) param;
                else
                    priority = TaskPriorityConverter.FromDescription((string) param);

                return new SmartViewPriorityRule(filter, field, priority);
            }
            else if (rule is SmartViewStringRule)
            {
                return new SmartViewStringRule(filter, field, (string)param);
            }
            else if (rule is SmartViewDateRule)
            {
                DateTime datetime = param is DateTime ? (DateTime) param : DateTime.MinValue;
                if (datetime != DateTime.MinValue)
                {
                    return new SmartViewDateRule(filter, field, new SmartViewDateParameter(datetime));
                }
                else
                {
                    int value = 0;
                    if (param is string)
                    {
                        string content = (string)param;
                        int.TryParse(content, out value);    
                    }
                    else if (param is int)
                    {
                        value = (int) param;
                    }
                    
                    value = Math.Max(0, value);
                    return new SmartViewDateRule(filter, field, new SmartViewDateParameter(value));
                }
            }
            else if (rule is SmartViewBoolRule)
            {
                bool value = false;
                if (param is bool)
                    value = (bool) param;

                return new SmartViewBoolRule(filter, field, value);
            }
            else if (rule is SmartViewFolderRule)
            {
                return new SmartViewFolderRule(filter, field, (string)param);
            }
            else if (rule is SmartViewContextRule)
            {
                string parameter = (string) param;
                if (string.IsNullOrWhiteSpace(parameter))
                    parameter = "0";
                
                return new SmartViewContextRule(filter, field, parameter);
            }
            else if (rule is SmartViewProgressRule)
            {
                double value = 0;
                if (param is string)
                {
                    string content = (string)param;
                    double.TryParse(content, out value);
                }
                else if (param is int)
                {
                    value = (int)param;
                }
                else if (param is double)
                {
                    value = (double) param;
                }

                return new SmartViewProgressRule(filter, field, value);
            }
            else if (rule is SmartViewTagRule)
            {
                string parameter = (string)param;
                if (string.IsNullOrWhiteSpace(parameter))
                    parameter = "0";

                return new SmartViewTagRule(filter, field, parameter);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public static SmartViewEditMode GetEditMode(SmartViewFilter filter, SmartViewField field)
        {
            switch (filter)
            {
                case SmartViewFilter.Contains:
                case SmartViewFilter.BeginsWith:
                case SmartViewFilter.EndsWith:
                case SmartViewFilter.DoesNotContains:
                    return new SmartViewEditMode(SmartViewEditType.Text);
                case SmartViewFilter.Is:
                case SmartViewFilter.IsNot:
                    return GetEditModeIs(field);
                case SmartViewFilter.IsMoreThan:
                case SmartViewFilter.IsLessThan:
                    return new SmartViewEditMode(SmartViewEditType.Numeric);
                case SmartViewFilter.IsAfter:
                case SmartViewFilter.IsBefore:
                    return new SmartViewEditMode(SmartViewEditType.Date);
                case SmartViewFilter.WasInTheLast:
                case SmartViewFilter.WasNotInTheLast:
                case SmartViewFilter.IsInTheNext:
                case SmartViewFilter.IsNotInTheNext:
                    return new SmartViewEditMode(SmartViewEditType.Numeric, StringResources.SmartView_DaysPeriod);
                case SmartViewFilter.IsIn:
                case SmartViewFilter.IsNotIn:
                    return new SmartViewEditMode(SmartViewEditType.Numeric, StringResources.SmartView_Days);
                case SmartViewFilter.Was:
                case SmartViewFilter.WasNot:
                    return new SmartViewEditMode(SmartViewEditType.Numeric, StringResources.SmartView_DaysAgo);
                case SmartViewFilter.Exists:
                case SmartViewFilter.DoesNotExist:
                case SmartViewFilter.Yes:
                case SmartViewFilter.No:
                case SmartViewFilter.IsYesterday:
                case SmartViewFilter.IsToday:
                case SmartViewFilter.IsTomorrow:
                    return new SmartViewEditMode(SmartViewEditType.None);
                default:
                    throw new ArgumentOutOfRangeException("filter");
            }
        }

        private static SmartViewEditMode GetEditModeIs(SmartViewField field)
        {
            switch (field)
            {
                case SmartViewField.Title:
                case SmartViewField.Note:
                case SmartViewField.Tags:
                    return new SmartViewEditMode(SmartViewEditType.Tag);
                case SmartViewField.Folder:
                    return new SmartViewEditMode(SmartViewEditType.Folder);
                case SmartViewField.Context:
                    return new SmartViewEditMode(SmartViewEditType.Context);
                case SmartViewField.Priority:
                    return new SmartViewEditMode(SmartViewEditType.Priority);
                case SmartViewField.Progress:
                    return new SmartViewEditMode(SmartViewEditType.Numeric);
                case SmartViewField.Added:
                case SmartViewField.Modified:
                case SmartViewField.Completed:
                case SmartViewField.Due:
                case SmartViewField.Start:
                    return new SmartViewEditMode(SmartViewEditType.Date);
                case SmartViewField.HasAlarm:
                case SmartViewField.HasRecurrence:
                case SmartViewField.HasSubtasks:
                case SmartViewField.IsLate:
                    return new SmartViewEditMode(SmartViewEditType.None);
                default:
                    throw new ArgumentOutOfRangeException("field");
            }
        }

        public static string GetMatchTypeDisplay(SmartViewMatchType type)
        {
            if (type == SmartViewMatchType.All)
                return StringResources.SmartView_And;
            else
                return StringResources.SmartView_Or;
        }
    }
}
