
namespace Chartreuse.Today.Core.Shared.Model
{
    public interface ITag : IViewTag
    {
        new string Name { get; set; }
        IView Owner { get; }
    }
}
