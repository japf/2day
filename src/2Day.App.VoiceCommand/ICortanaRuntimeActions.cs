using Chartreuse.Today.Core.Shared.Model;

namespace Chartreuse.Today.App.VoiceCommand
{
    public interface ICortanaRuntimeActions
    {
        void SelectFolder(IAbstractFolder folder);
    }
}