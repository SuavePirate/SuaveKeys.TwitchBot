using SuaveKeys.TwitchBot.Services;
using SuaveKeys.TwitchBot.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TinyIoC;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SuaveKeys.TwitchBot.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            var container = new TinyIoCContainer();
            container.Register<ISuaveKeysAuthSettings, SuaveKeysAuthSettings>();
            container.Register<ITwitchAuthSettings, TwitchAuthSettings>();
            LoadApplication(new SuaveKeys.TwitchBot.App(container));
        }
    }
}
