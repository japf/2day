namespace Chartreuse.Today.Core.Shared.Model
{
    public interface IContext : IAbstractFolder
    {
        #pragma warning disable 108, 114
        // for x:Binding compatibility
        string Name { get; set; }
        #pragma warning restore 108, 114

        string SyncId { get; set; }
    }
}
