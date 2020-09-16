using ServiceResult;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuaveKeys.TwitchBot.Services
{
    public interface ITwitchService
    {
        Task<Result<bool>> StartSignInAsync();
        Task<Result<string>> GetAccessTokenAsync();
        Task<Result<bool>> ToggleListeningAsync(string channel);

        event EventHandler<string> OnCommandReceived;
    }
}
