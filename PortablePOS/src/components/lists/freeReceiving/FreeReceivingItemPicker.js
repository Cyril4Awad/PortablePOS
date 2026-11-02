import React, { useState, useEffect } from 'react';
import { View, StyleSheet, Text, TextInput, FlatList, Modal } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import FreeReceivingItem from './FreeReceivingItem';
import FreeReceivingDatabase from '../../../database/FreeReceivingDatabase';
import { PICK_LIMIT } from '../../../variables/FreeReceivingRules';

// The stock search of the downloaded stock file (the Search button, a scan that finds several items), like the stock search of the Receiving Order
// props : visible, searchText, items, title, onPick(item), onClose
// - items : the items of a scanned code used by several items, the search box narrows them
// - no items : the search box searches the whole stock file (any part of the reference, description, single references or a code)
const FreeReceivingItemPicker = props => {

    const codeItems = props.items ?? null;

    const [searchString, setSearchString] = useState('');
    const [items, setItems] = useState([]);

    useEffect(() => {
        if (props.visible) setSearchString(codeItems ? '' : (props.searchText ?? ''));
    }, [props.visible]);

    useEffect(() => {

        if (!props.visible) return;

        if (codeItems) {
            const text = searchString.trim().toUpperCase();
            setItems(text === '' ? codeItems : codeItems.filter(item =>
                item.itemReferenceUpper.includes(text) || item.itemDescriptionUpper.includes(text)
                || (item.itemSingleRef1Upper ?? '').includes(text) || (item.itemSingleRef2Upper ?? '').includes(text)));
            return;
        }

        let active = true;

        FreeReceivingDatabase.SearchItems(searchString, PICK_LIMIT)
            .then(result => { if (active) setItems(result); })
            .catch(err => console.log(err));

        return () => { active = false; };

    }, [props.visible, searchString, codeItems]);

    const ItemRow = itemData => (
        <FreeReceivingItem item={itemData.item} onPress={() => props.onPick(itemData.item)} />
    );

    const subtitle = codeItems ? `${props.searchText ?? ''}  ·  ${codeItems.length} items` : '';

    return (
        <Modal visible={props.visible} onRequestClose={props.onClose}>
            <View style={styles.container}>

                <ModalTitle title={props.title ?? 'Choose Item'} onCloseButtonPress={props.onClose} />

                {subtitle !== '' ? <Text style={styles.subtitleText} numberOfLines={1}>{subtitle}</Text> : <></>}

                <View style={styles.searchContainer}>
                    <TextInput
                        style={styles.searchText}
                        placeholder={codeItems ? 'Narrow By Reference Or Description' : 'Reference, Description Or Code'}
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
    subtitleText: {
        color: Colors.brandGreen,
        fontSize: FontSize(15),
        fontWeight: 'bold',
        paddingHorizontal: 10,
        paddingTop: 6,
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

export default FreeReceivingItemPicker;
