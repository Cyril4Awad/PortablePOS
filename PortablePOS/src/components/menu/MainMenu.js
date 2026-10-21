import React, { useEffect, useState, useCallback } from 'react';
import MenuItem from './MenuItem';
import MenuHeader from './MenuHeader';
import Colors from '../../variables/Colors';
import { View, Text, ScrollView, StyleSheet, FlatList, Image, Platform, SectionList, LogBox } from 'react-native';
import { NavigationContainer, useFocusEffect } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import Sizing from '../../variables/Sizing';
import { getCashRegisterCode } from '../../variables/CashierInfo';
import LoadingData from '../LoadingData';
import MenuData from '../../data/MenuData';

// menuActive here is only the fallback, the effective value comes from tbl_CPP_ControlizedMenu
export const STEPS = [
    // {
    //     recKey: 'O',
    //     type: 'MH',
    //     title: 'OPERATIONS',
    // },
    // {
    //     recKey: 'O1',
    //     type: 'MI',
    //     stackScreen: 'OpeningClosing',
    //     title: 'Opening / Closing',
    //     description: 'Open / Close working day and cashier session',
    //     imageSource: require('../../assets/icons/opening.png'),
    //     menuActive: false
    // },
    {
        recKey: 'C',
        type: 'MH',
        title: 'CLIENTS',
    },
    {
        recKey: 'C1',
        type: 'MI',
        stackScreen: 'Clients',
        title: 'Clients View',
        description: 'Search & View Clients - Define geographic info',
        imageSource: require('../../assets/icons/clients.png'),
        menuActive: true,
        needCashRegister: false
    },
    {
        recKey: 'C2',
        type: 'MI',
        stackScreen: 'ClientsVisits',
        title: 'Clients Visits',
        description: 'View predefined daily clients visits',
        imageSource: require('../../assets/icons/clientsVisits.png'),
        menuActive: true,
        needCashRegister: false
    },
    {
        recKey: 'C3',
        type: 'MI',
        stackScreen: 'PotentialClients',
        title: 'Potential Clients',
        description: 'Request adding a new Client Account',
        imageSource: require('../../assets/icons/potentialClient.png'),
        menuActive: true,
        needCashRegister: false
    },
    {
        recKey: 'S',
        type: 'MH',
        title: 'STOCK',
    },
    {
        recKey: 'S1',
        type: 'MI',
        stackScreen: 'StockSearch',
        title: 'Stock Search',
        description: 'Search and View Stock Items',
        imageSource: require('../../assets/icons/stock.png'),
        menuActive: true,
        needCashRegister: false
    },
    {
        recKey: 'S2',
        type: 'MI',
        stackScreen: 'MissingItems',
        title: 'Missing Items',
        description: 'Report missing items to stock controller',
        imageSource: require('../../assets/icons/missingItems.png'),
        menuActive: true,
        needCashRegister: false
    },

    // {
    //     recKey: 'S3',
    //     type: 'MI',
    //     stackScreen: 'ItemChecker',
    //     title: 'Item Checker',
    //     description: 'Scan stock item to check the price and some important info',
    //     imageSource: require('../../assets/icons/missingItems.png'),
    //     menuActive: false
    // },
    {
        recKey: 'T',
        type: 'MH',
        title: 'TRANSACTIONS',
    },
    {
        recKey: 'T1',
        type: 'MI',
        stackScreen: 'SalesOrders',
        title: 'Prepare Sales Orders',
        description: 'Update Existing Sales Orders',
        imageSource: require('../../assets/icons/prepareSalesOrder.png'),
        menuActive: true,
        needCashRegister: true
    },
    {
        recKey: 'T2',
        type: 'MI',
        stackScreen: 'PortableClientOrders',
        title: 'Portable Clients Orders',
        description: 'Create or View Orders',
        imageSource: require('../../assets/icons/orders.png'),
        menuActive: true,
        needCashRegister: true
    },
    {
        recKey: 'T3',
        type: 'MI',
        stackScreen: 'ReturnItemsOrders',
        title: 'Return Items Orders',
        description: 'Return goods from client',
        imageSource: require('../../assets/icons/ReturnOrders.png'),
        menuActive: true,
        needCashRegister: true
    },
    {
        recKey: 'T4',
        type: 'MI',
        stackScreen: 'SalesInvoice',
        title: 'Sales Invoice',
        description: 'Issue Invoices and Credit Notes',
        imageSource: require('../../assets/icons/salesInvoice.png'),
        menuActive: true,
        needCashRegister: true
    },
    {
        recKey: 'T5',
        type: 'MI',
        stackScreen: 'ClientReceipt',
        title: 'Mobile Collection',
        description: 'Client Receipt',
        imageSource: require('../../assets/icons/collection.png'),
        menuActive: true,
        needCashRegister: true
    },
    {
        recKey: 'T6',
        type: 'MI',
        stackScreen: 'FreeReceiving',
        title: 'Free Receiving',
        description: 'Receive supplier items offline',
        imageSource: require('../../assets/icons/freeReceiving.png'),
        menuActive: true,
        needCashRegister: false
    },
    {
        recKey: 'T7',
        type: 'MI',
        stackScreen: 'ReceivingOrder',
        title: 'Receiving Order',
        description: 'Receive, modify or cancel supplier receiving orders',
        imageSource: require('../../assets/icons/orders.png'),
        menuActive: true,
        needCashRegister: false
    },
    {
        recKey: 'X',
        type: 'MH',
        title: 'TRANSFER',
    },
    {
        recKey: 'X1',
        type: 'MI',
        stackScreen: 'Transfer',
        title: 'Transfer',
        description: 'Transfer stock to one or more locations',
        imageSource: require('../../assets/icons/transfer.png'),
        menuActive: true,
        needCashRegister: false
    },
    {
        recKey: 'P',
        type: 'MH',
        title: 'PHYSICAL INVENTORY',
    },
    {
        recKey: 'P1',
        type: 'MI',
        stackScreen: 'PhysicalCount',
        title: 'Physical Count',
        description: 'Count stock offline and upload the counted quantities',
        imageSource: require('../../assets/icons/barcode.png'),
        menuActive: true,
        needCashRegister: false
    },
    {
        recKey: 'P2',
        type: 'MI',
        stackScreen: 'PhysicalCountCreate',
        title: 'Create Physical Count',
        description: 'Choose the items and create a count for this machine',
        imageSource: require('../../assets/icons/stock.png'),
        menuActive: true,
        needCashRegister: false
    },
    {
        recKey: 'L',
        type: 'MH',
        title: 'Labels',
    },
    {
        recKey: 'L1',
        type: 'MI',
        stackScreen: 'PrintLabel',
        title: 'Print Label',
        description: 'Scan an item and print its price label',
        imageSource: require('../../assets/icons/reprintDocuments.png'),
        menuActive: true,
        needCashRegister: false
    },
    {
        recKey: 'R',
        type: 'MH',
        title: 'REPORTS',
    },
    {
        recKey: 'R1',
        type: 'MI',
        stackScreen: 'ReprintDocuments',
        title: 'Reprint Documents',
        description: 'Reprint receipts and consumptions',
        imageSource: require('../../assets/icons/reprintDocuments.png'),
        menuActive: true,
        needCashRegister: false
    },
    {
        recKey: 'R2',
        type: 'MI',
        stackScreen: 'ViewDefineCash',
        title: 'Define Cash Counted',
        description: 'Define Counted Cash Money',
        imageSource: require('../../assets/icons/viewDefineCash.png'),
        menuActive: false,
        needCashRegister: true
    },
    {
        recKey: 'R3',
        type: 'MI',
        stackScreen: 'DailyJournal',
        title: 'Daily Journal',
        description: 'Check daily transactions journal',
        imageSource: require('../../assets/icons/dailyJournal.png'),
        menuActive: false,
        needCashRegister: false
    },
];

// Main menu steps that are active (menu items) plus the headers that still have active items
export const getVisibleSteps = menuActiveKeys => {

    const isActive = item => menuActiveKeys[item.recKey] ?? item.menuActive;

    const activeItems = STEPS.filter(item => item.type === 'MI' && isActive(item));

    return STEPS.filter(item => {
        if (item.type === 'MH') {
            return activeItems.some(activeItem => activeItem.recKey.startsWith(item.recKey));
        }
        return activeItems.includes(item);
    });
};

const MainMenu = props => {

    const [menuActiveKeys, setMenuActiveKeys] = useState(null);

    useEffect(() => {
        LogBox.ignoreLogs(['VirtualizedLists should never be nested']);
    }, [])

    // Reload on focus so changes saved from the Menu Items screen are applied when coming back
    useFocusEffect(
        useCallback(() => {
            MenuData.GetControlizedMenu(
                { setMenuActiveKeys },
                { setIsLoading: () => { } },
                {
                    setError: err => {
                        console.log(err);
                        setMenuActiveKeys(currentKeys => currentKeys ?? {});
                    }
                }
            );
        }, [])
    );

    const LcMenuItem = menuItem => {

        if (menuItem.item.type === 'MH') {
            return (
                <MenuHeader title={menuItem.item.title} />
            )
        }
        else {

            return (
                <MenuItem
                    title={menuItem.item.title}
                    description={menuItem.item.description}
                    onPress={() => {
                        if (menuItem.item.needCashRegister && getCashRegisterCode() === '') {
                            alert('Invalid or Inactive Cash Register')
                        }
                        else {
                            props.navigation.navigate(menuItem.item.stackScreen)
                        }
                    }}
                    imageSource={menuItem.item.imageSource}
                    menuActive={true} />
            )

        }

    }

    const TabletMenuSection = props => {

        const sectionHeader = visibleSteps.filter(item => {
            return item.recKey.startsWith(props.filterText) && item.type === 'MH';
        })[0];

        // Every item of the section is inactive
        if (!sectionHeader) {
            return (<></>);
        }

        return (

            <>

                <MenuHeader title={STEPS.filter(item => {
                    return item.recKey.startsWith(props.filterText) && item.type === 'MH';
                })[0].title} />

                <View style={styles.tabletFLContainer}>

                    <FlatList
                        style={styles.container}
                        contentContainerStyle={styles.contentContainerStyle}
                        keyExtractor={(item, index) => item + index}
                        //horizontal={true}
                        data={visibleSteps.filter(item => {
                            return item.recKey.startsWith(props.filterText) && item.type === 'MI';
                        })}
                        renderItem={LcMenuItem}
                        numColumns={2}
                        ListHeaderComponent={<></>}
                        ListFooterComponent={<></>}
                        ListEmptyComponent={<></>}
                        ItemSeparatorComponent={<></>}
                    />

                    {/* {STEPS.filter(item => {
                        return item.recKey.startsWith(props.filterText) && item.type === 'MI';
                    }).length > 2 ?
                        <View style={styles.scrollImageContainer}>
                            <Image
                                source={require('../../assets/icons/Scroll.png')}
                                style={styles.scrollImage}
                                resizeMode="contain"
                            />
                        </View> :
                        <></>} */}

                </View>

            </>

        );
    }

    if (menuActiveKeys === null) {
        return (
            <View style={styles.loadingContainer}>
                <LoadingData />
            </View>
        );
    }

    const visibleSteps = getVisibleSteps(menuActiveKeys);

    if (Sizing.isTablet) {
        return (

            <ScrollView>

                {/* <TabletMenuSection filterText={'O'} /> */}
                <TabletMenuSection filterText={'C'} />
                <TabletMenuSection filterText={'S'} />
                <TabletMenuSection filterText={'T'} />
                <TabletMenuSection filterText={'X'} />
                <TabletMenuSection filterText={'P'} />
                <TabletMenuSection filterText={'L'} />
                <TabletMenuSection filterText={'R'} />

            </ScrollView>

        );
    }
    else {
        return (



            <FlatList
                style={styles.container}
                contentContainerStyle={styles.contentContainerStyle}
                keyExtractor={(item, index) => item.recKey.toString()}
                data={visibleSteps}
                renderItem={LcMenuItem}
                numColumns={1}
            />


        );
    }

};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        //height: 250,//'100%',
        //height: '100%',
    },
    contentContainerStyle: {
        alignItems: Sizing.isTablet ? 'flex-start' : 'center',
        justifyContent: 'space-around',
        paddingLeft: 0,
        paddingRight: 0,
    },
    loadingContainer: {
        flex: 1,
        width: '100%',
    },
    tabletFLContainer: {
        flex: 1,
        width: '100%',
        flexDirection: 'row'
    },
    scrollImageContainer: {
        width: 35,
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: Colors.bgColor,
        opacity: 0.5,
    },
    scrollImage: {
        width: '70%',
        height: '70%',
        overflow: 'hidden',
        resizeMode: 'contain',
    },
});

export default MainMenu;
