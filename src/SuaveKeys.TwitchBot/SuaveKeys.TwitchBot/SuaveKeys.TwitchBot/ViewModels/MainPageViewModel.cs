using ServiceResult;
using SuaveKeys.TwitchBot.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace SuaveKeys.TwitchBot.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly ITwitchService _twitchService;
        private readonly ISuaveKeysService _suaveKeysService;
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand SuaveKeysSignInCommand { get; set; }
        public ICommand TwitchSignInCommand { get; set; }
        public ICommand ToggleStartCommand { get; set; }
        public bool IsListening { get; set; }
        public bool IsLinkedToSuaveKeys { get; set; }
        public bool IsLinkedToTwitch { get; set; }
        public string Prefix { get; set; }
        public string Channel { get; set; }
        public string CommandLog { get; set; }

        public MainPageViewModel()
        {
            _twitchService = App.Current?.Container?.Resolve<ITwitchService>();
            _suaveKeysService = App.Current?.Container?.Resolve<ISuaveKeysService>();
            CommandLog = "";
            SuaveKeysSignInCommand = new Command(async () =>
            {
                var signInResult = await _suaveKeysService.StartSignInAsync();
                IsLinkedToSuaveKeys = signInResult?.ResultType == ResultType.Ok;              
            });
            TwitchSignInCommand = new Command(async () =>
            {
                var signInResult = await _twitchService.StartSignInAsync();
                IsLinkedToTwitch= signInResult?.ResultType == ResultType.Ok;
            });
            ToggleStartCommand = new Command(async () =>
            {
                var startResult = await _twitchService.ToggleListeningAsync(Channel);
                if(startResult?.ResultType == ResultType.Ok)
                {
                    IsListening = !IsListening;
                }
            });

            _twitchService.OnCommandReceived += TwitchService_OnCommandReceived;

        }

        private async void TwitchService_OnCommandReceived(object sender, string command)
        {
            // pass the command to the key service and also append to log
            if(command.StartsWith($"!{Prefix}"))
            {
                await _suaveKeysService.SendCommandAsync(command.Replace($"!{Prefix}", string.Empty).Trim());
                CommandLog += $"\n\n{command}";
            }
        }
    }
}
