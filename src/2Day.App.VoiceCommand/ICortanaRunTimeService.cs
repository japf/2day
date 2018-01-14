using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace Chartreuse.Today.App.VoiceCommand
{
    public interface ICortanaRuntimeService
    {
        Task SetupDefinitionsAsync();
        void TryHandleActivation(ICortanaRuntimeActions runtimeActions, IActivatedEventArgs args);
    }
}