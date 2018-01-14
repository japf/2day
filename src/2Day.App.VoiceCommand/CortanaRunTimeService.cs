using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Chartreuse.Today.App.VoiceCommand.Commands.Foreground;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.App.VoiceCommand
{
    public class CortanaRuntimeService : ICortanaRuntimeService
    {
        // version 4: update definition with initial support of German
        private const int DefinitionVersion = 4;

        private readonly IWorkbook workbook;
        private readonly List<CortanaForegroundCommandBase> commands;

        public CortanaRuntimeService(IWorkbook workbook)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            this.workbook = workbook;

            this.commands = new List<CortanaForegroundCommandBase>
            {
                new OpenFolderCortanaForegroundCommand(),
                new OpenViewCortanaForegroundCommand(),
                new OpenSmartViewCortanaForegroundCommand(),
                new OpenContextCortanaForegroundCommand(),
                new OpenTagCortanaForegroundCommand()
            };
        }

        public async Task SetupDefinitionsAsync()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            try
            {
                // if voice commands definition are up-to-date, skip updates
                bool isDefinitionVersionObsolete = !localSettings.Values.ContainsKey(CortanaConstants.VoiceCommandDefinitionVersionKey) || (int) localSettings.Values[CortanaConstants.VoiceCommandDefinitionVersionKey] != DefinitionVersion;
#if DEBUG
                if (true)
#else
                if (isDefinitionVersionObsolete)
#endif
                {
                    localSettings.Values[CortanaConstants.VoiceCommandDefinitionVersionKey] = DefinitionVersion;

                    StorageFile vcdStorageFile = await Package.Current.InstalledLocation.GetFileAsync(CortanaConstants.VoiceCommandFilename);
                    await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdStorageFile);
                }

                var views = new List<string>(this.workbook.Views.Select(f => f.Name));
                var smartviews = new List<string>(this.workbook.SmartViews.Select(f => f.Name));
                var folders = new List<string>(this.workbook.Folders.Select(f => f.Name));
                var contexts = new List<string>(this.workbook.Contexts.Select(f => f.Name));
                var tags = new List<string>(this.workbook.Tags.Select(f => f.Name));

                foreach (var definition in VoiceCommandDefinitionManager.InstalledCommandDefinitions)
                {
                    await definition.Value.SetPhraseListAsync(CortanaConstants.PhraseListView, views);
                    await definition.Value.SetPhraseListAsync(CortanaConstants.PhraseListSmartView, smartviews);
                    await definition.Value.SetPhraseListAsync(CortanaConstants.PhraseListFolder, folders);
                    await definition.Value.SetPhraseListAsync(CortanaConstants.PhraseListContext, contexts);
                    await definition.Value.SetPhraseListAsync(CortanaConstants.PhraseListTag, tags);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Installing Voice Commands Failed: " + ex);
                TrackingManagerHelper.Exception(ex, "Error while installing voice commands definition");                
            }
        }

        public void TryHandleActivation(ICortanaRuntimeActions runtimeActions, IActivatedEventArgs args)
        {
            if (args.Kind != ActivationKind.VoiceCommand || !(args is VoiceCommandActivatedEventArgs))
                return;

            VoiceCommandActivatedEventArgs commandArgs = (VoiceCommandActivatedEventArgs)args;
            SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;

            string voiceCommandName = speechRecognitionResult.RulePath[0];

            foreach (var command in this.commands)
            {
                if (voiceCommandName.Equals(command.CommandName, StringComparison.OrdinalIgnoreCase))
                {
                    command.Execute(speechRecognitionResult, runtimeActions, this.workbook);
                }
            }
        }
    }

}
