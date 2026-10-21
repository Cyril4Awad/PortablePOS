import React from 'react';
import Sizing from '../../variables/Sizing';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';
import { View, Text, Image, StyleSheet, TouchableOpacity } from 'react-native';

const MenuItem = props => {

    var imagePath = `../../assets/icons/` + props.imagePath

    return (
        <View style={styles.container}>

            <View style={styles.buttonContainer}>

                <TouchableOpacity style={
                                        [styles.button, { backgroundColor: props.menuActive ? Colors.buttonBackColor1 : 'gainsboro' }]} 
                                        disabled={!props.menuActive} 
                                        onPress={props.onPress}>

                    <View style={styles.buttonImageContainer}>
                        <Image
                            source={props.imageSource}
                            style={styles.buttonImage}
                            resizeMode="contain"
                        />
                    </View>

                    <View style={styles.buttonTextContainer}>
                        <View style={styles.buttonTextContainerTitle}>
                            <Text style={styles.buttonTextTitleText} numberOfLines={Sizing.isPortableScanner ? 1 : undefined}>{props.title}</Text>
                        </View>
                        <View style={styles.buttonTextContainerDescription}>
                            <Text style={styles.buttonTextDescriptionText} numberOfLines={Sizing.isPortableScanner ? 2 : undefined}>{props.description}</Text>
                        </View>
                    </View>
                    
                </TouchableOpacity>

            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        width: Sizing.isTablet ? Sizing.windowsWidth / 2 : Sizing.isPortableScanner ? Sizing.windowsWidth : 395,
        height: Sizing.isTablet ? 150 : 130,
        alignItems: 'center',
        justifyContent: 'center',
        //backgroundColor : 'red',
        //marginBottom : 0,
        //borderWidth : 1
    },
    buttonContainer: {
        width: Sizing.isTablet ? '90%' : Sizing.isPortableScanner ? '92%' : '80%',
        //backgroundColor : 'red',
        height: '85%',
        borderRadius: 20,
        overflow: 'hidden',
        borderWidth: 2,
        borderColor: Colors.buttonBorderColor,
        backgroundColor: Colors.buttonBackColor1,
    },
    button: {
        width: '100%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
        flexDirection: 'row',
        //backgroundColor: Colors.buttonBackColor1,
        //backgroundColor: 'red'
    },
    buttonImageContainer: {
        width: '30%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
        //backgroundColor : 'red',
    },
    buttonImage: {
        width: '70%',
        height: '70%',
        overflow: 'hidden',
        resizeMode: 'contain',
    },
    buttonTextContainer: {
        width: '70%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
        paddingRight: Sizing.isPortableScanner ? 5 : 10,
        //backgroundColor : 'blue',
    },
    buttonTextContainerTitle: {
        width: '100%',
        height: '50%',
        justifyContent: 'flex-end',
        //backgroundColor :'red',
    },
    buttonTextTitleText: {
        color: Colors.buttonForeColor1,
        fontSize: FontSize(24),
        //fontWeight: 'bold'
    },
    buttonTextContainerDescription: {
        width: '100%',
        height: '50%',
        justifyContent: 'flex-start',
        //backgroundColor :'blue',
    },
    buttonTextDescriptionText: {
        color: Colors.buttonForeColor2,
        fontSize: FontSize(17),
        //color:'silver',
        fontStyle: 'italic',
        fontWeight : 'bold'
    }
});

export default MenuItem;
