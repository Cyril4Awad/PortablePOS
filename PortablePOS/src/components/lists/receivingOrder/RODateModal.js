import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Modal } from 'react-native';
import { Calendar } from 'react-native-calendars';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import { TodayISO } from '../../../variables/ReceivingOrderRules';

// Picks a date of the Receiving Order header : props visible, title, value (yyyy-MM-dd), onPick(yyyy-MM-dd), onClose
const RODateModal = props => (
    <Modal visible={props.visible} transparent={true} presentationStyle="overFullScreen" onRequestClose={props.onClose}>
        <View style={styles.modalContainer}>
            <View style={styles.modalCard}>
                <ModalTitle title={props.title} onCloseButtonPress={props.onClose} />
                <Calendar
                    current={props.value || TodayISO()}
                    firstDay={1}
                    markedDates={props.value ? { [props.value]: { selected: true, selectedColor: Colors.brandBlue } } : {}}
                    onDayPress={day => props.onPick(day.dateString)}
                    theme={{ todayTextColor: Colors.brandGreen, arrowColor: Colors.brandBlue }}
                />
                <View style={styles.footer}>
                    <TouchableOpacity style={styles.todayButton} onPress={() => props.onPick(TodayISO())}>
                        <Text style={styles.todayText}>Today</Text>
                    </TouchableOpacity>
                </View>
            </View>
        </View>
    </Modal>
);

const styles = StyleSheet.create({
    modalContainer: {
        width: '100%',
        height: '100%',
        backgroundColor: 'rgba(0,0,0,0.5)',
        alignItems: 'center',
        justifyContent: 'center',
    },
    modalCard: {
        width: Sizing.windowsWidth * 0.94,
        backgroundColor: 'white',
        borderRadius: 20,
        overflow: 'hidden',
        elevation: 10,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
    },
    footer: {
        alignItems: 'center',
        padding: 10,
    },
    todayButton: {
        height: 40,
        paddingHorizontal: 30,
        borderRadius: 15,
        borderWidth: 1,
        borderColor: Colors.buttonBorderColor,
        backgroundColor: 'white',
        alignItems: 'center',
        justifyContent: 'center',
    },
    todayText: {
        color: Colors.brandBlue,
        fontSize: FontSize(16),
        fontWeight: 'bold',
    },
});

export default RODateModal;
