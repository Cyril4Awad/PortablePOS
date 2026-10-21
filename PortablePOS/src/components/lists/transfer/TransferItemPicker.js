import React, { useState, useEffect } from 'react';
import { View, StyleSheet, Text, TextInput, FlatList, Modal } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import TransferItem from './TransferItem';
import TransferDatabase from '../../../database/TransferDatabase';
import { PICK_LIMIT } from '../../../variables/TransferRules';

// Opens when the scanned barcode / reference matches more than one item : the search box narrows the list
// (start of the reference or barcode, any part of the description)
// props : visible, searchText, currencyLC, onPick(item), onClose
const TransferItemPicker = props => {

    const [searchString, setSearchString] = useState('');
    const [items, setItems] = useState([]);

    useEffect(() => {
        if (props.visible) setSearchString(props.searchText ?? '');
    }, [props.visible]);

    useEffect(() => {

        if (!props.visible) return;

        let active = true;

        TransferDatabase.SearchItems(searchString, false, PICK_LIMIT)
            .then(result => { if (active) setItems(result); })
            .catch(err => console.log(err));

        return () => { active = false; };

    }, [props.visible, searchString]);

    const ItemRow = itemData => (
        <TransferItem item={itemData.item} currencyLC={props.currencyLC} onPress={() => props.onPick(itemData.item)} />
    );

    return (
        <Modal visible={props.visible} onRequestClose={props.onClose}>
            <View style={styles.container}>

                <ModalTitle title='Choose Item' onCloseButtonPress={props.onClose} />

                <View style={styles.searchContainer}>
                    <TextInput
                        style={styles.searchText}
                        placeholder='Reference, Barcode Or Description'
                        maxLength={50}
                        value={searchString}
                        autoFocus={true}
                        autoCapitalize='characters'
                        autoCorrect={false}
                        onChangeText={setSearchString}
                    />
                </View>

                <FlatList
                    data={items}
                    keyExtractor={(item) => item.itemRecID}
                    renderItem={ItemRow}
                    keyboardShouldPersistTaps='handled'
                    ListEmptyComponent={<Text style={styles.emptyText}>No Items Found</Text>}
                />

                {items.length === PICK_LIMIT ?
                    <Text style={styles.limitText}>Showing the first {PICK_LIMIT} items, type more to narrow the list</Text> :
                    <></>}

            </View>
        </Modal>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        width: '100%',
        backgroundColor: Colors.bgColor,
    },
    searchContainer: {
        width: '100%',
        height: 60,
        padding: 5,
    },
    searchText: {
        width: '100%',
        height: '100%',
        borderWidth: 1,
        borderColor: 'silver',
        paddingLeft: 25,
        fontSize: FontSize(20),
        backgroundColor: 'white',
        color: 'black',
    },
    emptyText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(17),
        padding: 20,
    },
    limitText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(13),
        padding: 5,
    },
});

export default TransferItemPicker;
