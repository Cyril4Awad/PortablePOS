import React from 'react';
import Colors from '../variables/Colors';
import CompanyLogo from '../components/CompanyLogo';
import MenuFooter from '../components/menu/MenuFooter';
import MainMenu from '../components/menu/MainMenu';
import CustomDrawer from '../components/CustomDrawer';

import OpeningScreen from './OpeningScreen';
import ClosingScreen from './ClosingScreen';
import SettingsScreen from './SettingsScreen';
import WebsiteScreen from './WebsiteScreen';
import MenuItemsScreen from './MenuItemsScreen';
import { getLoginUserCode } from '../variables/LoginInfo';

import { createDrawerNavigator } from '@react-navigation/drawer';
import { baseHeaderScreenOptions } from '../variables/NavigationOptions';

import { View, Text, StyleSheet } from 'react-native';

const MainScreen = props => {

    return (
        <View style={styles.container}>

            <CompanyLogo />
            <MainMenu {...props} />
            <MenuFooter />

        </View>
    );
};

const HomeScreen = params => {

    const Drawer = createDrawerNavigator();

    return (
        <Drawer.Navigator
            initialRouteName="Main"
            screenOptions={baseHeaderScreenOptions}
            drawerContent={props => <CustomDrawer {...props} {...params} />}>

            <Drawer.Screen
                name="Main"
                component={MainScreen}
                options={{
                    title: 'Mobile Trading',
                }} />

            <Drawer.Screen
                name="Opening"
                component={OpeningScreen}
                options={{
                    title: 'Opening',
                }} />

            {/*<Drawer.Screen
                name="Closing"
                component={ClosingScreen}
                options={
                    {
                        title: 'Closing',
                        headerStyle: {
                            backgroundColor: Colors.brandBlue,
                        },
                        headerTintColor: Colors.navTitleColor
                    }
                } /> */}

            <Drawer.Screen
                name="Settings"
                component={SettingsScreen}
                options={{
                    title: 'Settings',
                }} />

            {getLoginUserCode().toUpperCase() === 'SU' &&
                <Drawer.Screen
                    name="MenuItems"
                    component={MenuItemsScreen}
                    options={{
                        title: 'Menu Items',
                    }} />
            }

            <Drawer.Screen
                name="About"
                component={WebsiteScreen}
                options={{
                    title: 'About',
                }} />

        </Drawer.Navigator>
    );
};

const styles = StyleSheet.create({
    container: {
        //backgroundColor: 'red',// Colors.bgColor,
        alignItems: 'center',
        justifyContent: 'center',
        width: '100%',
        height: '100%',
    },
});

export default HomeScreen;
