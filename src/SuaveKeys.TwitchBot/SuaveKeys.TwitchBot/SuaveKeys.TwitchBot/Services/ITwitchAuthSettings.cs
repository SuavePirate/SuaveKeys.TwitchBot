using System;
using System.Collections.Generic;
using System.Text;

namespace SuaveKeys.TwitchBot.Services
{
    public interface ITwitchAuthSettings
    {
        string ClientId { get; }
        string ClientSecret { get; }
    }
}
