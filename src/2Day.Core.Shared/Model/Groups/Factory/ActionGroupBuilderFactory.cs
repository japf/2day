using System;
using Chartreuse.Today.Core.Shared.Resources;

namespace Chartreuse.Today.Core.Shared.Model.Groups.Factory
{
    public class ActionGroupBuilderFactory : GroupBuilderFactoryBase
    {
        public ActionGroupBuilderFactory(IAbstractFolder folder, ISettings settings) : base(folder, settings, true)
        {
        }

        protected override string GroupByCore(ITask task)
        {
            switch (task.Action)
            {
                case TaskAction.None:
                    return StringResources.ConverterAction_None;
                case TaskAction.Call:
                    return StringResources.ConverterAction_Call;
                case TaskAction.Email:
                    return StringResources.ConverterAction_Email;
                case TaskAction.Sms:
                    return StringResources.ConverterAction_Sms;
                case TaskAction.Visit:
                    return StringResources.ConverterAction_Visit;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override int CompareCore(ITask t1, ITask t2)
        {
            return t1.Action.CompareTo(t2.Action);
        }

        protected override object OrderByCore(ITask task)
        {
            return task.Action;
        }
    }
}