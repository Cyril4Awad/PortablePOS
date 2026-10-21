import React, { useState, useEffect } from 'react';
import { View, StyleSheet, Text, TextInput, TouchableOpacity, Modal } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';

// Deliver By and Header Remarks of the whole transfer
// props : visible, deliverBy, remarks, onSave(deliverBy, remarks), onClose
const TransferDetailsModal = props => {

    const [deliverBy, setDeliverBy] = useState('');
    const [remarks, setRemarks] = useState('');

    useEffect(() => {
        if (props.visible) {
            setDeliverBy(props.deliverBy ?? '');
            setRemarks(props.remarks ?? '');
        }
    }, [props.visible]);

    return (
        <Modal visible={props.visible} transparent={true} presentationStyle="overFullScreen" onRequestClose={props.onClose}>
            <View style={styles.modalContainer}>
                <View style={styles.modalCard}>

                    <ModalTitle title='Transfer Details' onCloseButtonPress={props.onClose} />

                    <View style={styles.bodyContainer}>

                        <Text style={styles.titleText}>Deliver By</Text>
                        <TextInput
                            style={styles.text}
                            value={deliverBy}
                            maxLength={50}
                            autoFocus={true}
                            onChangeText={setDeliverBy}
                        />

                        <Text style={styles.titleText}>Header Remarks</Text>
                        <TextInput
                            style={[styles.text, styles.remarksText]}
                            value={remarks}
                            maxLength={250}
                            multiline={true}
                            textAlignVertical='top'
                            onChangeText={setRemarks}
                        />

                    </View>

                    <View style={styles.footer}>
                        <TouchableOpacity style={[styles.button, { backgroundColor: 'gray' }]} onPress={props.onClose}>
                            <Text style={styles.buttonText}>Cancel</Text>
                        </TouchableOpacity>
                        <TouchableOpacity style={styles.button} onPress={() => props.onSave(deliverBy.trim(), remarks.trim())}>
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
    titleText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
        fontWeight: 'bold',
        marginBottom: 3,
    },
    text: {
        height: 44,
        backgroundColor: 'white',
        color: 'black',
        fontSize: FontSize(17),
        borderWidth: 1,
        borderColor: 'silver',
        borderRadius: 4,
        paddingHorizontal: 8,
        marginBottom: 10,
    },
    remarksText: {
        height: 90,
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

export default TransferDetailsModal;
