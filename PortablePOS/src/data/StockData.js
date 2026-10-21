import ServerInfo from "../configuration/ServerInfo";
import DeviceInfo from "react-native-device-info";
import { getLoginUserCode } from "../variables/LoginInfo";

// options (optional) : { searchFor, supplierRecID }
// searchFor = 'RO' : the conditions of the Receiving Order (items it can receive, the privileges of the user),
// supplierRecID : its supplier (its supplier references are searched too)
const GetItemProductGroups = (
    { setItemProductGroupsData },
    { setItemProductGroupsDataFiltered },
    { setIsLoading },
    { setError },
    options = {}) => {

    ServerInfo.serverURL().then((serverURL) => {

        setIsLoading(true);

        fetch(serverURL + 'GetItemProductGroups',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    searchFor: options.searchFor ?? '',
                    userCode: getLoginUserCode()
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetItemProductGroupsResult;

                if (strResult.startsWith('Error')) {
                    throw new Error(strResult.replace(/^Error /, ''));
                }

                if (strResult == '') {
                    setItemProductGroupsData([]);
                    setItemProductGroupsDataFiltered([]);
                } else {
                    setItemProductGroupsData(JSON.parse(strResult));
                    setItemProductGroupsDataFiltered(JSON.parse(strResult));
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

const GetItems = (
    itemProductGroup,
    itemPromotionCriteria,
    searchString,
    { setItemsData },
    { setItemsDataFiltered },
    { setIsLoading },
    { setError },
    options = {}) => {

    ServerInfo.serverURL().then((serverURL) => {

        setIsLoading(true);

        fetch(serverURL + 'GetItems',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    itemProductGroup: itemProductGroup,
                    itemPromotionCriteria: itemPromotionCriteria,
                    searchString: searchString,
                    searchFor: options.searchFor ?? '',
                    userCode: getLoginUserCode(),
                    supplierRecID: (options.supplierRecID ?? '').toString()
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetItemsResult;

                if (strResult.startsWith('Error')) {
                    throw new Error(strResult.replace(/^Error /, ''));
                }

                if (strResult == '') {
                    setItemsData([]);
                    setItemsDataFiltered([]);
                } else {
                    setItemsData(JSON.parse(strResult));
                    setItemsDataFiltered(JSON.parse(strResult));
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

const GetItemImage = async (
    itemRecID,
    { setItemImage }) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'GetItemImage',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync() 
                },
                body: JSON.stringify({
                    itemRecID: itemRecID
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetItemImageResult;
              
                if (strResult == '') {
                    setItemImage('');
                } else {
                    setItemImage(strResult);
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

const GetItemSpecs = (
    itemRecID,
    { setItemSpecsData },
    { setIsLoading },
    { setError }) => {

    ServerInfo.serverURL().then((serverURL) => {

        setIsLoading(true);

        fetch(serverURL + 'GetItemSpecs',
            {
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    itemRecID: itemRecID
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetItemSpecsResult;

                if (strResult == '') {
                    setItemSpecsData([]);
                } else {
                    setItemSpecsData(JSON.parse(strResult));
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

const GetItemPriceDiscount = (
    itemRecID,
    clientRecID,
    currency, 
    qty ,
    { setSelectedItemPrice },
    { setSelectedItemDiscount }
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'GetItemPriceDiscount',
            { 
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    itemRecID: itemRecID,
                    clientRecID: clientRecID,
                    currency: currency,
                    qty: qty
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetItemPriceDiscountResult;
                
                if (strResult == '') {
                    setSelectedItemPrice(0);
                    setSelectedItemDiscount(0);
                } else {
                    
                    var itemPriceDiscount = JSON.parse(strResult);
                  
                    setSelectedItemPrice(itemPriceDiscount[0].stockItemPrice);
                    setSelectedItemDiscount(itemPriceDiscount[0].stockItemDiscount);

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

const GetItemVatValue = (
    amount,
    itemVatType,
    itemVatPerc,
    clientPriceCat,
    { setSelectedItemVatValue }
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'GetItemVatValue',
            { 
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    amount:amount,
                    itemVatType:itemVatType,
                    itemVatPerc:itemVatPerc,
                    clientPriceCat:clientPriceCat,
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetItemVatValueResult;
                
                if (strResult == '') {
                    setSelectedItemVatValue(0);
                } else {
                    
                    var itemVatValues = JSON.parse(strResult);

                    setSelectedItemVatValue(itemVatValues[0].itemVatValue);

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

const GetPromotions = (
    { setPromotions },
    { setIsLoading },
    { setError }) => {

    ServerInfo.serverURL().then((serverURL) => {

        setIsLoading(true);

        fetch(serverURL + 'GetPromotions',
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

                var strResult = results.GetPromotionsResult;

                if (strResult == '') {
                    setPromotions([]);
                } else {
                    setPromotions(JSON.parse(strResult));
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

const GetPromotionItemPrice = (
    maxRecKey,
    itemRecID,
    itemReference,
    itemDescription,
    clientRecID,
    currency, 
    { setSelectedItemPrice },
    { AddPromotionLine }
    ) => {

    ServerInfo.serverURL().then((serverURL) => {

        //setIsLoading(true);

        fetch(serverURL + 'GetItemPriceDiscount',
            { 
                method: 'POST',
                headers: {
                    Accept: 'application/json',
                    'Content-Type': 'application/json',
                    token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
                },
                body: JSON.stringify({
                    itemRecID: itemRecID,
                    clientRecID: clientRecID,
                    currency: currency
                })
            },
        )
            .then(response => response.json())
            .then(results => {

                var strResult = results.GetItemPriceDiscountResult;
                
                if (strResult == '') {

                    setSelectedItemPrice(0);
                    AddPromotionLine(maxRecKey, itemRecID, itemReference, itemDescription, 0);

                } else {
                    
                    var itemPriceDiscount = JSON.parse(strResult);
                  
                    setSelectedItemPrice(itemPriceDiscount[0].stockItemPrice);
                    AddPromotionLine(maxRecKey, itemRecID, itemReference, itemDescription, itemPriceDiscount[0].stockItemPrice);
                }

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
    GetItemProductGroups: GetItemProductGroups,
    GetItems: GetItems,
    GetItemImage: GetItemImage,
    GetItemSpecs: GetItemSpecs,
    GetItemPriceDiscount: GetItemPriceDiscount,
    GetItemVatValue: GetItemVatValue,
    GetPromotions: GetPromotions,
    GetPromotionItemPrice: GetPromotionItemPrice
}