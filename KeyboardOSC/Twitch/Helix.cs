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
        try
        {
            Plugin.PluginLogger.LogWarning("Received auth callback, authenticating with code");
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
                Plugin.PluginLogger.LogInfo($"Validation result: {validation.Key} {validation.Value}");
                PluginSettings.SetSetting<string>("TwitchUserId", validation.Value);
            }
            else
            {
                RefreshTokens();
            }

            ThreadingHelper.Instance.StartSyncInvoke(() =>
            {
                const string successMsg = "Twitch authentication successful!";
                Plugin.PluginLogger.LogInfo(successMsg);
                Tools.SendBread("Success", successMsg);
            });
        }
        catch (Exception ex)
        {
            Plugin.PluginLogger.LogError($"Error authenticating, try again?: {ex}");
        }
    }

    public static void RefreshTokens()
    {
        try
        {
            Plugin.PluginLogger.LogWarning("Attempting a twitch token refresh!");
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
        }
    }

    private static KeyValuePair<bool, string> ValidateToken()
    {
        Plugin.PluginLogger.LogInfo("Attempting to validate twitch token");
        Client.Headers.Set("Authorization", $"Bearer {_tokens.Item1}");
        Client.Headers.Set("Client-Id", PluginSettings.GetSetting<string>("TwitchClientId").Value);
        try
        {
            var validateResp =
                JsonConvert.DeserializeObject<ValidateResponse>(
                    Client.DownloadString("https://id.twitch.tv/oauth2/validate"));
            return new KeyValuePair<bool, string>(true, validateResp.UserId);
        }
        catch (Exception ex)
        {
            if (ex is WebException { Response: HttpWebResponse { StatusCode: HttpStatusCode.Unauthorized } })
            {
                Plugin.PluginLogger.LogError("Twitch token validation failed??");
            }

            Plugin.PluginLogger.LogError(ex.ToString());
        }

        return new KeyValuePair<bool, string>(false, string.Empty);
    }

    public static void SendTwitchMessage(string msg)
    {
        var userId = PluginSettings.GetSetting<string>("TwitchUserId").Value;
        try
        {
            Client.Headers.Set("Authorization", $"Bearer {_tokens.Item1}");
            Client.Headers.Set("Client-Id", PluginSettings.GetSetting<string>("TwitchClientId").Value);
            Client.Headers.Set("Content-Type", "application/json");

            var body = new ChatMsgBody
            {
                BroadcasterId = userId,
                SenderId = userId,
                Message = msg
            };

            var cmb = JsonConvert.SerializeObject(body);
            var res = Client.UploadString("https://api.twitch.tv/helix/chat/messages", cmb);
            
            
            var resBody = JsonConvert.DeserializeObject<ChatMsgResponse>(res).Data[0];
            if (resBody.IsSent) return;
            Plugin.PluginLogger.LogError(
                $"Twitch msg was not sent: {resBody.DropReason.Code} {resBody.DropReason.Message}");
            Console.Beep();
        }
        catch (Exception ex)
        {
            if (ex is WebException { Response: HttpWebResponse { StatusCode: HttpStatusCode.Unauthorized } })
            {
                Plugin.PluginLogger.LogError("Twitch token expired; Refreshing tokens then attempting again");
                Plugin.PluginLogger.LogError($"Error: {ex}");
                RefreshTokens();
            }
            else
            {
                Plugin.PluginLogger.LogError($"Error sending twitch message: {ex}");
            }
        }
    }
}