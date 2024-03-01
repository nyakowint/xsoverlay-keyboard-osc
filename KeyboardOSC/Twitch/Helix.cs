using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using BepInEx;
using Newtonsoft.Json;

namespace KeyboardOSC.Twitch;

public abstract class Helix
{
    private static readonly WebClient Client = new();

    private static Tuple<string, string> _tokens = new(null, null);

    public static bool CheckAccessToken()
    {
        return !_tokens.Item1.IsNullOrWhiteSpace();
    }

    public static void AuthenticateTwitch(TwitchClientInfo auth)
    {
        var authResp = Client.UploadValues("https://id.twitch.tv/oauth2/token", new NameValueCollection
        {
            { "client_id", auth.ClientId },
            { "client_secret", auth.ClientSecret },
            { "grant_type", "authorization_code" },
            { "code", auth.Code },
            { "redirect_uri", auth.RedirectUri }
        });
        var authTokens = JsonConvert.DeserializeObject<AuthResponse>(Encoding.ASCII.GetString(authResp));
        _tokens = Tuple.Create(authTokens.AccessToken, authTokens.RefreshToken);
        PluginSettings.SetSetting<string>("TwitchClientId", auth.ClientId);
        PluginSettings.SetSetting<string>("TwitchClientSecret", auth.ClientSecret);
        PluginSettings.SetSetting<string>("TwitchRefreshToken", authTokens.RefreshToken);

        var validation = ValidateToken();
        if (validation.Key)
        {
            PluginSettings.SetSetting<string>("TwitchUserId", validation.Value);
        }
        else RefreshTokens();

        ThreadingHelper.Instance.StartSyncInvoke(() =>
        {
            Plugin.PluginLogger.LogInfo("Twitch authentication successful!");
            Tools.SendBread("Success", "Twitch authentication successful!");
        });
    }

    public static void RefreshTokens()
    {
        try
        {
            var authResp = Client.UploadValues("https://id.twitch.tv/oauth2/token", new NameValueCollection
            {
                { "client_id", PluginSettings.GetSetting<string>("TwitchClientId").Value },
                { "client_secret", PluginSettings.GetSetting<string>("TwitchClientSecret").Value },
                { "grant_type", "refresh_token" },
                {
                    "refresh_token", WebUtility.UrlEncode(PluginSettings.GetSetting<string>("TwitchRefreshToken").Value)
                },
            });
            var authTokens = JsonConvert.DeserializeObject<AuthResponse>(Encoding.ASCII.GetString(authResp));
            _tokens = Tuple.Create(authTokens.AccessToken, authTokens.RefreshToken);
            PluginSettings.SetSetting<string>("TwitchRefreshToken", authTokens.RefreshToken);
        }
        catch (Exception e)
        {
            Plugin.PluginLogger.LogError($"Error refreshing twitch tokens, refresh token expired?: {e}");
            Plugin.PluginLogger.LogWarning("Twitch re-authentication may be required!");
        }
    }

    private static KeyValuePair<bool, string> ValidateToken()
    {
        Client.Headers.Add("Authorization", $"Bearer {_tokens.Item1}");
        try
        {
            var validateResp =
                JsonConvert.DeserializeObject<ValidateResponse>(
                    Client.DownloadString("https://id.twitch.tv/oauth2/validate"));
            return new KeyValuePair<bool, string>(true, validateResp.UserId);
        }
        catch (WebException ex) when (ex.Response is HttpWebResponse { StatusCode: HttpStatusCode.Unauthorized })
        {
            Plugin.PluginLogger.LogError("Twitch token validation failed; Refresh tokens");
        }

        return new KeyValuePair<bool, string>(false, string.Empty);
    }

    public static void SendTwitchMessage(string msg)
    {
        try
        {
            Client.Headers.Add("Authorization", $"Bearer {_tokens.Item1}");
            Client.Headers.Add("Client-Id", PluginSettings.GetSetting<string>("TwitchClientId").Value);
            Client.Headers.Add("Content-Type", "application/json");
            var body = new ChatMsgBody
            {
                BroadcasterId = PluginSettings.GetSetting<string>("TwitchUserId").Value,
                SenderId = PluginSettings.GetSetting<string>("TwitchUserId").Value,
                Message = msg
            };
            var res = Client.UploadString("https://api.twitch.tv/helix/chat/messages",
                JsonConvert.SerializeObject(body));
            var resBody = JsonConvert.DeserializeObject<ChatMsgResponse>(res);
            if (!resBody.IsSent)
            {
                Plugin.PluginLogger.LogError(
                    $"Twitch msg was not sent: {resBody.DropReason.Code} {resBody.DropReason.Message}");
            }
        }
        catch (WebException ex)
        {
            if (ex.Response is HttpWebResponse { StatusCode: HttpStatusCode.Unauthorized })
            {
                Plugin.PluginLogger.LogError("Twitch token expired; Refreshing tokens then attempting again");
                RefreshTokens();
                SendTwitchMessage(msg);
            }
            else
            {
                Plugin.PluginLogger.LogError($"Error sending twitch message: {ex}");
            }
        }
    }
}