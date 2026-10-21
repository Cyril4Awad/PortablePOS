import ServerInfo from "../configuration/ServerInfo";
import { getLoginUserCode } from "../variables/LoginInfo";
import DeviceInfo from "react-native-device-info";
import { getCashRegisterCode, setCashRegisterCode } from "../variables/CashierInfo";

const GetLocationPretty = ( 
    {setLocationPretty}
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'GetLocationPretty',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    deviceName: DeviceInfo.getDeviceNameSync(),
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var result = results.GetLocationPrettyResult;

                if (result == '') {
                    setLocationPretty('')
                    alert('Invalid or Inactive Cash Register');
                }
                else {
                    setLocationPretty(result);
                }

                //setIsLoading(false);

            })
            .catch(err => {
                //setIsLoading(false);
                //setError(err);
            });

    }).catch((error) => {
        console.log(error)
    });

};

const GetCashRegister = ( 
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'GetCashRegister',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    deviceName: DeviceInfo.getDeviceNameSync(),
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var result = results.GetCashRegisterResult;

                if (result == '') {
                    setCashRegisterCode('')
                    alert('Invalid or Inactive Cash Register');
                }
                else {
                    setCashRegisterCode(result);
                }

                //setIsLoading(false);

            })
            .catch(err => {
                //setIsLoading(false);
                //setError(err);
            });

    }).catch((error) => {
        console.log(error)
    });

};

const GetCashRegisterPretty = ( 
    {setCashRegisterPretty}
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'GetCashRegisterPretty',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    deviceName: DeviceInfo.getDeviceNameSync(),
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var result = results.GetCashRegisterPrettyResult;

                if (result == '') {
                    setCashRegisterPretty('')
                    alert('Invalid or Inactive Cash Register');
                }
                else {
                    setCashRegisterPretty(result);
                }

                //setIsLoading(false);

            })
            .catch(err => {
                //setIsLoading(false);
                //setError(err);
            });

    }).catch((error) => {
        console.log(error)
    });

};

const GetWorkingDay = ( 
    {setWorkingDay}
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'GetWorkingDay',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    //deviceName: DeviceInfo.getDeviceNameSync(),
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var result = results.GetWorkingDayResult;

                if (result == '') {
                    setWorkingDay('')
                    alert('Cannot Get Working Day');
                }
                else {
                    setWorkingDay(result);
                }

                //setIsLoading(false);

            })
            .catch(err => {
                //setIsLoading(false);
                //setError(err);
            });

    }).catch((error) => {
        console.log(error)
    });

};

const SaveCashCount = (
    amountLC,
    amountCC
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'SaveCashCount',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    userCode: getLoginUserCode(),
                    deviceName: DeviceInfo.getDeviceNameSync(),
                    cashRegister: getCashRegisterCode(),
                    amountLC: amountLC,
                    amountCC: amountCC,
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var result = results.SaveCashCountResult;

                if (result == 'Success') {
                    alert('Cash Count Defined Successfully');
                }
                else {
                    //alert(result);
                    alert(result);
                }

                //setIsLoading(false);

            })
            .catch(err => {
                //setIsLoading(false);
                //setError(err);
            });

    }).catch((error) => {
        console.log(error)
    });

};

export default {
    GetLocationPretty: GetLocationPretty,
    GetCashRegister: GetCashRegister,
    GetCashRegisterPretty: GetCashRegisterPretty,
    GetWorkingDay: GetWorkingDay,
    SaveCashCount: SaveCashCount,
}