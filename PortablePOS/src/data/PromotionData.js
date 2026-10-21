import ServerInfo from "../configuration/ServerInfo";
import { getLoginUserCode } from "../variables/LoginInfo";
import DeviceInfo from "react-native-device-info";

const GetPromotion = (
    itemRecID, 
    itemQty,
    clientRecID, 
    takenPromotions, 
    transactionCurrency,
    {setPromotionRecords},
    {setPromotionVisible}) => {

        console.log('startGettingPromotion')

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'GetPromotion',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    itemRecID : itemRecID , 
                    itemQty : itemQty , 
                    clientRecID : clientRecID , 
                    takenPromotions : takenPromotions , 
                    transactionCurrency : transactionCurrency ,
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                console.log('getting rpomotion result')

                var strResult = results.GetPromotionResult;

                if (strResult == '') {
                    setPromotionRecords([]);
                    setPromotionVisible(false);
                } else {
                    console.log('strResult')
                    console.log(strResult)
                    setPromotionRecords(JSON.parse(strResult));
                    setPromotionVisible(true);
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
    GetPromotion: GetPromotion,
}