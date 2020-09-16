using System;
using System.Collections.Generic;
using System.Text;

namespace SuaveKeys.TwitchBot.Services
{
    public interface ISuaveKeysAuthSettings
    {
        string ClientId { get; }
        string ClientSecret { get;}
    }
}
