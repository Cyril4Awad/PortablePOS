import ServerInfo from "../configuration/ServerInfo";
import { getLoginUserCode } from "../variables/LoginInfo";
import DeviceInfo from "react-native-device-info";

const SaveMissingItem = (
    description,
    brand,
    requestedBy
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'SaveMissingItem',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    description: description,
                    brand: brand,
                    requestedBy: requestedBy,
                    userCode: getLoginUserCode()
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var result = results.SaveMissingItemResult;

                if (result == 'Success') {
                    alert('Report Saved');
                }
                else {
                    //alert(result);
                    alert('Cannot Save The Report, Please Try Again Later');
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
    SaveMissingItem: SaveMissingItem,
}