import React from 'react';
import Colors from '../variables/Colors';
import { View, Text, ScrollView, StyleSheet, Button } from 'react-native';
import { WebView } from 'react-native-webview';

const WebsiteScreen = props => {

    return (
        <View style={styles.container}>
            <WebView
                source={{uri: 'https://www.example.com/'}}
            />
        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        height: '100%',
    },
});

export default WebsiteScreen;
