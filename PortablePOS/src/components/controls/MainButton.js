import React from 'react';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity } from 'react-native';
import Colors from '../../variables/Colors';
import Sizing from '../../variables/Sizing';
import { FontSize } from '../../variables/ResponsiveFont';

// imageSource (optional) : icon shown at the left of the title, the button is taller with an icon
const MainButton = props => {

    return (
        <View style={[styles.container, props.imageSource ? styles.containerWithImage : null]}>
            <TouchableOpacity style={[styles.button, props.imageSource ? styles.buttonWithImage : null]} onPress={props.onPress}>
                {props.imageSource ?
                    <Image
                        source={props.imageSource}
                        style={styles.buttonImage}
                        resizeMode="contain"
                    /> :
                    <></>}
                <Text style={styles.buttonText} numberOfLines={1}>{props.title}</Text>
            </TouchableOpacity>
        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        width: Sizing.isTablet ? '30%' : '49%',
        height: 40,
        marginBottom: 10,
        borderRadius: 15,
        overflow: 'hidden',
        backgroundColor: Colors.buttonBackColor2,
        borderWidth: 1,
        borderColor: Colors.buttonBorderColor,
    },
    containerWithImage: {
        height: 50,
    },
    button: {
        width : '100%',
        height : '100%',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: Colors.buttonBackColor1,
    },
    buttonWithImage: {
        flexDirection: 'row',
        justifyContent: 'flex-start',
        paddingLeft: 8,
        paddingRight: 5,
    },
    buttonImage: {
        width: 32,
        height: 32,
        marginRight: 6,
    },
    buttonText: {
        fontSize: FontSize(18),
        color: Colors.buttonForeColor1,
        flexShrink: 1,
    },
});

export default MainButton;
