import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Modal, FlatList } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import { FormatQuantity, LotText } from '../../../variables/TransferRules';

// The lots of the scanned item (expiry / lot, color / grid / size) with their available qty : the picked lot is transferred
// A lot whose available qty is all in the transfer already is shown but cannot be picked
// props : item (null = hidden), lots (with transferQty), onPick(lot), onClose
const TransferLotPicker = props => {

    const LotRow = ({ item: lot }) => {
        const left = lot.lotAvailable - lot.transferQty;
        return (
            <TouchableOpacity style={[styles.lotRow, { opacity: left > 0 ? 1 : 0.45 }]} disabled={left <= 0} onPress={() => props.onPick(lot)}>
                <View style={styles.lotTextContainer}>
                    <Text style={styles.lotText} numberOfLines={1}>{LotText(lot) || 'No Expiry / Lot'}</Text>
                    <Text style={styles.lotQtyText} numberOfLines={1}>
                        Avlb {FormatQuantity(lot.lotAvailable)}
                        {lot.transferQty > 0 ? `   ·   In Transfer ${FormatQuantity(lot.transferQty)}` : ''}
                    </Text>
                </View>
                <Text style={[styles.leftText, { color: left > 0 ? Colors.brandGreen : 'red' }]}>{FormatQuantity(Math.max(left, 0))}</Text>
            </TouchableOpacity>
        );
    };

    return (
        <Modal visible={props.item !== null} transparent={true} presentationStyle="overFullScreen" onRequestClose={props.onClose}>
            <View style={styles.modalContainer}>
                <View style={styles.modalCard}>

                    <ModalTitle title='Choose Lot' onCloseButtonPress={props.onClose} />

                    {props.item ?
                        <View style={styles.itemContainer}>
                            <Text style={styles.itemReferenceText} numberOfLines={1}>{props.item.itemReference}</Text>
                            <Text style={styles.itemDescriptionText} numberOfLines={1}>{props.item.itemDescription}</Text>
                        </View> :
                        <></>}

                    <FlatList
                        data={props.lots}
                        keyExtractor={(lot) => lot.lotRecID}
                        renderItem={LotRow}
                    />

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
        width: Sizing.windowsWidth * 0.92,
        maxHeight: Sizing.windowsHeight * 0.85,
        backgroundColor: 'white',
        borderRadius: 20,
        overflow: 'hidden',
        elevation: 10,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
    },
    itemContainer: {
        paddingHorizontal: 12,
        paddingVertical: 6,
    },
    itemReferenceText: {
        color: 'black',
        fontSize: FontSize(17),
        fontWeight: 'bold',
    },
    itemDescriptionText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
    },
    lotRow: {
        flexDirection: 'row',
        alignItems: 'center',
        minHeight: 56,
        paddingHorizontal: 12,
        paddingVertical: 4,
        backgroundColor: Colors.listItemBackColor,
        marginBottom: 2,
    },
    lotTextContainer: {
        flex: 1,
    },
    lotText: {
        color: Colors.brandBlue,
        fontSize: FontSize(16),
        fontWeight: 'bold',
    },
    lotQtyText: {
        color: 'gray',
        fontSize: FontSize(13),
    },
    leftText: {
        fontSize: FontSize(20),
        fontWeight: 'bold',
        marginLeft: 8,
    },
});

export default TransferLotPicker;
