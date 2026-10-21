import React, { useState, useEffect } from 'react';
import { View, StyleSheet, Text, TextInput, TouchableOpacity, Modal } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import { FormatQuantity, ParseReceivedQty, RECEIVE_TYPE_LABELS, RECEIVE_TYPE_COLORS, RECEIVE_TYPES } from '../../../variables/FreeReceivingRules';

// Card of a collected line : change its qty or delete it
// props : line (null = hidden), decimals, onSave(qty), onDelete, onClose
const FreeReceivingLineEditor = props => {

    const line = props.line;
    const [qtyText, setQtyText] = useState('');
    const [message, setMessage] = useState('');

    useEffect(() => {
        if (line) {
            setQtyText(FormatQuantity(line.qty));
            setMessage('');
        }
    }, [line]);

    const Save = () => {
        const result = ParseReceivedQty(qtyText, props.decimals);
        if (!result.ok) {
            setMessage(result.message);
            return;
        }
        props.onSave(result.value);
    };

    return (
        <Modal visible={line !== null} transparent={true} presentationStyle="overFullScreen" onRequestClose={props.onClose}>
            <View style={styles.modalContainer}>
                <View style={styles.modalCard}>

                    <ModalTitle title='Collected Item' onCloseButtonPress={props.onClose} />

                    {line ?
                        <View style={styles.bodyContainer}>

                            <View style={styles.titleRow}>
                                <Text style={styles.referenceText} numberOfLines={1}>{line.itemReference}</Text>
                                <View style={[styles.typeBadge, { backgroundColor: RECEIVE_TYPE_COLORS[line.receiveType] }]}>
                                    <Text style={styles.typeBadgeText}>{RECEIVE_TYPE_LABELS[line.receiveType]}</Text>
                                </View>
                            </View>
                            <Text style={styles.descriptionText} numberOfLines={2}>{line.itemDescription}</Text>

                            <Text style={styles.infoText} numberOfLines={1}>{line.supplierCode} - {line.supplierName}</Text>
                            {line.invoiceNumber !== '' ? <Text style={styles.infoText} numberOfLines={1}>Supplier Invoice {line.invoiceNumber}</Text> : <></>}
                            {line.receiveType === RECEIVE_TYPES.PROMOTION && line.linkedItemRecID !== '' ?
                                <Text style={styles.infoText} numberOfLines={1}>Linked to {line.linkedItemReference} - {line.linkedItemDescription}</Text> :
                                <></>}

                            <Text style={styles.qtyTitleText}>Received Qty {line.itemUnit ? `(${line.itemUnit})` : ''}</Text>
                            <TextInput
                                style={styles.qtyText}
                                value={qtyText}
                                onChangeText={text => { setQtyText(text); setMessage(''); }}
                                keyboardType='numeric'
                                selectTextOnFocus={true}
                                autoFocus={true}
                                maxLength={16}
                                onSubmitEditing={Save}
                            />
                            <Text style={styles.messageText}>{message}</Text>

                        </View> :
                        <></>}

                    <View style={styles.footer}>
                        <TouchableOpacity style={[styles.button, { backgroundColor: 'red' }]} onPress={props.onDelete}>
                            <Text style={styles.buttonText}>Delete</Text>
                        </TouchableOpacity>
                        <TouchableOpacity style={styles.button} onPress={Save}>
                            <Text style={styles.buttonText}>Save</Text>
                        </TouchableOpacity>
                    </View>

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
    bodyContainer: {
        padding: 12,
    },
    titleRow: {
        flexDirection: 'row',
        alignItems: 'center',
    },
    referenceText: {
        flex: 1,
        color: 'black',
        fontSize: FontSize(18),
        fontWeight: 'bold',
    },
    typeBadge: {
        borderRadius: 9,
        paddingHorizontal: 8,
        height: 20,
        justifyContent: 'center',
        marginLeft: 8,
    },
    typeBadgeText: {
        color: 'white',
        fontSize: FontSize(13),
        fontWeight: 'bold',
    },
    descriptionText: {
        color: Colors.brandBlue,
        fontSize: FontSize(16),
        marginBottom: 4,
    },
    infoText: {
        color: 'gray',
        fontSize: FontSize(14),
        fontStyle: 'italic',
    },
    qtyTitleText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        fontWeight: 'bold',
        marginTop: 10,
        marginBottom: 3,
    },
    qtyText: {
        height: 45,
        backgroundColor: 'white',
        color: 'black',
        fontSize: FontSize(22),
        borderWidth: 1,
        borderColor: 'silver',
        borderRadius: 4,
        textAlign: 'center',
    },
    messageText: {
        color: 'red',
        fontSize: FontSize(14),
        fontWeight: 'bold',
        textAlign: 'center',
        minHeight: 20,
        marginTop: 4,
    },
    footer: {
        backgroundColor: 'gainsboro',
        height: 60,
        width: '100%',
        alignItems: 'center',
        justifyContent: 'space-evenly',
        flexDirection: 'row',
    },
    button: {
        backgroundColor: Colors.brandBlue,
        width: 110,
        height: 40,
        alignItems: 'center',
        justifyContent: 'center',
        borderRadius: 15,
    },
    buttonText: {
        fontSize: FontSize(15),
        color: 'white',
    },
});

export default FreeReceivingLineEditor;
