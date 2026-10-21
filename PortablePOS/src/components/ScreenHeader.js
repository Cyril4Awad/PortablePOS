import React from 'react';
import Sizing from '../variables/Sizing';
import Colors from '../variables/Colors';
import { FontSize } from '../variables/ResponsiveFont';
import { View, StyleSheet, Text, Image } from 'react-native';

const ScreenHeader = props => {

    return (
        <View style={styles.container}>

            <View style={styles.headerImageContainer}>
                <Image
                    source={props.imageSource}
                    style={styles.headerImage}
                    resizeMode="contain"
                />
            </View>

            <View style={styles.headerTextContainer}>
                <Text style={styles.headerText}>{props.description}</Text>
            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        elevation: 15,
        marginBottom: 10,
        width: '100%',
        height: '10%',
        backgroundColor: Colors.headerBackColor,
        justifyContent: 'flex-start',
        flexDirection: 'row',
    },
    headerImageContainer: {
        //backgroundColor : 'blue',
        width: 100,
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    headerImage: {
        width: '85%',
        height: '85%',
        overflow: 'hidden',
        resizeMode: 'contain',
    },
    headerTextContainer: {
        //backgroundColor : 'red',
        width: Sizing.windowsWidth - 120,
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingTop: 3,
        paddingBottom: 3,
        paddingRight: 3,
    },
    headerText: {
        color: Colors.brandGreen,
        fontSize: FontSize(18),
    },
});

export default ScreenHeader;
