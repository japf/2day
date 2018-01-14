using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public enum SmartViewMatchType
    {
        [EnumDisplay("SmartView_MatchAll")]
        All, // match AND (match OR match) AND mach
        [EnumDisplay("SmartView_MatchAny")]
        Any  // match OR (match AND match) OR match
    }
}