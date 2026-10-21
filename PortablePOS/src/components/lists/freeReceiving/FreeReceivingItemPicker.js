import React, { useState, useEffect } from 'react';
import { View, StyleSheet, Text, TextInput, FlatList, Modal } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import FreeReceivingItem from './FreeReceivingItem';
import FreeReceivingDatabase from '../../../database/FreeReceivingDatabase';
import { PICK_LIMIT, SEARCH_FIELD_LABELS } from '../../../variables/FreeReceivingRules';

// Items read at most for a scan that matches many items (all the items of a supplier code for example)
const BASE_LIMIT = 5000;

// Opens when the scanned text matches more than one item, or to pick the item a promotion is linked to
// props : visible, field, searchText, exact, title, onPick(item), onClose
// - field of the scan screen : the items of the scanned value, the search box narrows them (reference, description, single refs)
// - field 'any' : the search box searches the whole stock file (reference, description, codes)
const FreeReceivingItemPicker = props => {

    const anyMode = props.field === 'any';

    const [searchString, setSearchString] = useState('');
    const [baseItems, setBaseItems] = useState([]);
    const [items, setItems] = useState([]);

    // The items of the scanned value, read once when the list opens
    useEffect(() => {

        if (!props.visible) return;

        setSearchString(anyMode ? (props.searchText ?? '') : '');
        setBaseItems([]);
        setItems([]);

        if (anyMode) return;

        let active = true;

        FreeReceivingDatabase.FindItems(props.field, props.searchText ?? '', props.exact === true, BASE_LIMIT)
            .then(result => { if (active) setBaseItems(result); })
            .catch(err => console.log(err));

        return () => { active = false; };

    }, [props.visible]);

    useEffect(() => {

        if (!props.visible) return;

        if (!anyMode) {
            const text = searchString.trim().toUpperCase();
            const filtered = text === '' ? baseItems : baseItems.filter(item =>
                item.itemReferenceUpper.includes(text) || item.itemDescriptionUpper.includes(text)
                || (item.itemSingleRef1Upper ?? '').includes(text) || (item.itemSingleRef2Upper ?? '').includes(text));
            setItems(filtered.slice(0, PICK_LIMIT));
            return;
        }

        let active = true;

        FreeReceivingDatabase.FindItems('any', searchString, false, PICK_LIMIT)
            .then(result => { if (active) setItems(result); })
            .catch(err => console.log(err));

        return () => { active = false; };

    }, [props.visible, searchString, baseItems]);

    const ItemRow = itemData => (
        <FreeReceivingItem item={itemData.item} onPress={() => props.onPick(itemData.item)} />
    );

    const subtitle = anyMode ? '' : `${SEARCH_FIELD_LABELS[props.field]} ${props.searchText ?? ''}  ·  ${baseItems.length}${baseItems.length === BASE_LIMIT ? '+' : ''} items`;

    return (
        <Modal visible={props.visible} onRequestClose={props.onClose}>
            <View style={styles.container}>

                <ModalTitle title={props.title ?? 'Choose Item'} onCloseButtonPress={props.onClose} />

                {subtitle !== '' ? <Text style={styles.subtitleText} numberOfLines={1}>{subtitle}</Text> : <></>}

                <View style={styles.searchContainer}>
                    <TextInput
                        style={styles.searchText}
                        placeholder={anyMode ? 'Reference, Description Or Code' : 'Narrow By Reference Or Description'}
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
