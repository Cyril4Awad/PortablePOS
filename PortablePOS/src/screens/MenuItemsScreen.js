import React, { useState, useCallback } from 'react';
import { View, Text, FlatList, Image, TouchableOpacity, StyleSheet } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import Colors from '../variables/Colors';
import { FontSize } from '../variables/ResponsiveFont';
import { getLoginUserCode } from '../variables/LoginInfo';
import MenuHeader from '../components/menu/MenuHeader';
import MainButton from '../components/controls/MainButton';
import LoadingData from '../components/LoadingData';
import ServerError from '../components/ServerError';
import MenuData from '../data/MenuData';
import { STEPS } from '../components/menu/MainMenu';

// Super user screen: choose which main menu items are shown (tbl_CPP_ControlizedMenu)
const MenuItemsScreen = props => {

    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);

    const [menuActiveKeys, setMenuActiveKeys] = useState({});

    useFocusEffect(
        useCallback(() => {
            MenuData.GetControlizedMenu(
                { setMenuActiveKeys },
                { setIsLoading },
                { setError }
            );
        }, [])
    );

    const isChecked = item => menuActiveKeys[item.recKey] ?? item.menuActive;

    const toggleItem = item => {
        setMenuActiveKeys(currentKeys => ({ ...currentKeys, [item.recKey]: !(currentKeys[item.recKey] ?? item.menuActive) }));
    };

    const saveMenuItems = () => {
        MenuData.SaveControlizedMenu(
            STEPS.filter(item => item.type === 'MI').map(item => ({ recKey: item.recKey, menuActive: isChecked(item) })),
            { setIsLoading },
            () => props.navigation.navigate('Main')
        );
    };

    if (getLoginUserCode().toUpperCase() !== 'SU') {
        return (
            <View style={styles.messageContainer}>
                <Text style={styles.messageText}>Only the super user can modify the menu items</Text>
            </View>
        );
    };

    //While Loading Or Searching Data
    if (isLoading) {
        return (
            <LoadingData />
        );
    };

    //In case of error
    if (error) {
        return (
            <ServerError errorMessage={error.toString()} setError={setError} />
        );
    };

    const MenuItemRow = menuItem => {

        if (menuItem.item.type === 'MH') {
            return (
                <MenuHeader title={menuItem.item.title} />
            );
        }

        return (
            <TouchableOpacity style={styles.itemContainer} onPress={() => toggleItem(menuItem.item)}>

                <View style={styles.itemImageContainer}>
                    <Image
                        source={menuItem.item.imageSource}
                        style={styles.itemImage}
                        resizeMode="contain"
                    />
                </View>

                <View style={styles.itemTextContainer}>
                    <Text style={styles.itemTitleText}>{menuItem.item.title}</Text>
                    <Text style={styles.itemDescriptionText}>{menuItem.item.description}</Text>
                </View>

                <View style={styles.itemCheckBoxContainer}>
                    <Image
                        source={isChecked(menuItem.item) ? require('../assets/icons/checked.png') : require('../assets/icons/unchecked.png')}
                        style={styles.itemCheckBoxImage}
                        resizeMode="contain"
                    />
                </View>

            </TouchableOpacity>
        );
    };

    return (
        <View style={styles.container}>

            <FlatList
                style={styles.list}
                keyExtractor={item => item.recKey}
                data={STEPS}
                renderItem={MenuItemRow}
            />

            <View style={styles.buttonsContainer}>
                <MainButton title='Save' onPress={saveMenuItems} />
            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: Colors.bgColor,
    },
    list: {
        flex: 1,
    },
    itemContainer: {
        flexDirection: 'row',
        alignItems: 'center',
        height: 70,
        paddingHorizontal: 10,
        borderBottomWidth: 1,
        borderBottomColor: 'gainsboro',
    },
    itemImageContainer: {
        width: 50,
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    itemImage: {
        width: '75%',
        height: '75%',
    },
    itemTextContainer: {
        flex: 1,
        justifyContent: 'center',
        paddingHorizontal: 10,
    },
    itemTitleText: {
        color: Colors.brandBlue,
        fontSize: FontSize(18),
    },
    itemDescriptionText: {
        color: 'gray',
        fontSize: FontSize(14),
        fontStyle: 'italic',
    },
    itemCheckBoxContainer: {
        width: 40,
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    itemCheckBoxImage: {
        width: 30,
        height: 30,
    },
    buttonsContainer: {
        alignItems: 'center',
        paddingTop: 10,
    },
    messageContainer: {
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
        padding: 20,
    },
    messageText: {
        color: Colors.brandBlue,
        fontSize: FontSize(18),
        textAlign: 'center',
    },
});

export default MenuItemsScreen;
