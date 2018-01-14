using System;
using System.Collections.Generic;
using System.Diagnostics;
using Chartreuse.Today.Exchange.Ews.Model;

namespace Chartreuse.Today.Exchange.Ews.Schema
{
    internal class EwsTaskBuilder
    {
        private readonly List<Tuple<int, string>> properties;

        internal EwsTaskBuilder()
        {
            this.properties = new List<Tuple<int, string>>();
        }

        public void LoadExtendedProperty(int propertyId, string value)
        {
            this.properties.Add(new Tuple<int, string>(propertyId, value));
        }

        public EwsTask BuildTask()
        {
            var task = new EwsTask();

            foreach (var property in this.properties)
            {
                int propertyId = property.Item1;
                string value = property.Item2;

                var definition = EwsTaskSchema.GetPropertyDefinition(propertyId);

                switch (definition.FieldUri)
                {
                    case EwsFieldUri.TaskTitle:
                        task.Subject = EwsXmlHelper.ReadStringAs<string>(value);
                        break;
                    case EwsFieldUri.TaskDueDate:
                        task.DueDate = EwsXmlHelper.ReadStringAs<DateTime>(value);
                        break;
                    case EwsFieldUri.TaskStartDate:
                        task.StartDate = EwsXmlHelper.ReadStringAs<DateTime>(value);
                        break;
                    case EwsFieldUri.TaskOrdinalDate:
                        task.OrdinalDate = EwsXmlHelper.ReadStringAs<DateTime>(value);
                        break;
                    case EwsFieldUri.TaskSubOrdinalDate:
                        task.SubOrdinalDate = EwsXmlHelper.ReadStringAs<string>(value);
                        break;
                    case EwsFieldUri.TaskCompletedDate:
                        task.CompleteDate = EwsXmlHelper.ReadStringAs<DateTime>(value);
                        break;
                    case EwsFieldUri.TaskComplete:
                        task.Complete = EwsXmlHelper.ReadStringAs<bool>(value);
                        break;
                    case EwsFieldUri.TaskIsRecurring:
                        task.IsRecurring = EwsXmlHelper.ReadStringAs<bool>(value);
                        break;
                    case EwsFieldUri.TaskIsDeadOccurence:
                        task.IsDeadOccurence = EwsXmlHelper.ReadStringAs<bool>(value);
                        break;
                    case EwsFieldUri.TaskPercentComplete:
                        task.PercentComplete = EwsXmlHelper.ReadStringAs<double>(value);
                        break;
                    case EwsFieldUri.TaskStatus:
                        task.Status = EwsXmlHelper.ReadStringAs<EwsTaskStatus>(value);
                        break;
                    default:
                        Debugger.Break();
                        break;
                }
            }

            return task;
        }
    }
}