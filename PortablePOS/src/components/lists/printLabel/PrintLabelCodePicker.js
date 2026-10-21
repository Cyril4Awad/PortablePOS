import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Modal, FlatList, Image } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';

export const CODE_TABS = [
    { group: 'coding', title: 'Coding' },
    { group: 'barcodes', title: 'Barcodes' },
    { group: 'supplier', title: 'Supplier Ref.' },
];

const SameCode = (code, other) => other !== null && other !== undefined && code.group === other.group && code.title === other.title && code.value === other.value;

// Barcode / QR To Print : the codes of the scanned item in 3 tabs (its references, its barcodes, its supplier references),
// the one picked is printed as the barcode / QR of the label
// props : visible, codes [{ group, title, value }], selected (the chosen code), onSubmit(code), onCancel
// The codes are compared whole : a supplier reference can have the value of a reference of the item
const PrintLabelCodePicker = props => {

    const [tab, setTab] = useState(CODE_TABS[0].group);

    // Opens on the tab of the code chosen
    useEffect(() => {
        if (props.visible) {
            setTab(props.selected?.group ?? CODE_TABS[0].group);
        }
    }, [props.visible]);

    const tabCodes = props.codes.filter(code => code.group === tab);

    const CodeRow = ({ item }) => (
        <TouchableOpacity style={styles.codeRow} onPress={() => props.onSubmit(item)}>
            <View style={styles.codeTextContainer}>
                <Text style={styles.codeTitleText} numberOfLines={1}>{item.title}</Text>
                <Text style={styles.codeValueText} numberOfLines={2}>{item.value}</Text>
            </View>
            <Image
                source={SameCode(item, props.selected) ? require('../../../assets/icons/checked.png') : require('../../../assets/icons/unchecked.png')}
                style={styles.checkBoxImage}
                resizeMode='contain'
            />
        </TouchableOpacity>
    );

    return (
        <Modal visible={props.visible} transparent={true} presentationStyle="overFullScreen" onRequestClose={props.onCancel}>
            <View style={styles.modalContainer}>
                <View style={styles.modalCard}>

                    <ModalTitle title='Barcode / QR To Print' onCloseButtonPress={props.onCancel} />

                    <View style={styles.tabsRow}>
                        {CODE_TABS.map(codeTab => (
                            <TouchableOpacity key={codeTab.group}
                                style={[styles.tab, tab === codeTab.group ? styles.tabSelected : null]}
                                onPress={() => setTab(codeTab.group)}>
                                <Text style={[styles.tabText, tab === codeTab.group ? styles.tabTextSelected : null]} numberOfLines={1}>
                                    {codeTab.title} ({props.codes.filter(code => code.group === codeTab.group).length})
                                </Text>
                            </TouchableOpacity>
                        ))}
                    </View>

                    <FlatList
                        data={tabCodes}
                        keyExtractor={(item, index) => item.group + index}
                        renderItem={CodeRow}
                        ListEmptyComponent={<Text style={styles.emptyText}>None For This Item</Text>}
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
        minHeight: 220,
        maxHeight: Sizing.windowsHeight * 0.85,
        backgroundColor: 'white',
        borderRadius: 20,
        overflow: 'hidden',
        elevation: 10,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
    },
    tabsRow: {
        flexDirection: 'row',
        borderBottomWidth: 2,
        borderBottomColor: Colors.brandBlue,
    },
    tab: {
        flex: 1,
        height: 42,
        alignItems: 'center',
        justifyContent: 'center',
        paddingHorizontal: 2,
        backgroundColor: 'white',
    },
    tabSelected: {
        backgroundColor: Colors.brandBlue,
    },
    tabText: {
        color: Colors.brandBlue,
        fontSize: FontSize(14),
        fontWeight: 'bold',
    },
    tabTextSelected: {
        color: 'white',
    },
    codeRow: {
        flexDirection: 'row',
        alignItems: 'center',
        minHeight: 55,
        paddingHorizontal: 12,
        paddingVertical: 4,
        backgroundColor: Colors.listItemBackColor,
        marginBottom: 2,
    },
    codeTextContainer: {
        flex: 1,
    },
    codeTitleText: {
        color: Colors.brandBlue,
        fontSize: FontSize(14),
        fontWeight: 'bold',
    },
    codeValueText: {
        color: 'black',
        fontSize: FontSize(18),
    },
    checkBoxImage: {
        width: 30,
        height: 30,
    },
    emptyText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(16),
        padding: 20,
    },
});

export default PrintLabelCodePicker;
