using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.Core.Shared.Model.Smart
{
    public enum SmartViewField
    {
        [EnumDisplay("SmartView_FieldTitle")]
        Title,
        [EnumDisplay("SmartView_FilterNote")]
        Note,
        [EnumDisplay("SmartView_FilterTags")]
        Tags,
        [EnumDisplay("SmartView_FilterFolder")]
        Folder,
        [EnumDisplay("SmartView_FilterContext")]
        Context,
        [EnumDisplay("SmartView_FilterPriority")]     
        Priority,
        [EnumDisplay("SmartView_FilterProgress")]
        Progress,
        [EnumDisplay("SmartView_FilterAdded")]        
        Added,
        [EnumDisplay("SmartView_FilterModified")]        
        Modified,
        [EnumDisplay("SmartView_FilterCompleted")]
        Completed,
        [EnumDisplay("SmartView_FilterDue")]
        Due,
        [EnumDisplay("SmartView_FilterStart")]
        Start,
        [EnumDisplay("SmartView_FilterHasAlarm")]
        HasAlarm,
        [EnumDisplay("SmartView_FilterHasRecurrence")]
        HasRecurrence,
        [EnumDisplay("SmartView_FilterHasSubtasks")]
        HasSubtasks,
        [EnumDisplay("SmartView_FilterIsLate")]
        IsLate
    }
}