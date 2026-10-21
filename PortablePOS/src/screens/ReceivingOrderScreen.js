import React from 'react';
import { View, Text, Image, StyleSheet, TouchableOpacity, Alert } from 'react-native';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import { useReceivingOrderDraft, StartDraft, ClearDraft } from '../variables/ReceivingOrderDraft';
import { MODE_TITLES, Plural } from '../variables/ReceivingOrderRules';

// A button like the main menu (icon, title, what it does), half of the width : the 5 fit without scrolling
// compact : smaller tiles when there are 5 (Continue) so they all fit on the screen of the scanners
const Tile = ({ title, description, imageSource, onPress, highlighted, compact }) => (
    <View style={styles.tileContainer}>
        <TouchableOpacity style={[styles.tile, compact ? styles.tileCompact : null, highlighted ? styles.tileHighlighted : null]} onPress={onPress}>
            <Image source={imageSource} style={compact ? styles.tileImageCompact : styles.tileImage} resizeMode='contain' />
            <Text style={styles.tileTitleText} numberOfLines={1}>{title}</Text>
            <Text style={styles.tileDescriptionText} numberOfLines={2}>{description}</Text>
        </TouchableOpacity>
    </View>
);

// Receiving Order (FrmDO "Receiving Order", trading and expiry items) : online, saved in one call
// New = a new RO (optionally from a purchase order), View = read only, Modify = new lines added to an RO, Cancel = a reverse RO
const ReceivingOrderScreen = props => {

    const draft = useReceivingOrderDraft();

    // A receiving order being prepared is kept until it is saved or cleared (viewing one does not need to ask)
    const Start = (start) => {
        if (!draft || draft.mode === 'VIEW') {
            start();
            return;
        }
        Alert.alert('Receiving Order Not Saved', `The ${MODE_TITLES[draft.mode].toLowerCase()} being prepared has ${Plural(draft.lines.length, 'new line')}.`, [
            { text: 'Cancel', style: 'cancel' },
            { text: 'Continue It', onPress: () => props.navigation.navigate('ReceivingOrderEdit') },
            { text: 'Clear It', style: 'destructive', onPress: () => { ClearDraft(); start(); } },
        ]);
    };

    const preparing = draft && draft.mode !== 'VIEW';

    return (
        <View style={styles.container}>

            {preparing ?
                <TouchableOpacity style={styles.draftContainer} onPress={() => props.navigation.navigate('ReceivingOrderEdit')}>
                    <Text style={styles.draftTitleText}>{MODE_TITLES[draft.mode]}{draft.roRecID ? ` #${draft.roRecID}` : ''}  ·  Continue ›</Text>
                    <Text style={styles.draftText} numberOfLines={1}>
                        {draft.supplier ? draft.supplier.supplierName : 'No Supplier'}  ·  {Plural(draft.lines.length, 'new line')}
                    </Text>
                </TouchableOpacity> :
                <></>}

            <View style={styles.tilesContainer}>

                <Tile compact={!!preparing} title='New' imageSource={require('../assets/icons/trolleyAddEmpty.png')}
                    description='Receive the items of a supplier, from a PO or not'
                    onPress={() => Start(() => { StartDraft('ADD'); props.navigation.navigate('ReceivingOrderEdit'); })} />

                <Tile compact={!!preparing} title='View' imageSource={require('../assets/icons/orders.png')}
                    description='Open a receiving order to see it, nothing changes'
                    onPress={() => Start(() => props.navigation.navigate('ReceivingOrderList', { mode: 'VIEW' }))} />

                <Tile compact={!!preparing} title='Modify' imageSource={require('../assets/icons/trolleyEdit.png')}
                    description='Add new lines to a receiving order'
                    onPress={() => Start(() => props.navigation.navigate('ReceivingOrderList', { mode: 'MODIFY' }))} />

                <Tile compact={!!preparing} title='Cancel RO' imageSource={require('../assets/icons/ReturnOrders.png')}
                    description='A reverse RO takes its quantities out of the stock'
                    onPress={() => Start(() => props.navigation.navigate('ReceivingOrderList', { mode: 'CANCEL' }))} />

                {preparing ?
                    <Tile compact={!!preparing} title='Continue' imageSource={require('../assets/icons/trolleyFull.png')} highlighted={true}
                        description='Back to the receiving order being prepared'
                        onPress={() => props.navigation.navigate('ReceivingOrderEdit')} /> :
                    <></>}

            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        height: '100%',
    },
    draftContainer: {
        margin: 8,
        marginBottom: 0,
        padding: 8,
        borderRadius: 10,
        borderWidth: 1,
        borderColor: Colors.brandGreen,
        backgroundColor: 'rgb(235, 248, 240)',
    },
    draftTitleText: {
        color: Colors.brandGreen,
        fontSize: FontSize(15),
        fontWeight: 'bold',
    },
    draftText: {
        color: 'black',
        fontSize: FontSize(14),
    },
    tilesContainer: {
        flexDirection: 'row',
        flexWrap: 'wrap',
        padding: 4,
    },
    tileContainer: {
        width: Sizing.isTablet ? '33.3%' : '50%',
        padding: 4,
    },
    tile: {
        height: Sizing.isTablet ? 190 : Sizing.isPortableScanner ? 135 : 150,
        borderRadius: 20,
        borderWidth: 2,
        borderColor: Colors.buttonBorderColor,
        backgroundColor: Colors.buttonBackColor1,
        alignItems: 'center',
        justifyContent: 'center',
        paddingHorizontal: 8,
    },
    tileCompact: {
        height: Sizing.isTablet ? 160 : Sizing.isPortableScanner ? 104 : 125,
    },
    tileHighlighted: {
        borderColor: Colors.brandGreen,
        backgroundColor: 'rgb(235, 248, 240)',
    },
    tileImage: {
        width: 46,
        height: 46,
        marginBottom: 4,
    },
    tileImageCompact: {
        width: 32,
        height: 32,
        marginBottom: 2,
    },
    tileTitleText: {
        color: Colors.buttonForeColor1,
        fontSize: FontSize(19),
    },
    tileDescriptionText: {
        color: Colors.buttonForeColor2,
        fontSize: FontSize(12),
        fontStyle: 'italic',
        fontWeight: 'bold',
        textAlign: 'center',
    },
});

export default ReceivingOrderScreen;
