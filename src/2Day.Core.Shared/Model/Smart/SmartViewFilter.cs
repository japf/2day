using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public enum SmartViewFilter
    {
        [EnumDisplay("SmartView_FilterContains")]
        Contains,           // Matches a string if it contains this substring
        [EnumDisplay("SmartView_FilterBeginsWith")]
        BeginsWith,         // Matches a string if it starts with this substring
        [EnumDisplay("SmartView_FilterEndsWith")]
        EndsWith,           // Matches a string if it ends with this substring
        [EnumDisplay("SmartView_FilterDoesNotContains")]
        DoesNotContains,    // Matches a string if it does not contain this substring

        [EnumDisplay("SmartView_FilterIs")]
        Is,                 // Exact value match
        [EnumDisplay("SmartView_FilterIsNot")]
        IsNot,              // Exact value mismatch

        [EnumDisplay("SmartView_FilterIsMoreThan")]
        IsMoreThan,         // Matches a number if it comes after this value
        [EnumDisplay("SmartView_FilterIsLessThan")]
        IsLessThan,         // Matches a number if it comes before this value

        [EnumDisplay("SmartView_FilterIsYesterday")]
        IsYesterday,             // Matches if date is yesterday
        [EnumDisplay("SmartView_FilterIsToday")]
        IsToday,                // Matches if date is today
        [EnumDisplay("SmartView_FilterIsTomorrow")]
        IsTomorrow,             // Matches if date is tomorrow

        [EnumDisplay("SmartView_FilterIsAfter")]
        IsAfter,            // Matches a date if it comes after this value
        [EnumDisplay("SmartView_FilterIsBefore")]
        IsBefore,           // Matches a date if it comes before this value
        [EnumDisplay("SmartView_FilterWasInTheLast")]
        WasInTheLast,       // Matches a date if it was in the last X days
        [EnumDisplay("SmartView_FilterWasNotInTheLast")]
        WasNotInTheLast,    // Matches a date if it was not in the last X days
        [EnumDisplay("SmartView_FilterIsInTheNext")]
        IsInTheNext,        // Matches a date if it is in the next X days
        [EnumDisplay("SmartView_FilterIsNotInTheNext")]
        IsNotInTheNext,     // Matches a date if it is not in the next X days
        [EnumDisplay("SmartView_FilterIsIn")]
        IsIn,               // Matches a date if it is in exactly X days
        [EnumDisplay("SmartView_FilterIsNotIn")]
        IsNotIn,            // Matches a date if it is not in exactly X days
        [EnumDisplay("SmartView_FilterWas")]
        Was,                // Matches a date if it was exactly X days ago
        [EnumDisplay("SmartView_FilterWasNot")]
        WasNot,             // Matches a date if it was not exactly X days ago

        [EnumDisplay("SmartView_FilterExists")]
        Exists,             // Matches if the field has a non-zero value
        [EnumDisplay("SmartView_FilterDoesNotExist")]
        DoesNotExist,       // Matches if the field is empty or zero

        [EnumDisplay("SmartView_FilterYes")]
        Yes,                // Boolean true (for example: HasAlarm)
        [EnumDisplay("SmartView_FilterNo")]
        No                  // Boolean false
    }
}