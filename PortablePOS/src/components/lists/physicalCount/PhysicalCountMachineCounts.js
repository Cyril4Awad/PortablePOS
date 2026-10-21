import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Modal, FlatList } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import LoadingData from '../../LoadingData';
import PhysicalCountData from '../../../data/PhysicalCountData';

const STATUS_TEXTS = {
    open: { text: 'Open', color: Colors.brandGreen },
    cancelled: { text: 'Cancelled', color: 'red' },
    finalized: { text: 'Finalized', color: 'gray' },
};

// My Counts : the counts of this machine on the location (the last 50), same card as the other modals
// An open count is picked to be downloaded, a cancelled or finalized one is closed (shown, not picked)
// props : visible, server, location, machineName, deviceCountNumber (count on the device), onPick(countNumber), onClose
const PhysicalCountMachineCounts = props => {

    const [counts, setCounts] = useState([]);
    const [loading, setLoading] = useState(false);
    const [errorText, setErrorText] = useState('');

    useEffect(() => {

        if (!props.visible) return;

        let active = true;
        setCounts([]);
        setErrorText('');
        setLoading(true);

        PhysicalCountData.GetMachinePhysicalCounts(props.server, props.location, props.machineName)
            .then(result => { if (active) setCounts(result); })
            .catch(error => { if (active) setErrorText(error.message); })
            .finally(() => { if (active) setLoading(false); });

        return () => { active = false; };

    }, [props.visible]);

    const CountRow = ({ item }) => {
        const status = STATUS_TEXTS[item.status] ?? STATUS_TEXTS.open;
        const open = item.status === 'open';
        const onDevice = props.deviceCountNumber !== null && props.deviceCountNumber === item.countNumber;
        return (
            <TouchableOpacity style={[styles.countRow, { opacity: open ? 1 : 0.5 }]} disabled={!open} onPress={() => props.onPick(item.countNumber)}>
                <View style={styles.countTextContainer}>
                    <Text style={styles.countNumberText}>Count {item.countNumber}</Text>
                    <Text style={styles.countInfoText} numberOfLines={1}>
                        {item.dateCount}   {item.nbrItems} Items{onDevice ? '   ·   On This Device' : ''}
                    </Text>
                </View>
                <Text style={[styles.statusText, { color: status.color }]}>{status.text}</Text>
            </TouchableOpacity>
        );
    };

    return (
        <Modal visible={props.visible} transparent={true} presentationStyle="overFullScreen" onRequestClose={props.onClose}>
            <View style={styles.modalContainer}>
                <View style={styles.modalCard}>

                    <ModalTitle title={`My Counts - ${props.machineName}`} onCloseButtonPress={props.onClose} />

                    {errorText !== '' ? <Text style={styles.errorText}>{errorText}</Text> : <></>}

                    <FlatList
                        data={counts}
                        keyExtractor={(item) => item.countNumber}
                        renderItem={CountRow}
                        ListEmptyComponent={loading || errorText !== '' ? <></> : <Text style={styles.emptyText}>No Counts For This Machine</Text>}
                    />

                    <Text style={styles.hintText}>Tap an open count to download it</Text>

                    {loading ? <LoadingData /> : <></>}

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
        minHeight: 200,
        maxHeight: Sizing.windowsHeight * 0.85,
        backgroundColor: 'white',
        borderRadius: 20,
        overflow: 'hidden',
        elevation: 10,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
    },
    countRow: {
        flexDirection: 'row',
        alignItems: 'center',
        minHeight: 55,
        paddingHorizontal: 12,
        paddingVertical: 4,
        backgroundColor: Colors.listItemBackColor,
        marginBottom: 2,
    },
    countTextContainer: {
        flex: 1,
    },
    countNumberText: {
        color: Colors.brandBlue,
        fontSize: FontSize(18),
        fontWeight: 'bold',
    },
    countInfoText: {
        color: 'gray',
        fontSize: FontSize(14),
    },
    statusText: {
        fontSize: FontSize(15),
        fontWeight: 'bold',
        marginLeft: 8,
    },
    errorText: {
        color: 'red',
        fontSize: FontSize(15),
        fontWeight: 'bold',
        padding: 12,
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

export default PhysicalCountMachineCounts;
