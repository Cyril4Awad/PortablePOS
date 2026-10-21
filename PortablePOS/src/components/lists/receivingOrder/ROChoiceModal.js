import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Modal, FlatList } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';

// A list to pick one row of the Receiving Order (the items of a code used by several items, the purchase orders...)
// props : visible, title, rows, keyOf(row), textsOf(row) => { title, detail, note, badge, warning }, onPick(row), onClose, emptyText
// warning : shown in red under the row (the row can still be tapped, the screen tells why it cannot be used)
const ROChoiceModal = props => {

    const Row = ({ item: row }) => {
        const texts = props.textsOf(row);
        return (
            <TouchableOpacity style={styles.row} onPress={() => props.onPick(row)}>
                <View style={styles.rowText}>
                    <Text style={styles.titleText} numberOfLines={1}>{texts.title}</Text>
                    {texts.detail ? <Text style={styles.detailText} numberOfLines={2}>{texts.detail}</Text> : <></>}
                    {texts.note ? <Text style={styles.noteText} numberOfLines={2}>{texts.note}</Text> : <></>}
                    {texts.warning ? <Text style={styles.warningText} numberOfLines={2}>{texts.warning}</Text> : <></>}
                </View>
                {texts.badge ?
                    <View style={styles.badge}>
                        <Text style={styles.badgeText}>{texts.badge}</Text>
                    </View> :
                    <></>}
            </TouchableOpacity>
        );
    };

    return (
        <Modal visible={props.visible} transparent={true} presentationStyle="overFullScreen" onRequestClose={props.onClose}>
            <View style={styles.modalContainer}>
                <View style={styles.modalCard}>
                    <ModalTitle title={props.title} onCloseButtonPress={props.onClose} />
                    <FlatList
                        data={props.rows}
                        keyExtractor={row => props.keyOf(row).toString()}
                        renderItem={Row}
                        ListEmptyComponent={<Text style={styles.emptyText}>{props.emptyText ?? 'Nothing Found'}</Text>}
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
        width: Sizing.windowsWidth * 0.94,
        maxHeight: Sizing.windowsHeight * 0.85,
        backgroundColor: 'white',
        borderRadius: 20,
        overflow: 'hidden',
        elevation: 10,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
    },
    row: {
        flexDirection: 'row',
        alignItems: 'center',
        minHeight: 58,
        paddingHorizontal: 12,
        paddingVertical: 5,
        backgroundColor: Colors.listItemBackColor,
        marginBottom: 2,
    },
    rowText: {
        flex: 1,
    },
    titleText: {
        color: 'black',
        fontSize: FontSize(16),
        fontWeight: 'bold',
    },
    detailText: {
        color: Colors.brandBlue,
        fontSize: FontSize(14),
    },
    noteText: {
        color: 'gray',
        fontSize: FontSize(13),
        fontStyle: 'italic',
    },
    warningText: {
        color: 'red',
        fontSize: FontSize(13),
        fontWeight: 'bold',
    },
    badge: {
        marginLeft: 8,
        paddingHorizontal: 8,
        paddingVertical: 3,
        borderRadius: 10,
        backgroundColor: Colors.brandGreen,
    },
    badgeText: {
        color: 'white',
        fontSize: FontSize(12),
        fontWeight: 'bold',
    },
    emptyText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(16),
        padding: 20,
    },
});

export default ROChoiceModal;
