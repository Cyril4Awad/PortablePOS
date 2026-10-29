import React from 'react';
import { Button } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import Colors from './../variables/Colors';
import { baseHeaderScreenOptions, portableScannerStackScreenOptions, BackArrowOptions } from '../variables/NavigationOptions';

import {
    StyleSheet,
} from 'react-native';

import HomeScreen from './../screens/HomeScreen';
import ClientsScreen from './../screens/ClientsScreen';
import PotentialClientsScreen from './../screens/PotentialClientsScreen';
import ClientsVisitsScreen from '../screens/ClientsVisitsScreen';
import StockSearchScreen from './../screens/StockSearchScreen';
import MissingItemsScreen from '../screens/MissingItemsScreen';
import OrdersScreen from './../screens/OrdersScreen';
import SalesInvoiceScreen from './../screens/SalesInvoiceScreen';
import SalesOrdersScreen from './../screens/SalesOrdersScreen';
import ClientReceiptScreen from './../screens/ClientReceiptScreen';
import ViewDefineCashScreen from './../screens/ViewDefineCashScreen';
import PhysicalCountScreen from './../screens/PhysicalCountScreen';
import PhysicalCountScanScreen from './../screens/PhysicalCountScanScreen';
import PhysicalCountItemsScreen from './../screens/PhysicalCountItemsScreen';
import PhysicalCountScanTestingScreen from './../screens/PhysicalCountScanTestingScreen';
import PhysicalCountCreateScreen from './../screens/PhysicalCountCreateScreen';
import PhysicalCountCreateItemsScreen from './../screens/PhysicalCountCreateItemsScreen';
import PrintLabelScreen from './../screens/PrintLabelScreen';
import ReprintDocumentsScreen from './../screens/ReprintDocumentsScreen';
import FreeReceivingScreen from './../screens/FreeReceivingScreen';
import FreeReceivingScanScreen from './../screens/FreeReceivingScanScreen';
import FreeReceivingItemsScreen from './../screens/FreeReceivingItemsScreen';
import FreeReceivingSettingsScreen from './../screens/FreeReceivingSettingsScreen';
import TransferScanScreen from './../screens/TransferScanScreen';
import TransferSettingsScreen from './../screens/TransferSettingsScreen';
import ReceivingOrderScreen from './../screens/ReceivingOrderScreen';
import ReceivingOrderListScreen from './../screens/ReceivingOrderListScreen';
import ReceivingOrderEditScreen from './../screens/ReceivingOrderEditScreen';
import ReceivingOrderLineScreen from './../screens/ReceivingOrderLineScreen';

const AppNavigator = (params) => {

    const Stack = createNativeStackNavigator();

    return (

        <NavigationContainer>

            <Stack.Navigator
                initialRouteName='Home'
                screenOptions={{
                    ...baseHeaderScreenOptions,
                    ...portableScannerStackScreenOptions,
                }}>

                <Stack.Screen
                    name='Home'
                    options={{
                        headerShown: false,
                    }} >
                    {() => <HomeScreen {...params} />}
                </Stack.Screen>

                <Stack.Screen
                    name='Clients'
                    component={ClientsScreen}
                    options={{
                        title: 'Clients',
                    }} />

                <Stack.Screen
                    name='ClientsVisits'
                    component={ClientsVisitsScreen}
                    options={{
                        title: 'Clients Visits',
                    }} />

                <Stack.Screen
                    name='PotentialClients'
                    component={PotentialClientsScreen}
                    options={{
                        title: 'Potential Clients',
                    }} />

                <Stack.Screen
                    name='StockSearch'
                    component={StockSearchScreen}
                    options={{
                        title: 'Stock Search',
                    }} />

                <Stack.Screen
                    name='MissingItems'
                    component={MissingItemsScreen}
                    options={{
                        title: 'Missing Items',
                    }} />

                <Stack.Screen
                    name='PhysicalCount'
                    component={PhysicalCountScreen}
                    options={({ navigation }) => ({
                        title: 'Physical Count',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='PhysicalCountScan'
                    component={PhysicalCountScanScreen}
                    options={{
                        title: 'Physical Count - Scan Items',
                    }} />

                <Stack.Screen
                    name='PhysicalCountItems'
                    component={PhysicalCountItemsScreen}
                    options={({ navigation }) => ({
                        title: 'Physical Count - Items',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='PhysicalCountScanTesting'
                    component={PhysicalCountScanTestingScreen}
                    options={({ navigation }) => ({
                        title: 'Physical Count - Scan Items',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='PhysicalCountCreate'
                    component={PhysicalCountCreateScreen}
                    options={({ navigation }) => ({
                        title: 'Create Physical Count',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='PhysicalCountCreateItems'
                    component={PhysicalCountCreateItemsScreen}
                    options={({ navigation }) => ({
                        title: 'Create Physical Count - Items',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='PrintLabel'
                    component={PrintLabelScreen}
                    options={({ navigation }) => ({
                        title: 'Print Label',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='ReprintDocuments'
                    component={ReprintDocumentsScreen}
                    options={({ navigation }) => ({
                        title: 'Reprint Documents',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='SalesOrders'
                    component={SalesOrdersScreen}
                    options={{
                        title: 'Prepare Sales Orders (For Expiry)',
                    }} />

                <Stack.Screen
                    name='PortableClientOrders'
                    component={OrdersScreen}
                    initialParams={{ isReturn: false }}
                    options={{
                        title: 'Portable Client Orders',
                    }} />

                <Stack.Screen
                    name='ReturnItemsOrders'
                    component={OrdersScreen}
                    initialParams={{ isReturn: true }}
                    options={{
                        title: 'Return Items Orders',
                    }} />

                <Stack.Screen
                    name='ClientReceipt'
                    component={ClientReceiptScreen}
                    options={{
                        title: 'Mobile Collection',
                    }} />

                <Stack.Screen
                    name='SalesInvoice'
                    component={SalesInvoiceScreen}
                    initialParams={{ isReturn: false }}
                    options={{
                        title: 'Sales Invoice',
                    }} />

                <Stack.Screen
                    name='FreeReceiving'
                    component={FreeReceivingScreen}
                    options={({ navigation }) => ({
                        title: 'Free Receiving',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='FreeReceivingScan'
                    component={FreeReceivingScanScreen}
                    options={({ navigation }) => ({
                        title: 'Free Receiving - Scan Items',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='FreeReceivingItems'
                    component={FreeReceivingItemsScreen}
                    options={({ navigation }) => ({
                        title: 'Free Receiving - Items',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='FreeReceivingSettings'
                    component={FreeReceivingSettingsScreen}
                    options={({ navigation }) => ({
                        title: 'Free Receiving - Settings',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='ReceivingOrder'
                    component={ReceivingOrderScreen}
                    options={({ navigation }) => ({
                        title: 'Receiving Order',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='ReceivingOrderList'
                    component={ReceivingOrderListScreen}
                    options={({ navigation }) => ({
                        title: 'Receiving Orders',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='ReceivingOrderEdit'
                    component={ReceivingOrderEditScreen}
                    options={({ navigation }) => ({
                        title: 'Receiving Order',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='ReceivingOrderLine'
                    component={ReceivingOrderLineScreen}
                    options={({ navigation }) => ({
                        title: 'Receiving Order - Line',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='Transfer'
                    component={TransferScanScreen}
                    options={({ navigation }) => ({
                        title: 'Transfer',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='TransferSettings'
                    component={TransferSettingsScreen}
                    options={({ navigation }) => ({
                        title: 'Transfer - Settings',
                        ...BackArrowOptions(navigation),
                    })} />

                <Stack.Screen
                    name='ViewDefineCash'
                    component={ViewDefineCashScreen}
                    options={{
                        title: 'Define Cash Counted',
                    }} />

            </Stack.Navigator>

        </NavigationContainer>
    );
}

// headerRight: () => (
//     <Button
//         onPress={() => alert('This is a button!')}
//         title="History"
//         color={Colors.brandBlue}
//     />
// ),

const styles = StyleSheet.create({
    container: {
        height: '100%'
    },
    SettingsImage: {
        width: 35,
        height: 35,
        overflow: 'hidden',
        resizeMode: 'contain',
    },
});

export default AppNavigator;
