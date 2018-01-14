using System;

namespace Chartreuse.Today.Core.Shared.Model.Recurrence
{
    public interface ICustomFrequency : IEquatable<ICustomFrequency>
    {
        /// <summary>
        /// Gets the FrequencyType type associated to the class
        /// </summary>
        FrequencyType FrequencyType { get; }

        /// <summary>
        /// Value representing this instance
        /// </summary>
        string Value { get; set; }
        
        /// <summary>
        /// Title to explain what is this custom FrequencyType
        /// </summary>
        string DefaultTitle { get; }

        /// <summary>
        /// A user friendly string representing the value (e.g. 'Every weekday')
        /// </summary>
        string DisplayValue { get; }

        /// <summary>
        /// Indicates whether the frequency is customizable or not (need parameters to be correctly set)
        /// </summary>
        bool IsCustomizable { get; }

        /// <summary>
        /// Indicates whether the frequency value is a valid one or not
        /// </summary>
        bool IsValidValue { get; }

        /// <summary>
        /// Defines the reference date; can be used to display value in a more precise way (e.g each Tuesday instead of weekly)
        /// </summary>
        /// <param name="date"></param>
        void SetReferenceDate(DateTime? date);

        /// <summary>
        /// Computes the next matching date corresponding to this instance parameters
        /// </summary>
        /// <param name="fromDate">Date to compute ext date from</param>
        /// <returns>The next date</returns>
        DateTime ComputeNextDate(DateTime fromDate);
    }
}
