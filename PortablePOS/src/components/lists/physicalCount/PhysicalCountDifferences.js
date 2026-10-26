import React, { useState, useEffect } from 'react';
import { View, Text, TextInput, StyleSheet, Modal, FlatList } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import PhysicalCountDatabase from '../../../database/PhysicalCountDatabase';
import { FormatQuantity, LineFields } from '../../../variables/PhysicalCountRules';

const SEARCH_DELAY_MS = 300;

// List Differences : the lines whose counted qty is not the available qty, view only (search, nothing else)
// Shown only with the available qty privilege (the available qty is not downloaded otherwise)
// props : visible, countKey, header, onClose
const PhysicalCountDifferences = props => {

    const [searchString, setSearchString] = useState('');
    const [lines, setLines] = useState([]);

    useEffect(() => {

        if (!props.visible) return;

        let active = true;
        const timer = setTimeout(() => {
            PhysicalCountDatabase.GetDifferences(props.countKey, searchString)
                .then(result => { if (active) setLines(result); })
                .catch(error => alert(error.message));
        }, searchString === '' ? 0 : SEARCH_DELAY_MS);

        return () => { active = false; clearTimeout(timer); };

    }, [props.visible, searchString]);

    const Close = () => {
        setSearchString('');
        props.onClose();
    };

    const DifferenceRow = ({ item }) => {
        const fields = LineFields(props.header, item);
        const difference = item.counted - item.systemQty;
        const infoTexts = [];
        if (fields.showFashion) {
            infoTexts.push([item.colorDescription || item.colorCode, item.gridDescription || item.gridCode, item.sizeDescription || item.sizeOrder]
                .filter(text => (text ?? '') !== '').join(' / '));
        }
        if (fields.showExpiry) infoTexts.push(`Exp ${item.expiry || '-'}   Lot ${item.batch || '-'}`);
        return (
            <View style={styles.row}>
                <View style={styles.rowLine}>
                    <Text style={styles.referenceText} numberOfLines={1}>{item.itemReference}</Text>
                    <Text style={[styles.differenceText, { color: difference > 0 ? Colors.brandGreen : 'red' }]}>
                        {difference > 0 ? '+' : ''}{FormatQuantity(difference)}
                    </Text>
                </View>
                <Text style={styles.descriptionText} numberOfLines={1}>{fields.showBook ? item.bookTitle : item.itemDescription}</Text>
                <View style={styles.rowLine}>
                    <Text style={styles.infoText} numberOfLines={1}>{infoTexts.join('   ')}</Text>
                    <Text style={styles.qtyText}>Avlb {FormatQuantity(item.systemQty)}   Counted {FormatQuantity(item.counted)}</Text>
                </View>
            </View>
        );
    };

    return (
        <Modal visible={props.visible} transparent={true} presentationStyle="overFullScreen" onRequestClose={Close}>
            <View style={styles.modalContainer}>
                <View style={styles.modalCard}>

                    <ModalTitle title='List Differences' onCloseButtonPress={Close} />

                    <TextInput
                        style={styles.searchText}
                        placeholder='Search Items'
                        maxLength={30}
                        value={searchString}
                        onChangeText={setSearchString}
                    />

                    <FlatList
                        data={lines}
                        keyExtractor={(item) => item.lineRecID}
                        renderItem={DifferenceRow}
                        keyboardShouldPersistTaps='handled'
                        ListEmptyComponent={<Text style={styles.emptyText}>No Differences</Text>}
                    />

                    {lines.length === 500 ?
                        <Text style={styles.hintText}>Showing the first 500 items, use the search</Text> :
                        <Text style={styles.hintText}>{lines.length} items · Difference = Counted - Avlb</Text>}

                </View>
            </View>
        </Modal>
    );
};

const styles = StyleSheet.create({
    modalContainer: {
        width: '100%',
        height: '100%',
        backgroundColor: 'rgba(0,0,0,0.5)',
        alignItems: 'center',
        justifyContent: 'center',
    },
    modalCard: {
        width: Sizing.windowsWidth * 0.95,
        height: Sizing.windowsHeight * 0.85,
        backgroundColor: 'white',
        borderRadius: 20,
        overflow: 'hidden',
        elevation: 10,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
    },
    searchText: {
        height: 50,
        margin: 5,
        borderWidth: 1,
        borderColor: 'silver',
        paddingLeft: 15,
        fontSize: FontSize(20),
        backgroundColor: 'white',
        color: 'black',
    },
    row: {
        backgroundColor: Colors.listItemBackColor,
        marginBottom: 2,
        paddingHorizontal: 10,
        paddingVertical: 4,
    },
    rowLine: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
    },
    referenceText: {
        color: 'black',
        fontWeight: 'bold',
        fontSize: FontSize(16),
        flex: 1,
    },
    differenceText: {
        fontWeight: 'bold',
        fontSize: FontSize(17),
        marginLeft: 8,
    },
    descriptionText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
    },
    infoText: {
        color: 'gray',
        fontSize: FontSize(13),
        fontStyle: 'italic',
        flex: 1,
    },
    qtyText: {
        color: 'black',
        fontSize: FontSize(14),
    },
    emptyText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(16),
        padding: 20,
    },
    hintText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(13),
        fontStyle: 'italic',
        padding: 6,
    },
});

export default PhysicalCountDifferences;
