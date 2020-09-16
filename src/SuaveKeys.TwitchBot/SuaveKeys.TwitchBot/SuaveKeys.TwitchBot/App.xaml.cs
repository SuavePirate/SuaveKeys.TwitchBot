using SuaveKeys.TwitchBot.Services;
using System;
using TinyIoC;
using Voicify.Sdk.Assistant.Api;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SuaveKeys.TwitchBot
{
    public partial class App : Application
    {
        public static new App Current;
        public readonly TinyIoCContainer Container;
        public App(TinyIoCContainer container)
        {
            InitializeComponent();
            Current = this;
            container.Register<ISuaveKeysService, SuaveKeysService>();
            container.Register<ITwitchService, TwitchService>();
            container.Register<ICustomAssistantApi>((i,d) => new CustomAssistantApi("https://assistant.voicify.com"));
            Container = container;
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
