import DeviceInfo from "react-native-device-info";

// Calls a method of the web service of a given server (Physical Count, Free Receiving),
// not the server of the login screen

// The server as typed in the settings : an IP or name (http://), or a full address starting with http:// or https://
// (https://assets.example.com for the devices on GSM)
export const serverBaseURL = (server) => {
    const value = (server ?? '').trim().replace(/\/+$/, '');
    return /^https?:\/\//i.test(value) ? value : 'http://' + value;
};

export const serviceURL = (server) => serverBaseURL(server) + '//PortablePOS_WS/dataservice.svc/Web/';

// Returns the result string of the method, rejects with the error message
// timeoutMs : stops waiting after that time (the request may still finish on the server), 0 = waits
export const callService = (server, method, body, timeoutMs = 0, timeoutMessage = 'The server is taking too long, please try again') => {

    const request = fetch(serviceURL(server) + method,
        {
            method: 'POST',
            headers: {
                Accept: 'application/json',
                'Content-Type': 'application/json',
                token: DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
            },
            body: JSON.stringify(body)
        },
    )
        .then(response => response.json())
        .then(results => {

            var strResult = results[method + 'Result'];

            if (strResult == null) {
                throw new Error('No response from the server ' + server);
            }

            if (strResult.startsWith('Error')) {
                throw new Error(strResult.replace(/^Error /, ''));
            }

            return strResult;

        });

    if (timeoutMs <= 0) return request;

    return Promise.race([
        request,
        new Promise((resolve, reject) => setTimeout(() => reject(new Error(timeoutMessage)), timeoutMs)),
    ]);

};
