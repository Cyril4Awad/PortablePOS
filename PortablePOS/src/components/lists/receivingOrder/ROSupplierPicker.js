import React, { useState, useEffect } from 'react';
import { View, Text, TextInput, StyleSheet, TouchableOpacity, Modal, FlatList } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import LoadingData from '../../LoadingData';
import ReceivingOrderData from '../../../data/ReceivingOrderData';

const FILTERS = [
    { code: 'ALL', title: 'All' },
    { code: 'LOCAL', title: 'Local' },
    { code: 'FOREIGN', title: 'Foreign' },
];

// The suppliers search of the Receiving Order (the suppliers the user can purchase from) : like the stock search,
// any part of the code, name or country (several words), local / foreign
// props : visible, selectedRecID, onPick(supplier), onClose, data (the data of the screen, ReceivingOrderData by default : Free Receiving offline the device)
const ROSupplierPicker = props => {

    const [suppliers, setSuppliers] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [searchString, setSearchString] = useState('');
    const [filter, setFilter] = useState('ALL');

    const Load = () => {
        setIsLoading(true);
        setError('');
        (props.data ?? ReceivingOrderData).GetSuppliers()
            .then(rows => setSuppliers(rows))
            .catch(err => setError(err.message))
            .finally(() => setIsLoading(false));
    };

    useEffect(() => {
        if (!props.visible) return;
        setSearchString('');
        if (suppliers.length === 0) Load();
    }, [props.visible]);

    const words = searchString.trim().toUpperCase().split(' ').filter(word => word !== '');
    const shown = suppliers.filter(supplier => {
        if (filter === 'LOCAL' && supplier.supplierForeign === 1) return false;
        if (filter === 'FOREIGN' && supplier.supplierForeign !== 1) return false;
        const text = `${supplier.supplierCode} ${supplier.supplierName} ${supplier.supplierCountry}`.toUpperCase();
        return words.every(word => text.includes(word));
    });

    const SupplierRow = ({ item: supplier }) => {
        const selected = supplier.supplierRecID.toString() === (props.selectedRecID ?? '').toString();
        return (
            <TouchableOpacity style={[styles.row, selected ? styles.rowSelected : null]} onPress={() => props.onPick(supplier)}>
                <View style={styles.rowText}>
                    <Text style={styles.nameText} numberOfLines={2}>{supplier.supplierName}</Text>
                    <Text style={styles.detailText} numberOfLines={1}>
                        {supplier.supplierCode}{supplier.supplierCountry ? `  ·  ${supplier.supplierCountry}` : ''}{supplier.supplierCurrency ? `  ·  ${supplier.supplierCurrency}` : ''}
                    </Text>
                </View>
                <View style={[styles.badge, supplier.supplierForeign === 1 ? styles.badgeForeign : null]}>
                    <Text style={styles.badgeText}>{supplier.supplierForeign === 1 ? 'Foreign' : 'Local'}</Text>
                </View>
            </TouchableOpacity>
        );
    };

    return (
        <Modal visible={props.visible} onRequestClose={props.onClose}>
            <View style={styles.container}>

                <ModalTitle title='Suppliers' onCloseButtonPress={props.onClose} />

                <View style={styles.searchContainer}>
                    <TextInput
                        style={styles.searchText}
                        placeholder='Search Suppliers (code, name, country)'
                        maxLength={60}
                        value={searchString}
                        autoCorrect={false}
                        onChangeText={setSearchString}
                    />
                </View>

                <View style={styles.filtersContainer}>
                    {FILTERS.map(option => (
                        <TouchableOpacity key={option.code}
                            style={[styles.filterButton, filter === option.code ? styles.filterButtonOn : null]}
                            onPress={() => setFilter(option.code)}>
                            <Text style={[styles.filterText, filter === option.code ? styles.filterTextOn : null]}>{option.title}</Text>
                        </TouchableOpacity>
                    ))}
                    <Text style={styles.countText}>{shown.length}</Text>
                </View>

                {isLoading ?
                    <View style={styles.list}><LoadingData /></View> :
                    error !== '' ?
                        <TouchableOpacity style={styles.list} onPress={Load}>
                            <Text style={styles.errorText}>{error}</Text>
                            <Text style={styles.emptyText}>Tap to try again</Text>
                        </TouchableOpacity> :
                        <FlatList
                            style={styles.list}
                            data={shown}
                            keyExtractor={supplier => supplier.supplierRecID.toString()}
                            renderItem={SupplierRow}
                            keyboardShouldPersistTaps='handled'
                            refreshing={false}
                            onRefresh={Load}
                            ListEmptyComponent={<Text style={styles.emptyText}>No Supplier Found</Text>}
                        />}

            </View>
        </Modal>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: Colors.bgColor,
    },
    searchContainer: {
        width: '100%',
        height: 56,
        padding: 5,
    },
    searchText: {
        width: '100%',
        height: '100%',
        borderWidth: 1,
        borderColor: 'silver',
        paddingLeft: 15,
        fontSize: FontSize(19),
        backgroundColor: 'white',
        color: 'black',
    },
    filtersContainer: {
        flexDirection: 'row',
        alignItems: 'center',
        paddingHorizontal: 5,
        paddingBottom: 5,
    },
    filterButton: {
        height: 34,
        paddingHorizontal: 14,
        marginRight: 6,
        borderRadius: 15,
        borderWidth: 1,
        borderColor: Colors.buttonBorderColor,
        backgroundColor: 'white',
        alignItems: 'center',
        justifyContent: 'center',
    },
    filterButtonOn: {
        backgroundColor: Colors.brandBlue,
    },
    filterText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    filterTextOn: {
        color: 'white',
    },
    countText: {
        flex: 1,
        textAlign: 'right',
        color: 'gray',
        fontSize: FontSize(14),
        paddingRight: 8,
    },
    list: {
        flex: 1,
    },
    row: {
        flexDirection: 'row',
        alignItems: 'center',
        minHeight: 60,
        backgroundColor: Colors.listItemBackColor,
        marginBottom: 2,
        paddingHorizontal: 12,
        paddingVertical: 5,
    },
    rowSelected: {
        backgroundColor: 'rgb(220, 240, 255)',
    },
    rowText: {
        flex: 1,
    },
    nameText: {
        color: 'black',
        fontSize: FontSize(16),
        fontWeight: 'bold',
    },
    detailText: {
        color: Colors.brandBlue,
        fontSize: FontSize(14),
    },
    badge: {
        marginLeft: 8,
        paddingHorizontal: 8,
        paddingVertical: 3,
        borderRadius: 10,
        backgroundColor: Colors.brandGreen,
    },
    badgeForeign: {
        backgroundColor: Colors.brandBlue,
    },
    badgeText: {
        color: 'white',
        fontSize: FontSize(12),
        fontWeight: 'bold',
    },
    emptyText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(16),
        padding: 20,
    },
    errorText: {
        textAlign: 'center',
        color: 'red',
        fontSize: FontSize(16),
        padding: 20,
    },
});

export default ROSupplierPicker;
