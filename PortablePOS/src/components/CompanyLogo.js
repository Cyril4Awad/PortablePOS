import React from 'react';
import Sizing from '../variables/Sizing';
import Colors from '../variables/Colors';
import { View, StyleSheet, Image } from 'react-native';

const CompanyLogo = props => {

    return (
        <View style={styles.container}>
            <View style={styles.imageContainer}>

                <Image
                    source={require('./../assets/images/logo.png')}
                    style={styles.logo}
                    resizeMode="contain"
                />

            </View>
        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 0,
        width: '100%',
        height: '15%',
        alignItems: 'center',
        justifyContent: 'center',
        overflow: 'hidden',
        padding: 15,
        //backgroundColor: 'red',
        backgroundColor: Colors.bgColor,
        elevation :20,
        //marginBottom: 5
    },
    imageContainer: {
        //backgroundColor: 'yellow',
        width: '100%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    logo: {
        width: '75%',
        height: '75%',
        overflow: 'hidden',
        resizeMode: 'contain',
        //backgroundColor:'blue'
    },
});

export default CompanyLogo;
