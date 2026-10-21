import React, { useState, useEffect } from 'react';
import { View, StyleSheet, Text, TextInput, FlatList } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import PhysicalCountLine from './PhysicalCountLine';
import PhysicalCountDatabase from '../../../database/PhysicalCountDatabase';

const PICK_LIMIT = 100;

// Opens when the scanned text is the start of more than one reference, or a barcode of more than one line (byBarcode) :
// the picked line is counted. The scanned reference shows the lines it starts, typing in the box also searches the description
const PhysicalCountItemPicker = props => {

    const [searchString, setSearchString] = useState(props.searchString);
    // the lines of the scanned barcode / reference until the user types
    const [byBarcode, setByBarcode] = useState(props.byBarcode === true);
    const [typed, setTyped] = useState(false);
    const [lines, setLines] = useState([]);

    useEffect(() => {

        if (searchString.trim() === '') {
            setLines([]);
            return;
        }

        let active = true;

        const search = byBarcode
            ? PhysicalCountDatabase.FindLinesByBarcode(props.countKey, searchString.trim(), PICK_LIMIT)
            : typed
                ? PhysicalCountDatabase.FindLinesBySearch(props.countKey, searchString.trim(), PICK_LIMIT)
                : PhysicalCountDatabase.FindLinesByReferencePrefix(props.countKey, searchString.trim(), PICK_LIMIT);

        search
            .then(result => { if (active) setLines(result); })
            .catch(err => console.log(err));

        return () => { active = false; };

    }, [searchString, byBarcode, typed]);

    const LineItem = lineData => {
        return (
            <PhysicalCountLine
                line={lineData.item}
                header={props.header}
                onPress={() => props.onPick(lineData.item.lineRecID)}
            />
        );
    };

    return (
        <View style={styles.container}>

            <ModalTitle title={byBarcode ? 'Choose The Item' : 'Search Item'} onCloseButtonPress={props.onCloseButtonPress} />

            <View style={styles.searchContainer}>
                <TextInput
                    style={styles.searchText}
                    placeholder='Reference Or Description'
                    maxLength={30}
                    value={searchString}
                    autoFocus={true}
                    autoCapitalize='characters'
                    autoCorrect={false}
                    onChangeText={text => {
                        setByBarcode(false);
                        setTyped(true);
                        setSearchString(text);
                    }}
                />
            </View>

            <FlatList
                data={lines}
                keyExtractor={(item) => item.lineRecID}
                renderItem={LineItem}
                ListEmptyComponent={<Text style={styles.emptyText}>No Items Found</Text>}
            />

            {lines.length === PICK_LIMIT ?
                <Text style={styles.limitText}>Showing the first {PICK_LIMIT} items, type more of the reference</Text> :
                <></>}

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        width: '100%',
        backgroundColor: Colors.bgColor,
    },
    searchContainer: {
        width: '100%',
        height: 60,
        padding: 5,
    },
    searchText: {
        width: '100%',
        height: '100%',
        borderWidth: 1,
        borderColor: 'silver',
        paddingLeft: 25,
        fontSize: FontSize(23),
        backgroundColor: 'white',
        color: 'black',
    },
    emptyText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(17),
        padding: 20,
    },
    limitText: {
        textAlign: 'center',
        color: 'gray',
        fontSize: FontSize(13),
        padding: 5,
    },
});

export default PhysicalCountItemPicker;
