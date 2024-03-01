using System.Collections.Generic;
using Newtonsoft.Json;

namespace KeyboardOSC.Twitch;

public class ChatMsgBody
{
    [JsonProperty(PropertyName = "broadcaster_id")]
    public string BroadcasterId { get; set; }

    [JsonProperty(PropertyName = "sender_id")]
    public string SenderId { get; set; }

    [JsonProperty(PropertyName = "message")]
    public string Message { get; set; }
}

public class ValidateResponse
{
    [JsonProperty(PropertyName = "client_id")]
    public string ClientId { get; set; }

    [JsonProperty(PropertyName = "login")] public string Login { get; set; }

    [JsonProperty(PropertyName = "scopes")]
    public string[] Scopes { get; set; }

    [JsonProperty(PropertyName = "user_id")]
    public string UserId { get; set; }

    [JsonProperty(PropertyName = "expires_in")]
    public int ExpiresIn { get; set; }
}

public class TwitchClientInfo
{
    [JsonProperty(PropertyName = "clientId")]
    public string ClientId { get; set; }

    [JsonProperty(PropertyName = "clientSecret")]
    public string ClientSecret { get; set; }

    [JsonProperty(PropertyName = "redirectUri")]
    public string RedirectUri { get; set; }

    public string Code { get; set; }
}

public class AuthResponse
{
    [JsonProperty(PropertyName = "access_token")]
    public string AccessToken { get; set; }

    [JsonProperty(PropertyName = "refresh_token")]
    public string RefreshToken { get; set; }

    [JsonProperty(PropertyName = "expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty(PropertyName = "scope")] public string[] Scopes { get; set; }

    [JsonProperty(PropertyName = "token_type")]
    public string TokenType { get; set; }
}

public class ChatMsgResponse
{
    [JsonProperty(PropertyName = "data")]
    public List<ChatMsgData> Data { get; set; }
}

public class ChatMsgData
{
    [JsonProperty(PropertyName = "message_id")]
    public string MessageId { get; set; }

    [JsonProperty(PropertyName = "is_sent")]
    public bool IsSent { get; set; }

    [JsonProperty(PropertyName = "drop_reason")]
    public DropReason DropReason { get; set; }
}

public class DropReason
{
    [JsonProperty(PropertyName = "code")] public string Code { get; set; }

    [JsonProperty(PropertyName = "message")]
    public string Message { get; set; }
}