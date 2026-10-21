import ServerInfo from "../configuration/ServerInfo";
import { getLoginUserCode } from "../variables/LoginInfo";
import DeviceInfo from "react-native-device-info";

const GetSalesOrders = (
    { setSalesOrdersData },
    { setSalesOrdersDataFiltered },
    { setIsLoading },
    { setError }) => {

    ServerInfo.serverURL().then((serverURL) => {

        setIsLoading(true);

        fetch(serverURL + 'GetSalesOrders',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    //userCode: userCode
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetSalesOrdersResult;

                if (strResult == '') {
                    setSalesOrdersData([]);
                    setSalesOrdersDataFiltered([]);
                } else {
                    setSalesOrdersData(JSON.parse(strResult));
                    setSalesOrdersDataFiltered(JSON.parse(strResult));
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

const OpenSalesOrder = (
    headerRecID,
    { setModalVisible },
) => {

    DeviceInfo.getDeviceName().then((deviceName) => {

        ServerInfo.serverURL().then((serverURL) => {

            //setIsLoading(true);

            fetch(serverURL + 'OpenSalesOrder',
                {
                    method: 'POST',
                    headers: {
                        Accept: 'application/json',
                        'Content-Type': 'application/json',
                        token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                    },
                    body: JSON.stringify({
                        headerRecID: headerRecID,
                        machine: deviceName,
                        userCode: getLoginUserCode()
                    })
                },
            )
                .then(response => response.json())
                .then(results => {

                    var result = results.OpenSalesOrderResult;

                    if (result == 'Success') {
                        setModalVisible(true);
                    }
                    else {
                        alert(result);
                    }

                })
                .catch(err => {

                });

        }).catch((error) => {
            console.log(error)
        });

    });

};

const CloseSalesOrder = (
    headerRecID,
) => {

    DeviceInfo.getDeviceName().then((deviceName) => {

        ServerInfo.serverURL().then((serverURL) => {

            //setIsLoading(true);

            fetch(serverURL + 'CloseSalesOrder',
                {
                    method: 'POST',
                    headers: {
                        Accept: 'application/json',
                        'Content-Type': 'application/json',
                        token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                    },
                    body: JSON.stringify({
                        headerRecID: headerRecID,
                        machine: deviceName,
                        userCode: getLoginUserCode()
                    })
                },
            )
                .then(response => response.json())
                .then(results => {

                    var result = results.CloseSalesOrderResult;

                    if (result == 'Success') {
                        //setModalVisible(false);
                    }
                    else {
                        //alert(result);
                    }

                })
                .catch(err => {

                });

        }).catch((error) => {
            console.log(error)
        });

    });

};

const GetSalesOrderItems = (
    headerRecID,
    { setSalesOrderItems },
    { setSalesOrderItemsFiltered },
    { setIsLoading },
    { setError },
    showLoading) => {

    ServerInfo.serverURL().then((serverURL) => {

        if (showLoading) {
            setIsLoading(true);
        };

        fetch(serverURL + 'GetSalesOrderItems',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    headerRecID: headerRecID
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetSalesOrderItemsResult;

                if (strResult == '') {
                    setSalesOrderItems([]);
                    setSalesOrderItemsFiltered([]);
                } else {
                    setSalesOrderItems(JSON.parse(strResult));
                    setSalesOrderItemsFiltered(JSON.parse(strResult));
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

const GetSalesOrderDetailedQty = (
    headerRecID,
    counter,
    { setOrdersDetailedQty },
    { setIsLoading },
    { setError }) => {

    ServerInfo.serverURL().then((serverURL) => {

        setIsLoading(true);

        fetch(serverURL + 'GetSalesOrderDetailedQty',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    headerRecID: headerRecID,
                    counter: counter
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetSalesOrderDetailedQtyResult;

                if (strResult == '') {
                    setOrdersDetailedQty([]);
                } else {
                    setOrdersDetailedQty(JSON.parse(strResult));
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

const SaveSalesOrderDetailedQty = (
    headerRecID,
    counter,
    itemRecID,
    expiryLotString,
    { setScanner },
    qty,
    { setQty },
    defaultQty,
    { setOrdersDetailedQty },
    { setIsLoading },
    { setError }
) => {

    DeviceInfo.getDeviceName().then((deviceName) => {

        ServerInfo.serverURL().then((serverURL) => {

            setIsLoading(true);

            fetch(serverURL + 'SaveSalesOrderDetailedQty',
                {
                    method: 'POST',
                    headers: {
                        Accept: 'application/json',
                        'Content-Type': 'application/json',
                        token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync() 
                    },
                    body: JSON.stringify({
                        headerRecID: headerRecID,
                        counter: counter,
                        itemRecID: itemRecID,
                        expiryLotString: expiryLotString,
                        qty: qty,
                        machine: deviceName,
                        userCode: getLoginUserCode()
                    })
                },
            )
                .then(response => response.json())
                .then(results => {

                    var result = results.SaveSalesOrderDetailedQtyResult;

                    if (result == 'Success') {

                        setScanner('');
                        setQty(defaultQty);

                        GetSalesOrderDetailedQty(headerRecID,
                                                 counter,
                                                 { setOrdersDetailedQty },
                                                 { setIsLoading },
                                                 { setError });

                    }
                    else if (result == 'Success1') {

                        alert('There Is An Item With Older Expiry Date');

                        setScanner('');
                        setQty(defaultQty);

                        GetSalesOrderDetailedQty(headerRecID,
                                                 counter,
                                                 { setOrdersDetailedQty },
                                                 { setIsLoading },
                                                 { setError });

                    }
                    else {
                        //alert(result);
                        alert(result);
                    }

                    setIsLoading(false);

                })
                .catch(err => {
                    setIsLoading(false);
                    //setError(err);
                });

        }).catch((error) => {
            console.log(error)
        });

    });

};

const DeleteSalesOrderDetailedQty = (
    recID,
    headerRecID,
    counter,
    { setOrdersDetailedQty },
    { setIsLoading },
    { setError }
) => {

    ServerInfo.serverURL().then((serverURL) => {

        setIsLoading(true);

        fetch(serverURL + 'DeleteSalesOrderDetailedQty',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    recID: recID,
                    headerRecID: headerRecID,
                    counter: counter
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var result = results.DeleteSalesOrderDetailedQtyResult;

                if (result == 'Success') {

                    GetSalesOrderDetailedQty(headerRecID,
                        counter,
                        { setOrdersDetailedQty },
                        { setIsLoading },
                        { setError });

                    alert('Record Deleted');
                }
                else {
                    //alert(result);
                    alert('Cannot Delete Record');
                }

                setIsLoading(false);

            })
            .catch(err => {
                setIsLoading(false);
                //setError(err);
            });

    }).catch((error) => {
        console.log(error)
    });

};

const SendSalesOrderToPOS = (
    headerRecID,
) => {

    DeviceInfo.getDeviceName().then((deviceName) => {

        ServerInfo.serverURL().then((serverURL) => {

            //setIsLoading(true);

            fetch(serverURL + 'SendSalesOrderToPOS',
                {
                    method: 'POST',
                    headers: {
                        Accept: 'application/json',
                        'Content-Type': 'application/json',
                        token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                    },
                    body: JSON.stringify({
                        headerRecID: headerRecID,
                        machine: deviceName,
                        userCode: getLoginUserCode()
                    })
                },
            )
                .then(response => response.json())
                .then(results => {

                    var result = results.SendSalesOrderToPOSResult;

                    if (result == 'Success') {
                        //alert('result');
                    }
                    else {
                        //alert(result);
                    }

                })
                .catch(err => {

                });

        }).catch((error) => {
            console.log(error)
        });

    });

};

export default {
    GetSalesOrders: GetSalesOrders,
    OpenSalesOrder: OpenSalesOrder,
    CloseSalesOrder: CloseSalesOrder,
    GetSalesOrderItems: GetSalesOrderItems,
    GetSalesOrderDetailedQty: GetSalesOrderDetailedQty,
    SaveSalesOrderDetailedQty: SaveSalesOrderDetailedQty,
    DeleteSalesOrderDetailedQty: DeleteSalesOrderDetailedQty,
    SendSalesOrderToPOS: SendSalesOrderToPOS,
}