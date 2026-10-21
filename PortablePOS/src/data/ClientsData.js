import ServerInfo from "../configuration/ServerInfo";
import { getDBConnection, executeNonQuery, executeReader, executeWithinTransaction, TransactionObject } from '../database/Database';
import { getLoginUserCode } from "../variables/LoginInfo";
import DeviceInfo from "react-native-device-info";

const online = true;

const GetClientGroups = (
    clientGroupType,
    { setClientGroupsData },
    { setClientGroupsDataFiltered },
    { setIsLoading },
    { setError }) => {

    if (!online) {
        
        // setIsLoading(true);

        // executeReader(`select * from CPP_ClientTypes cct ;`)
        // .then(result => {
        //     setClientGroupsData(result);
        //     setClientGroupsDataFiltered(result);
        //     setIsLoading(false);
        // })
        // .catch(err => {
        //     setIsLoading(false);
        //     setError(err.message);
        // });

    }
    else 
    {
        ServerInfo.serverURL().then((serverURL) => {

            setIsLoading(true);

            fetch(serverURL + 'GetClientGroups',
                {
                    method: 'POST',
                    headers: {
                        Accept: 'application/json',
                        'Content-Type': 'application/json',
                        token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                    },
                    body: JSON.stringify({
                        clientGroupType: clientGroupType,
                        userCode: getLoginUserCode()
                    })
                },
            )
                .then(response => response.json())
                .then(results => {

                    var strResult = results.GetClientGroupsResult;

                    if (strResult == '') {
                        setClientGroupsData([]);
                        setClientGroupsDataFiltered([]);
                    } else {
                        setClientGroupsData(JSON.parse(strResult));
                        setClientGroupsDataFiltered(JSON.parse(strResult));
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
    }

};

const GetClients = (
    clientType,
    clientCountryRegion,
    searchString,
    latitude,
    longitude,
    { setClientsData },
    { setClientsDataFiltered },
    { setIsLoading },
    { setError },
    onlyHavingVisits = false,
    visitDate = null) => {

    ServerInfo.serverURL().then((serverURL) => {

        setIsLoading(true);

        fetch(serverURL + 'GetClients',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    clientType: clientType,
                    clientCountryRegion: clientCountryRegion,
                    searchString: searchString,
                    latitude: latitude,
                    longitude: longitude,
                    userCode: getLoginUserCode(),
                    onlyHavingVisits: onlyHavingVisits,
                    visitDate: visitDate
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetClientsResult;

                if (strResult == '') {
                    setClientsData([]);
                    setClientsDataFiltered([]);
                } else {
                    setClientsData(JSON.parse(strResult));
                    setClientsDataFiltered(JSON.parse(strResult));
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

const GetClientLastPurchases = (
    clientRecID,
    { setClientLastPurchasesList }
    ) => {

        ServerInfo.serverURL().then((serverURL) => {

            //setIsLoading(true);

            fetch(serverURL + 'GetClientLastPurchases',
                {
                    method: 'POST',
                    headers: {
                        Accept: 'application/json',
                        'Content-Type': 'application/json',
                        token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                    },
                    body: JSON.stringify({
                        clientRecID: clientRecID
                    })
                },
            )
                .then(response => response.json())
                .then(results => {

                    var strResult = results.GetClientLastPurchasesResult;

                    if (strResult == '') {
                        setClientLastPurchasesList([]);
                        console.log('empty')
                    } else {
                        setClientLastPurchasesList(JSON.parse(strResult));
                        console.log(JSON.parse(strResult))
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

const GetClientStatement = (
    clientRecID,
    { setClientStatementList }
    ) => {

        ServerInfo.serverURL().then((serverURL) => {

            //setIsLoading(true);

            fetch(serverURL + 'GetClientStatement',
                {
                    method: 'POST',
                    headers: {
                        Accept: 'application/json',
                        'Content-Type': 'application/json',
                        token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                    },
                    body: JSON.stringify({
                        clientRecID: clientRecID,
                        userCode: getLoginUserCode()
                    })
                },
            )
                .then(response => response.json())
                .then(results => {

                    var strResult = results.GetClientStatementResult;

                    if (strResult == '') {
                        setClientStatementList([]);
                        console.log('empty')
                    } else {
                        setClientStatementList(JSON.parse(strResult));
                        console.log(JSON.parse(strResult))
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

const GetClientGeoLocation = (
    clientRecID,
    latitude,
    longitude,
    { setClientGeoLocation }
    ) => {

        ServerInfo.serverURL().then((serverURL) => {

            //setIsLoading(true);

            fetch(serverURL + 'GetClientGeoLocation',
                {
                    method: 'POST',
                    headers: {
                        Accept: 'application/json',
                        'Content-Type': 'application/json',
                        token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                    },
                    body: JSON.stringify({
                        clientRecID: clientRecID,
                        latitude: latitude,
                        longitude: longitude,
                    })
                },
            )
                .then(response => response.json())
                .then(results => {

                    var strResult = results.GetClientGeoLocationResult;

                    if (strResult == '') {
                        setClientGeoLocation([]);
                    } else {
                        setClientGeoLocation(JSON.parse(strResult));
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

const SaveClientGeoLocation = (
    clientRecID,
    latitude,
    longitude,
    accuracy,
    radiusInMeters
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'SaveClientGeoLocation',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    clientRecID: clientRecID,
                    latitude: latitude,
                    longitude: longitude,
                    accuracy: accuracy,
                    radiusInMeters: radiusInMeters,
                    userCode: getLoginUserCode()
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var result = results.SaveClientGeoLocationResult;

                if (result == 'Success') {
                    alert('Geo Location Saved');
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

const SaveClientVisit = (
    clientRecID
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'SaveClientVisit',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    clientRecID : clientRecID,
                    userCode: getLoginUserCode()
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var result = results.SaveClientVisitResult;

                if (result == 'Success') {
                    alert('Visit Saved');
                }
                else {
                    alert(result);
                    //alert('Cannot Save The Visit, Please Try Again Later');
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

const SavePotentialClient = (
    clientName,
    business,
    contact,
    phoneMobile,
    address,
    remarks,
    latitude,
    longitude,
    accuracy,
    radiusInMeters
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'SavePotentialClient',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    clientName : clientName,
                    business : business,
                    contact: contact,
                    phoneMobile : phoneMobile,
                    address : address,
                    remarks : remarks,
                    latitude : latitude,
                    longitude : longitude,
                    accuracy: accuracy,
                    radiusInMeters : radiusInMeters,
                    userCode: getLoginUserCode()
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var result = results.SavePotentialClientResult;

                if (result == 'Success') {
                    alert('Request Saved');
                }
                else {
                    //alert(result);
                    alert('Cannot Save The Request, Please Try Again Later');
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
    GetClientGroups: GetClientGroups,
    GetClients: GetClients,
    GetClientLastPurchases: GetClientLastPurchases,
    GetClientStatement: GetClientStatement,
    GetClientGeoLocation: GetClientGeoLocation,
    SaveClientVisit: SaveClientVisit,
    SaveClientGeoLocation: SaveClientGeoLocation,
    SavePotentialClient: SavePotentialClient,
}