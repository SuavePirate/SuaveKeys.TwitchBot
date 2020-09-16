using Newtonsoft.Json;
using ServiceResult;
using SuaveKeys.TwitchClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using Xamarin.Essentials;
using TwitchClientObject = TwitchLib.Client.TwitchClient;

namespace SuaveKeys.TwitchBot.Services
{
    public class TwitchService : ITwitchService
    {
        private readonly ITwitchAuthSettings _authSettings;
        private TwitchClientObject _twitchClient;
        private string _currentToken;
        private string _baseUrl = "https://id.twitch.tv";
        private const string TokenInfoKey = "TWITCH_TOKEN_INFO";
        private bool _isListening;
        public event EventHandler<string> OnCommandReceived;

        public TwitchService(ITwitchAuthSettings authSettings)
        {
            _authSettings = authSettings;
        }

        public async Task<Result<string>> GetAccessTokenAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentToken))
                    return new SuccessResult<string>(_currentToken);

                var refreshResult = await RefreshToken();
                if (refreshResult?.ResultType == ResultType.Ok)
                    return new SuccessResult<string>(_currentToken);

                return new InvalidResult<string>(refreshResult.Errors?.FirstOrDefault());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new UnexpectedResult<string>();
            }
        }

        private async Task<Result<bool>> RefreshToken()
        {
            try
            {
                var tokenJson = await SecureStorage.GetAsync(TokenInfoKey);
                if (string.IsNullOrEmpty(tokenJson))
                    return new InvalidResult<bool>("No token info. You are not signed in.");

                var tokenInfo = JsonConvert.DeserializeObject<TokenResponse>(tokenJson);


                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync($"{_baseUrl}/oauth2/token?client_id={_authSettings.ClientId}&client_secret={_authSettings.ClientSecret}&refresh_token={tokenInfo.RefreshToken}&grant_type=refresh_token&redirect_uri=suavekeystwitch://", null);
                    if (response?.IsSuccessStatusCode == true)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        tokenInfo = JsonConvert.DeserializeObject<TokenResponse>(json);

                        // we have our tokens. Gotta do something with it
                        _currentToken = tokenInfo?.AccessToken;
                        await StoreTokenInfo(tokenInfo);

                        return new SuccessResult<bool>(true);
                    }
                }
                return new InvalidResult<bool>("Unable to authenticate.");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new UnexpectedResult<bool>();
            }
        }


        public async Task<Result<bool>> StartSignInAsync()
        {
            try
            {
                var existingToken = await GetAccessTokenAsync();
                if (existingToken?.ResultType == ResultType.Ok && existingToken.Data != null)
                    return new SuccessResult<bool>(true);

                var state = Guid.NewGuid().ToString();
                var clientId = _authSettings.ClientId;
                var url = $"{_baseUrl}/oauth2/authorize?client_id={clientId}&response_type=code&scope=chat:read+chat:edit+channel:moderate+whispers:read+whispers:edit+channel_editor&redirect_uri=suavekeystwitch://";
                var authResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri(url),
                    new Uri("suavekeystwitch://"));

                var code = authResult.Properties["code"];
               
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync($"{_baseUrl}/oauth2/token?client_id={clientId}&client_secret={_authSettings.ClientSecret}&code={code}&grant_type=authorization_code&redirect_uri=suavekeystwitch://", null);
                    if (response?.IsSuccessStatusCode == true)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var tokenInfo = JsonConvert.DeserializeObject<TokenResponse>(json);

                        // we have our tokens. Gotta do something with it
                        _currentToken = tokenInfo?.AccessToken;
                        await StoreTokenInfo(tokenInfo);

                        return new SuccessResult<bool>(true);
                    }
                }


                return new InvalidResult<bool>("Unable to authenticate.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new UnexpectedResult<bool>();
            }
        }

        private async Task StoreTokenInfo(TokenResponse tokenInfo)
        {
            await SecureStorage.SetAsync(TokenInfoKey, JsonConvert.SerializeObject(tokenInfo));
        }

        public async Task<Result<bool>> ToggleListeningAsync(string channel)
        {
            try
            {
                if (!_isListening)
                {
                    // initialize twitch client based on credentials
                    var tokenResult = await GetAccessTokenAsync();
                    if (tokenResult?.ResultType != ResultType.Ok)
                        return new InvalidResult<bool>(tokenResult?.Errors?.FirstOrDefault() ?? "Unable to get access token. Sign in first");

                    var credentials = new ConnectionCredentials(channel, tokenResult?.Data);
                    var clientOptions = new ClientOptions
                    {
                        MessagesAllowedInPeriod = 750,
                        ThrottlingPeriod = TimeSpan.FromSeconds(30)
                    };
                    var customClient = new WebSocketClient(clientOptions);
                    _twitchClient = new TwitchClientObject(customClient);
                    _twitchClient.Initialize(credentials, channel);
                    _twitchClient.OnMessageReceived += TwitchClient_OnMessageReceived;
                    _twitchClient.Connect();
                }
                else if (_twitchClient != null)
                {
                    _twitchClient.Disconnect();
                    _twitchClient.OnMessageReceived -= TwitchClient_OnMessageReceived;
                }

                return new SuccessResult<bool>(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new UnexpectedResult<bool>();
            }
        }

        private void TwitchClient_OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            if (e?.ChatMessage?.Message?.StartsWith("!") == true)
                OnCommandReceived?.Invoke(this, e.ChatMessage.Message);
        }
    }
}
