import * as Api from '../_Shared/api.js';
window.Initialize = Initialize;
window.SendAuthCallback = SendAuthCallback;

window.AuthorizeTwitch = function () {
    let clientId = document.getElementById('clientId').value;
    let clientSecret = document.getElementById('clientSecret').value;
    let redirectUri = window.location.href;
    window.localStorage.setItem('twitch', JSON.stringify({ clientId, clientSecret, redirectUri }));
    window.location = `https://id.twitch.tv/oauth2/authorize?response_type=code&client_id=${clientId}&redirect_uri=${redirectUri}&scope=user%3Aread%3Achat+user%3Awrite%3Achat`;
};

function SendAuthCallback(code) {
    let clientInfo = JSON.parse(window.localStorage.getItem('twitch'));
    window.localStorage.removeItem('twitch');
    clientInfo.code = code;
    let UpdatedSetting = {
        internalName: 'TwitchAuthCode',
        value: JSON.stringify(clientInfo),
        fullUpdate: true,
    }
    try {
        setTimeout(() => {
            console.log("Sending auth callback");
            Api.Send('SetXSOverlaySystemSetting', JSON.stringify(UpdatedSetting), null);
        }, 2000);
    } catch {
        console.log("Failed to send auth callback, retrying in 5 seconds");
        setTimeout(() => {
            console.log("Sending auth callback");
            Api.Send('SetXSOverlaySystemSetting', JSON.stringify(UpdatedSetting), null);
        }, 5000);
    }
}

function Initialize(name) {
    Api.Connect(name);
    SubscribeToApiEvents();
    let params = new URLSearchParams(window.location.search);
    document.getElementById('warn').innerText += ` ${window.location.href.replace(window.location.search, '')}`;
    if (params.get('code')) {
        console.log('Found access token');
        SendAuthCallback(getAccessToken(params.get('code')));
    }
}

function getAccessToken(str) {
    if (str.startsWith('?')) {
        str = str.substring(1);
    }
    return str.split('&')[0].replace('code=', '');
}

// Message Handling Functions
function SubscribeToApiEvents() {
    Api.Client.Socket.addEventListener('open', () => {
        Api.Send(Api.Commands.RequestGetSettings, null, null);
    });

    Api.Client.Socket.addEventListener('message', function message(data) {
        HandleMessages(data);
    });

    Api.Client.Socket.addEventListener('close', () => {
        console.log(`${Api.ApiObject.sender} websocket was disconnected. Attempting reconnect.`);
        setTimeout(function () {
            Api.Connect('twitch');
            SubscribeToApiEvents();
            console.log("Reconnecting...");
        }, 1000);
    });
}


function HandleMessages(msg) {
    var decoded = Api.Parse(msg);
    switch (decoded.Command) {
        case 'UpdateSettings':
            break;
    }
}