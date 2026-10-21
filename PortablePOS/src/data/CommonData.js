import ServerInfo from "../configuration/ServerInfo";
import DeviceInfo from "react-native-device-info";

const GetCurrencies = (
    { setLocalCurrency },
    { setControlCurrency },
    { setOperatingCurrency },
    { setDefaultCurrency },
    { setRateLCCC }
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        fetch(serverURL + 'GetCurrencies',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    //ScannedBarcode: 'N'
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetCurrenciesResult;

                if (strResult == '') {
                    setLocalCurrency('');
                    setControlCurrency('');
                    setOperatingCurrency('');
                    setDefaultCurrency('');
                    setRateLCCC(0);
                } else {

                    var arrCurrencies = JSON.parse(strResult);

                    setLocalCurrency(arrCurrencies[0].localCurrency);
                    setControlCurrency(arrCurrencies[0].controlCurrency);
                    setOperatingCurrency(arrCurrencies[0].operatingCurrency);
                    setDefaultCurrency(arrCurrencies[0].defaultCurrency);
                    setRateLCCC(arrCurrencies[0].rateLCCC);

                }
            })
            .catch(err => {

            });

    }).catch((error) => {
        console.log(error)
    });

};

export default {
    GetCurrencies: GetCurrencies,
}