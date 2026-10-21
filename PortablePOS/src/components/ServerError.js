import React from 'react';
import { View, Text, StyleSheet, Image, Button, TouchableOpacity } from 'react-native';
import Sizing from '../variables/Sizing';
import Colors from '../variables/Colors';
import { FontSize } from '../variables/ResponsiveFont';

const ServerError = props => {

    return (
        <View style={styles.container}>
            
            <View style={styles.imageContainer}>
                <Image
                    source={require('../assets/images/error.png')}
                    style={styles.image}
                    resizeMode="contain"
                />
            </View>

            <View style={styles.messageContainer}>
                <Text style={styles.messageTitleText}>Ooops...</Text>
                <Text style={styles.messageDetailsText}>Something went wrong...</Text>
                <Text style={styles.messageDetailsText}>Please try again...</Text>

            </View>

            <View style={styles.buttonsContainer}>

                <TouchableOpacity style={styles.buttonContainer} onPress={()=> alert(props.errorMessage)}>
                    <Text style={styles.buttonText}>Error Details</Text>
                </TouchableOpacity>

                <TouchableOpacity style={styles.buttonContainer} onPress={()=> props.setError(null)}>
                    <Text style={styles.buttonText}>Continue</Text>
                </TouchableOpacity>

            </View>

        </View>
    )
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        //height: Sizing.WindowsHeight,
        //position: 'absolute',
        //backgroundColor: Colors.brandGreen,
        backgroundColor: 'black',
        //left: 0,
        //right: 0,
        //top: 0,
        //bottom: 0,
        alignItems: 'center',
        justifyContent: 'flex-start',
        //padding: 30
    },
    imageContainer: {
        width: '100%',
        height: '35%',
        //backgroundColor: 'green',
        //borderWidth: 2,
        alignItems: 'center',
        justifyContent: 'space-evenly',
    },
    image: {
        width: '70%',
        height: '70%',
        overflow: 'hidden',
        resizeMode: 'contain',
    },
    messageContainer: {
        width: '100%',
        height: '25%',
        //backgroundColor: 'green',
        //borderWidth: 2,
        alignItems: 'center',
        justifyContent: 'space-evenly',
    },
    messageTitleText: {
        fontSize: FontSize(40),
        color: 'white',
    },
    messageDetailsText: {
        fontSize: FontSize(20),
        color: 'silver',
    },
    buttonsContainer: {
        width: '100%',
        height: '25%',
        //backgroundColor: 'green',
        //borderWidth: 2,
        alignItems: 'center',
        justifyContent: 'space-evenly',
    },
    buttonContainer: {
        backgroundColor: Colors.brandBlue,
        width: 140,
        height: 40,
        alignItems: 'center',
        justifyContent: 'center',
        borderRadius: 5
    },
    buttonText: {
        fontSize: FontSize(20),
        color: 'white',
    }
});

export default ServerError;
