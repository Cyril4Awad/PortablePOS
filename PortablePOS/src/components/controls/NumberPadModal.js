import React, { useState, useEffect, useRef } from 'react';
import { View, Text, TextInput, StyleSheet, TouchableOpacity, Modal } from 'react-native';
import Colors from '../../variables/Colors';
import Sizing from '../../variables/Sizing';
import { FontSize } from '../../variables/ResponsiveFont';
import ModalTitle from '../ModalTitle';

// Number pad (Set total, Other quantity of the physical count), same card and footer as the app modals
// props : visible, title, preview(value) => text, onSubmit(value), onCancel
// The value can be typed with the keypad of the device (Enter = OK) or with the keys on the screen
const NumberPadModal = props => {

    const [value, setValue] = useState('');
    const inputRef = useRef();

    useEffect(() => {
        if (props.visible) setValue('');
    }, [props.visible]);

    // Digits only, at most 9
    const changeValue = (text) => setValue(text.replace(/[^0-9]/g, '').replace(/^0+(?=[0-9])/, '').slice(0, 9));

    const press = (key) => {
        if (key === 'C') setValue('');
        else if (key === '<') setValue(value.slice(0, -1));
        else if (value.length < 9) setValue(value === '0' ? key : value + key);
        // the keypad of the device keeps typing in the field
        inputRef.current?.focus();
    };

    const submit = () => {
        if (value !== '') props.onSubmit(parseInt(value, 10));
    };

    const keys = ['1', '2', '3', '4', '5', '6', '7', '8', '9', 'C', '0', '<'];

    return (
        <Modal visible={props.visible} transparent={true} presentationStyle="overFullScreen" onRequestClose={props.onCancel}>
            <View style={styles.modalContainer}>
                <View style={styles.modalCard}>

                    <ModalTitle title={props.title} onCloseButtonPress={props.onCancel} />

                    <View style={styles.valueContainer}>
                        <TextInput
                            ref={inputRef}
                            style={styles.valueText}
                            value={value}
                            placeholder='0'
                            placeholderTextColor='black'
                            onChangeText={changeValue}
                            keyboardType='number-pad'
                            showSoftInputOnFocus={false}
                            autoFocus={true}
                            caretHidden={false}
                            maxLength={9}
                            blurOnSubmit={false}
                            onSubmitEditing={submit}
                        />
                        <Text style={styles.previewText}>{value !== '' && props.preview ? props.preview(parseInt(value, 10)) : ' '}</Text>
                    </View>

                    <View style={styles.keysContainer}>
                        {keys.map(key => (
                            <TouchableOpacity key={key} style={styles.key} onPress={() => press(key)}>
                                <Text style={[styles.keyText, { color: key === 'C' || key === '<' ? 'red' : Colors.brandBlue }]}>{key}</Text>
                            </TouchableOpacity>
                        ))}
                    </View>

                    <View style={styles.footer}>
                        <TouchableOpacity style={styles.button} onPress={props.onCancel}>
                            <Text style={styles.buttonText}>Cancel</Text>
                        </TouchableOpacity>
                        <TouchableOpacity style={[styles.button, { opacity: value === '' ? 0.4 : 1 }]} disabled={value === ''}
                            onPress={submit}>
                            <Text style={styles.buttonText}>OK</Text>
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
        backgroundColor: 'white',
        borderRadius: 20,
        overflow: 'hidden',
        elevation: 10,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
    },
    valueContainer: {
        alignItems: 'center',
        paddingTop: 8,
    },
    valueText: {
        width: '60%',
        fontSize: FontSize(32),
        color: 'black',
        fontWeight: 'bold',
        textAlign: 'center',
        padding: 0,
        borderBottomWidth: 2,
        borderBottomColor: Colors.brandBlue,
    },
    previewText: {
        fontSize: FontSize(15),
        color: Colors.brandGreen,
        fontWeight: 'bold',
        marginBottom: 6,
    },
    keysContainer: {
        flexDirection: 'row',
        flexWrap: 'wrap',
        justifyContent: 'space-between',
        paddingHorizontal: 10,
    },
    key: {
        width: '31.5%',
        height: 52,
        marginBottom: 8,
        borderRadius: 15,
        backgroundColor: Colors.buttonBackColor1,
        borderWidth: 1,
        borderColor: Colors.buttonBorderColor,
        alignItems: 'center',
        justifyContent: 'center',
    },
    keyText: {
        fontSize: FontSize(24),
        fontWeight: 'bold',
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

export default NumberPadModal;
