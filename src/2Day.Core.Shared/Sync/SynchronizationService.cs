namespace Chartreuse.Today.Core.Shared.Sync
{
    /// <summary>
    /// Do NOT change ordering in this enum has values are persisted in a json file
    /// </summary>
    public enum SynchronizationService
    {
        None = 0,
        ToodleDo = 1,
        Exchange = 2,
        OutlookActiveSync = 3,
        Vercors = 4,
        ExchangeEws = 5,
        ActiveSync = 6
    }
}
