using SuaveKeys.TwitchBot.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SuaveKeys.TwitchBot
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _vm = new MainPageViewModel();
        public MainPage()
        {
            InitializeComponent();
            BindingContext = _vm;
        }
    }
}
