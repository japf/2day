using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.SpeechRecognition;
using Chartreuse.Today.Core.Shared.Resources;
using Chartreuse.Today.Core.Shared.Speech;
using Chartreuse.Today.Core.Shared.Tools.Dialog;

namespace Chartreuse.Today.App.Manager
{
    public class SpeechService : ISpeechService
    {
        private readonly IMessageBoxService messageBoxService;

        public SpeechService(IMessageBoxService messageBoxService)
        {
            if (messageBoxService == null)
                throw new ArgumentNullException(nameof(messageBoxService));

            this.messageBoxService = messageBoxService;
        }

        public Task<SpeechResult> RecognizeAsync(IEnumerable<string> choices = null)
        {
            return null;
        }

        public async Task<SpeechResult> RecognizeAsync(string listenText, string exampleText)
        {
            try
            {
                bool permissionGained = await AudioCapturePermissions.RequestMicrophonePermission();
                if (!permissionGained)
                {
                    await this.messageBoxService.ShowAsync(
                        StringResources.Message_Warning,
                        StringResources.Speech_ReviewPermission);

                    return new SpeechResult(string.Empty, false);
                }

                SpeechRecognizer recognizer = new SpeechRecognizer(SpeechRecognizer.SystemSpeechLanguage);
                recognizer.UIOptions.IsReadBackEnabled = false;
                recognizer.UIOptions.ShowConfirmation = false;

                var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
                recognizer.Constraints.Add(dictationConstraint);

                SpeechRecognitionCompilationResult compilationResult = await recognizer.CompileConstraintsAsync();

                if (compilationResult.Status == SpeechRecognitionResultStatus.Success)
                {
                    SpeechRecognitionResult result = await recognizer.RecognizeWithUIAsync();
                    return new SpeechResult(result.Text.Trim('.'), result.Status == SpeechRecognitionResultStatus.Success);
                }
                
                return new SpeechResult(string.Empty, false);
            }
            catch (Exception e)
            {
                return new SpeechResult(e.Message, false);
            }
        }
        
        private class AudioCapturePermissions
        {
            // If no recording device is attached, attempting to get access to audio capture devices will throw 
            // a System.Exception object, with this HResult set.
            private static int NoCaptureDevicesHResult = -1072845856;

            /// <summary>
            /// On desktop/tablet systems, users are prompted to give permission to use capture devices on a 
            /// per-app basis. Along with declaring the microphone DeviceCapability in the package manifest,
            /// this method tests the privacy setting for microphone access for this application.
            /// Note that this only checks the Settings->Privacy->Microphone setting, it does not handle
            /// the Cortana/Dictation privacy check, however (Under Settings->Privacy->Speech, Inking and Typing).
            /// 
            /// Developers should ideally perform a check like this every time their app gains focus, in order to 
            /// check if the user has changed the setting while the app was suspended or not in focus.
            /// </summary>
            /// <returns>true if the microphone can be accessed without any permissions problems.</returns>
            public async static Task<bool> RequestMicrophonePermission()
            {
                try
                {
                    // Request access to the microphone only, to limit the number of capabilities we need
                    // to request in the package manifest.
                    MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
                    settings.StreamingCaptureMode = StreamingCaptureMode.Audio;
                    settings.MediaCategory = MediaCategory.Speech;
                    MediaCapture capture = new MediaCapture();

                    await capture.InitializeAsync(settings);
                }
                catch (UnauthorizedAccessException)
                {
                    // The user has turned off access to the microphone. If this occurs, we should show an error, or disable
                    // functionality within the app to ensure that further exceptions aren't generated when 
                    // recognition is attempted.
                    return false;
                }
                catch (Exception exception)
                {
                    // This can be replicated by using remote desktop to a system, but not redirecting the microphone input.
                    // Can also occur if using the virtual machine console tool to access a VM instead of using remote desktop.
                    if (exception.HResult == NoCaptureDevicesHResult)
                    {
                        var messageDialog = new Windows.UI.Popups.MessageDialog(StringResources.Speech_NoAudioDevice);
                        await messageDialog.ShowAsync();
                        return false;
                    }
                    else
                    {
                        throw;
                    }
                }
                return true;
            }
        }
    }
}
