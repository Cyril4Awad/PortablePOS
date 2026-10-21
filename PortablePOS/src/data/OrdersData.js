import ServerInfo from "../configuration/ServerInfo";
import { getLoginUserCode } from "../variables/LoginInfo";
import DeviceInfo from "react-native-device-info";

const GetOrderVatType = (
    clientPriceCat,
    { setOrderVatType }
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'GetVatType',
            { 
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync() 
                },
                body: JSON.stringify({
                    clientPriceCat: clientPriceCat
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetVatTypeResult;
                
                if (strResult == '') {
                    setOrderVatType('');
                } else {
                    
                    var vatTypes = JSON.parse(strResult);

                    setOrderVatType(vatTypes[0].vatType);

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

const GetOrders = (
    isReturn,
    { setOrdersData },
    { setIsLoading },
    { setError }) => {

    ServerInfo.serverURL().then((serverURL) => {

        setIsLoading(true);

        fetch(serverURL + 'GetOrders',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    isReturn: isReturn,
                    userCode: getLoginUserCode()
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetOrdersResult;

                if (strResult == '') {
                    setOrdersData([]);
                } else {
                    setOrdersData(JSON.parse(strResult));
                }

                setIsLoading(false);

            })
            .catch(err => {
                setIsLoading(false);
                setError(err);
            });

    }).catch((error) => {
        console.log(error)
    });

};

const GetOrderDetails = (
    orderNumber,
    isReturn,
    { setOrderLines },
    { setIsLoading },
    { setError }) => {

    ServerInfo.serverURL().then((serverURL) => {

        setIsLoading(true);

        fetch(serverURL + 'GetOrderDetails',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    orderNumber: orderNumber,
                    isReturn: isReturn,
                    userCode: getLoginUserCode()
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetOrderDetailsResult;

                if (strResult == '' || strResult.startsWith('Error')) {
                    if (strResult.startsWith('Error')) {
                        setError(strResult);
                    } else {
                        setOrderLines([]);
                    }
                } else {
                    setOrderLines(JSON.parse(strResult));
                }

                setIsLoading(false);

            })
            .catch(err => {
                setIsLoading(false);
                setError(err);
            });

    }).catch((error) => {
        console.log(error);
        setError(error);
        setIsLoading(false);
    });

};

const SaveOrder = (
    isReturn,
    clientRecID,
    orderVatType,
    orderDetails,
    orderHeaderRemark,
    orderFooterRemark,
    orderCurrency,
    orderTotal,
    orderTotalVAT,
    orderDiscrepancy,
    PrintTransaction
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);
        //console.log(orderDetails);

        fetch(serverURL + 'SaveOrder',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync() 
                },
                body: JSON.stringify({
                    isReturn: isReturn,
                    clientRecID: clientRecID,
                    orderVatType: orderVatType,
                    orderDetails: orderDetails,
                    orderHeaderRemark : orderHeaderRemark,
                    orderFooterRemark: orderFooterRemark,
                    orderCurrency: orderCurrency,
                    orderTotal: orderTotal,
                    orderTotalVAT: orderTotalVAT,
                    orderDiscrepancy: orderDiscrepancy,
                    userCode: getLoginUserCode()
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var result = results.SaveOrderResult;

                if (result.startsWith('Success')) {
                    alert('Order Saved - Order Number ' + result.replace('Success|',''));
                    PrintTransaction(result.replace('Success|',''));
                }
                else {
                    //alert(result);
                    alert('Cannot Save The Order, Please Try Again Later');
                }

                //setIsLoading(false);

            })
            .catch(err => {
                //setIsLoading(false);
                //setError(err);
                alert(err);
            });

    }).catch((error) => {
        console.log(error)
        alert(error);
    });

};

export default {
    GetOrderVatType: GetOrderVatType,
    GetOrders: GetOrders,
    GetOrderDetails: GetOrderDetails,
    SaveOrder: SaveOrder, 
}