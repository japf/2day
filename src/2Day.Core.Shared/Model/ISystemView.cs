namespace Chartreuse.Today.Core.Shared.Model
{
    public interface ISystemView : IView
    {
        ViewKind ViewKind { get; }
        bool IsEnabled { get; set; }
        void DetachWorkbook();
    }
}