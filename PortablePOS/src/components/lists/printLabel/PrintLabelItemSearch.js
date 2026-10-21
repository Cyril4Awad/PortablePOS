import React, { useState, useEffect } from 'react';
import { View, StyleSheet, Text, TextInput, FlatList, TouchableOpacity, Modal, Image } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import LoadingData from '../../LoadingData';
import PrintLabelData from '../../../data/PrintLabelData';

const SEARCH_LIMIT = 50;

// Search of the item to print by its references, barcodes or supplier references (the scan icon of Print Label)
// Opened by a scanned code of more than one item too : its items are shown at once (props.matches)
// When the search finds one item it is picked at once
// props : visible, searchText, matches, onPick(itemRecID), onClose
const PrintLabelItemSearch = props => {

    const [searchText, setSearchText] = useState('');
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState('');

    useEffect(() => {
        if (props.visible) {
            setSearchText(props.searchText ?? '');
            setItems(props.matches ?? []);
            setMessage('');
        }
    }, [props.visible]);

    const Search = () => {

        if (searchText.trim().length < 2) {
            setMessage('Type At Least 2 Characters');
            return;
        }

        setLoading(true);
        setMessage('');

        PrintLabelData.SearchLabelItems(searchText)
            .then(foundItems => {
                if (foundItems.length === 1) {
                    props.onPick(foundItems[0].itemRecID);
                    return;
                }
                setItems(foundItems);
                setMessage(foundItems.length === 0 ? 'No Items Found' : '');
            })
            .catch(error => setMessage(error.message))
            .finally(() => setLoading(false));

    };

    const ItemRow = ({ item }) => (
        <TouchableOpacity style={styles.itemRow} onPress={() => props.onPick(item.itemRecID)}>
            <Text style={styles.itemReferenceText}>{item.itemReference}</Text>
            <Text style={styles.itemDescriptionText} numberOfLines={2}>{item.itemDescription}</Text>
        </TouchableOpacity>
    );

    return (
        <Modal visible={props.visible} onRequestClose={props.onClose}>
            <View style={styles.container}>

                <ModalTitle title='Search Item' onCloseButtonPress={props.onClose} />

                <View style={styles.searchContainer}>
                    <TextInput
                        style={styles.searchText}
                        placeholder='Reference, Barcode Or Description'
                        maxLength={50}
                        value={searchText}
                        autoFocus={true}
                        autoCapitalize='characters'
                        returnKeyType='search'
                        onChangeText={setSearchText}
                        onSubmitEditing={Search}
                    />
                    <TouchableOpacity style={styles.searchButton} onPress={Search}>
                        <Image source={require('../../../assets/icons/stock.png')} style={styles.searchButtonImage} resizeMode='contain' />
                    </TouchableOpacity>
                </View>

                {message !== '' ? <Text style={styles.messageText}>{message}</Text> : <></>}

                <FlatList
                    data={items}
                    keyExtractor={(item) => item.itemRecID}
                    renderItem={ItemRow}
                    keyboardShouldPersistTaps='handled'
                />

                {items.length === SEARCH_LIMIT ?
                    <Text style={styles.limitText}>Showing the first {SEARCH_LIMIT} items, type more of the code</Text> :
                    <></>}

                {loading ? <LoadingData /> : <></>}

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
        flexDirection: 'row',
        width: '100%',
        height: 60,
        padding: 5,
    },
    searchText: {
        flex: 1,
        height: '100%',
        borderWidth: 1,
        borderColor: 'silver',
        paddingLeft: 15,
        fontSize: FontSize(20),
        backgroundColor: 'white',
        color: 'black',
    },
    searchButton: {
        width: 55,
        height: '100%',
        marginLeft: 5,
        borderRadius: 15,
        borderWidth: 1,
        borderColor: Colors.buttonBorderColor,
        backgroundColor: Colors.buttonBackColor1,
        alignItems: 'center',
        justifyContent: 'center',
    },
    searchButtonImage: {
        width: 34,
        height: 34,
    },
    itemRow: {
        minHeight: 55,
        paddingHorizontal: 12,
        paddingVertical: 5,
        backgroundColor: Colors.listItemBackColor,
        marginBottom: 2,
        justifyContent: 'center',
    },
    itemReferenceText: {
        color: Colors.brandBlue,
        fontSize: FontSize(17),
        fontWeight: 'bold',
    },
    itemDescriptionText: {
        color: 'black',
        fontSize: FontSize(15),
    },
    messageText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(16),
        padding: 15,
    },
    limitText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(13),
        padding: 5,
    },
});

export default PrintLabelItemSearch;
