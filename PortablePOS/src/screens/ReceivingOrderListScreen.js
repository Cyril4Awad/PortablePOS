import React, { useState, useEffect, useRef } from 'react';
import { View, Text, TextInput, StyleSheet, TouchableOpacity, FlatList } from 'react-native';
import Colors from '../variables/Colors';
import { FontSize } from '../variables/ResponsiveFont';
import LoadingData from '../components/LoadingData';
import ReceivingOrderData from '../data/ReceivingOrderData';
import { StartDraft } from '../variables/ReceivingOrderDraft';
import { MODE_TITLES, Plural } from '../variables/ReceivingOrderRules';

// The receiving orders that can be modified or cancelled (normal, not linked to a purchase invoice, not cancelled yet),
// the last 200, searched by RO #, ref or supplier. route.params.mode : MODIFY or CANCEL
const ReceivingOrderListScreen = ({ route, navigation }) => {

    const mode = route.params?.mode ?? 'MODIFY';

    const [orders, setOrders] = useState([]);
    const [searchString, setSearchString] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [message, setMessage] = useState('');
    const searchTimer = useRef(null);

    const Load = (search) => {
        setIsLoading(true);
        setMessage('');
        ReceivingOrderData.GetReceivingOrders(mode !== 'VIEW', search)
            .then(rows => {
                setOrders(rows);
                if (rows.length === 0) setMessage('No Receiving Order Found');
            })
            .catch(err => setMessage(err.message))
            .finally(() => setIsLoading(false));
    };

    useEffect(() => {
        navigation.setOptions({ title: MODE_TITLES[mode] });
        Load('');
        return () => clearTimeout(searchTimer.current);
    }, []);

    const Search = (text) => {
        setSearchString(text);
        clearTimeout(searchTimer.current);
        searchTimer.current = setTimeout(() => Load(text.trim()), 600);
    };

    const Open = (order) => {
        setIsLoading(true);
        ReceivingOrderData.GetReceivingOrder(order.roRecID, mode !== 'VIEW')
            .then(receivingOrder => {
                StartDraft(mode, receivingOrder);
                navigation.replace('ReceivingOrderEdit');
            })
            .catch(err => {
                setIsLoading(false);
                alert(err.message);
            });
    };

    const OrderRow = ({ item: order }) => (
        <TouchableOpacity style={styles.row} onPress={() => Open(order)}>
            <View style={styles.rowText}>
                <Text style={styles.titleText} numberOfLines={1}>
                    RO #{order.roRecID}  ·  Ref {order.roRef}
                    {order.roStatus === 'R' ? <Text style={styles.reverseText}>  ·  Reverse</Text> : <></>}
                    {order.piRecID ? <Text style={styles.invoicedText}>  ·  PI #{order.piRecID}</Text> : <></>}
                </Text>
                <Text style={styles.supplierText} numberOfLines={1}>{order.supplierName}</Text>
                <Text style={styles.detailText} numberOfLines={1}>
                    {order.roDate}  ·  {order.locationCode} {order.locationDescription}{order.poRecID ? `  ·  PO #${order.poRecID}` : ''}
                </Text>
            </View>
            <View style={styles.countContainer}>
                <Text style={styles.countText}>{order.lineCount}</Text>
                <Text style={styles.countLabelText}>{order.lineCount === 1 ? 'Line' : 'Lines'}</Text>
            </View>
        </TouchableOpacity>
    );

    return (
        <View style={styles.container}>

            <View style={styles.searchContainer}>
                <TextInput
                    style={styles.searchText}
                    placeholder='Search RO #, Ref Or Supplier'
                    value={searchString}
                    autoCorrect={false}
                    maxLength={60}
                    onChangeText={Search}
                    onSubmitEditing={() => { clearTimeout(searchTimer.current); Load(searchString.trim()); }}
                />
            </View>

            <View style={[styles.statusContainer, { backgroundColor: mode === 'CANCEL' ? 'rgb(200, 60, 60)' : Colors.brandBlue }]}>
                <Text style={styles.statusText} numberOfLines={1}>
                    {mode === 'CANCEL' ? 'Choose the receiving order to cancel' : mode === 'VIEW' ? 'Choose the receiving order to view' : 'Choose the receiving order to add lines to'}
                </Text>
            </View>

            {isLoading ?
                <View style={styles.list}><LoadingData /></View> :
                <FlatList
                    style={styles.list}
                    data={orders}
                    keyExtractor={order => order.roRecID.toString()}
                    renderItem={OrderRow}
                    refreshing={false}
                    onRefresh={() => Load(searchString.trim())}
                    keyboardShouldPersistTaps='handled'
                    ListEmptyComponent={<Text style={styles.emptyText}>{message}</Text>}
                    ListFooterComponent={orders.length > 0 ? <Text style={styles.footerText}>{Plural(orders.length, 'receiving order')}</Text> : <></>}
                />}

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: Colors.bgColor,
    },
    searchContainer: {
        backgroundColor: Colors.brandGreen,
        height: 54,
        padding: 6,
    },
    searchText: {
        flex: 1,
        backgroundColor: 'white',
        color: 'black',
        fontSize: FontSize(18),
        borderRadius: 4,
        paddingLeft: 10,
        paddingVertical: 0,
    },
    statusContainer: {
        height: 32,
        alignItems: 'center',
        justifyContent: 'center',
        paddingHorizontal: 10,
    },
    statusText: {
        color: 'white',
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    list: {
        flex: 1,
    },
    row: {
        flexDirection: 'row',
        alignItems: 'center',
        minHeight: 70,
        backgroundColor: Colors.listItemBackColor,
        marginBottom: 2,
        paddingHorizontal: 12,
        paddingVertical: 5,
    },
    rowText: {
        flex: 1,
    },
    titleText: {
        color: 'black',
        fontSize: FontSize(16),
        fontWeight: 'bold',
    },
    reverseText: {
        color: 'rgb(200, 60, 60)',
    },
    invoicedText: {
        color: Colors.brandGreen,
    },
    supplierText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
    },
    detailText: {
        color: 'gray',
        fontSize: FontSize(13),
    },
    countContainer: {
        alignItems: 'center',
        minWidth: 50,
        marginLeft: 8,
    },
    countText: {
        color: 'black',
        fontSize: FontSize(19),
        fontWeight: 'bold',
    },
    countLabelText: {
        color: 'gray',
        fontSize: FontSize(12),
    },
    emptyText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(16),
        padding: 20,
    },
    footerText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(13),
        padding: 10,
    },
});

export default ReceivingOrderListScreen;
