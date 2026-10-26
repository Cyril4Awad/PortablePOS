import ServerInfo from '../configuration/ServerInfo';
import { callService } from '../data/ServiceCall';

// Online = the server of the login screen answers the Ping call (not only the Wi-Fi being connected)
// The menu footer checks it every few seconds, the menu reads the last answer before opening a screen

let lastIsOnline = null;

export const CheckServer = () =>
    ServerInfo.serverIP()
        .then(serverIP => callService(serverIP, 'Ping', {}, 4000))
        .then(() => { lastIsOnline = true; return true; })
        .catch(() => { lastIsOnline = false; return false; });

// null while the first check is not answered yet
export const LastIsOnline = () => lastIsOnline;
