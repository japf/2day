namespace Chartreuse.Today.Core.Shared.Model
{
    /// <summary>
    /// Kind of system view
    /// </summary>
    /// <remarks>Do NOT change the order of the element in this element has the int value is stored in the database</remarks>
    public enum ViewKind
    {
        Today,
        Week,
        All,
        Completed,
        Starred,
        NoDate,
        Tomorrow,
        Reminder,
        Late,
        StartDate,
        NonCompleted,
        None,
        Tag,
        ToSync,
        Search
    }
}
