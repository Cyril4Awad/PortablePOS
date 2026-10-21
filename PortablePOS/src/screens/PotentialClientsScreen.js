import React, { useRef } from 'react';
import { useState, useEffect } from 'react';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import ScreenHeader from '../components/ScreenHeader';
import { View, Text, ScrollView, StyleSheet, Button, Modal, Image, TouchableOpacity } from 'react-native';
import TextBoxField from '../components/controls/TextBoxField';
import MainButton from '../components/controls/MainButton';
import ClientsData from '../data/ClientsData';
import { FontSize } from '../variables/ResponsiveFont';
import Slider from '@react-native-community/slider';
import GeoLocationInfo from '../variables/GeoLocationInfo';
import { UseLocation } from '../variables/AppConfig';
import MapView from '../components/MapView';

const PotentialClientsScreen = props => {
    
    const [mapVisible, setMapVisible] = useState(false);

    const [clientName, setClientName] = useState('');
    const [business, setBusiness] = useState('');
    const [contact, setContact] = useState('');
    const [phoneMobile, setPhoneMobile] = useState('');
    const [address, setAddress] = useState('');
    const [remarks, setRemarks] = useState('');
    const [location, setLocation] = useState(false);
    const [sliderRadiusValue, setSliderRadiusValue] = useState(50);

    useEffect(() => {

        if (UseLocation) {
            GeoLocationInfo.GetLocation().then((loc) => {
                setLocation(loc)
            });
        }
    
      }, []);
 
    const ClearData = () => {
        setClientName('');
        setBusiness('');
        setContact('');
        setPhoneMobile('');
        setAddress('');
        setRemarks('');
        setLocation('');
        setSliderRadiusValue(50);
        if (UseLocation) {
            GeoLocationInfo.GetLocation().then((loc) => {
                setLocation(loc)
            });
        }
    };

    return (
        <View style={styles.container}>

            <Modal
                style={styles.modalStyle}
                visible={mapVisible}
                animationType='fade'
                transparent={true}
                presentationStyle='overFullScreen'>

                <View style={styles.modalContainer}>
                    <View style={styles.modalCard}>
                        <MapView clientName={clientName} latitude={location ? location.coords.latitude : null} longitude={location ? location.coords.longitude : null} setMapVisible={setMapVisible}/>
                    </View>
                </View>

            </Modal>

            <ScreenHeader
                description='Request adding a new Client Account'
                imageSource={require('../assets/icons/potentialClient.png')}
            />

            <TextBoxField
                title='Client Name'
                value={clientName}
                multiline={true}
                maxLength={100}
                numberOfLines={1}
                onChangeText={(value) => {setClientName(value)}} />

            <TextBoxField
                title='Business'
                value={business}
                multiline={true}
                maxLength={100}
                numberOfLines={1}
                onChangeText={(value) => { setBusiness(value) }} />

            <TextBoxField
                title='Contact'
                value={contact}
                multiline={false}
                maxLength={100}
                numberOfLines={1}
                onChangeText={(value) => {setContact(value)}} />

            <TextBoxField
                title='Phone / Mobile'
                value={phoneMobile}
                multiline={false}
                maxLength={100}
                numberOfLines={1}
                onChangeText={(value) => { setPhoneMobile(value) }} />

            <TextBoxField
                title='Address'
                value={address}
                multiline={true}
                maxLength={500}
                numberOfLines={5}
                onChangeText={(value) => { setAddress(value) }} />

            <TextBoxField
                title='Remarks'
                value={remarks}
                multiline={true}
                maxLength={500}
                numberOfLines={5}
                onChangeText={(value) => { setRemarks(value) }} />

            {UseLocation ? (
            <View style={styles.geoLocationContainer}>

                <View style={styles.geoLocationTitle}>
                    <Text style={styles.geoLocationTitleText}>Geo Location</Text>
                </View>

                <View style={styles.geoLocationDetails}>

                    <View style={styles.geoLocationDetailsDataAndButton}>

                        <View style={styles.geoLocationDetailsData}>

                            <Text style={styles.geoLocationDetailsCaptionText}>Accurency : <Text style={styles.geoLocationDetailsDataText}>{location ? location.coords.accuracy : null}</Text></Text>
                            <Text style={styles.geoLocationDetailsCaptionText}>Latitude : <Text style={styles.geoLocationDetailsDataText}>{location ? location.coords.latitude : null}</Text></Text>
                            <Text style={styles.geoLocationDetailsCaptionText}>Longitude : <Text style={styles.geoLocationDetailsDataText}>{location ? location.coords.longitude : null}</Text></Text>
                            <Text style={styles.geoLocationDetailsCaptionText}>Radius in Meters : <Text style={styles.geoLocationDetailsDataText}>{sliderRadiusValue}</Text></Text>

                        </View>

                        <TouchableOpacity style={styles.geoLocationDetailsButton} onPress={() => {setMapVisible(true)}}>

                            <Image
                                source={require('../assets/icons/openMap.png')}
                                style={styles.geoLocationDetailsButtonImage}
                                resizeMode="contain"
                            />

                        </TouchableOpacity>

                    </View>



                    <Slider
                        style={styles.geoLocationDetailsSlider}
                        minimumValue={0}
                        maximumValue={5000}
                        step={10}
                        value={sliderRadiusValue}
                        onValueChange={setSliderRadiusValue}
                        minimumTrackTintColor={Colors.brandBlue}
                        maximumTrackTintColor={Colors.brandGreen}
                        thumbTintColor={Colors.brandGreen}
                    />

                </View>

            </View>
            ) : null}

            <View style={styles.buttonsContainer}>

                <MainButton title='Save Request' onPress={() => {
                    if (clientName == '') {
                        alert('Client Name is Mandatory');
                    }
                    else if (phoneMobile == '') {
                        alert('Phone/Mobile is Mandatory');
                    }
                    else {
                        ClientsData.SavePotentialClient(clientName, 
                                                        business,
                                                        contact,
                                                        phoneMobile,
                                                        address, 
                                                        remarks, 
                                                        UseLocation && location ? location.coords.latitude : -1,
                                                        UseLocation && location ? location.coords.longitude : -1,
                                                        UseLocation && location ? location.coords.accuracy : -1,
                                                        sliderRadiusValue);
                        ClearData();
                    }
                }} />

                <MainButton title='Clear Request' onPress={() => {
                    ClearData();
                }} />

            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        height: '100%',
        padding: 10,
    },
    buttonsContainer: {
        alignItems: Sizing.isTablet ? 'flex-end' : 'center',
        justifyContent: 'center',
        width: '100%',
        padding: 10,
    },
    geoLocationContainer: {
        //backgroundColor: 'red',
        width: '100%',
        borderLeftWidth: 2,
        borderBottomWidth: 2,
        borderColor: 'silver',
        paddingLeft: 8,
        paddingRight: 5,
    },
    geoLocationTitle: {
        //backgroundColor: 'red',
        width: '100%',
        //height: 30,
        alignItems: 'flex-start',
        justifyContent: 'center',
    },
    geoLocationTitleText: {
        fontSize: FontSize(14),
        fontWeight: 'bold',
        color: Colors.brandGreen,
    },
    geoLocationDetails: {
        //backgroundColor: 'yellow',
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingTop: 5,
        paddingBottom: 5,
    },
    geoLocationDetailsDataAndButton:{
        //backgroundColor: Colors.brandBlue,
        width: '100%',
        flexDirection: 'row',
    },
    geoLocationDetailsData: {
        backgroundColor: Colors.bgColor,
        width: '80%',
    },
    geoLocationDetailsButton: {
        backgroundColor: Colors.bgColor,
        width: '20%',
        height: 70,
        alignItems: 'center',
        justifyContent: 'center',
    },
    geoLocationDetailsButtonImage: {
        width: '75%',
        height: '75%',
        overflow: 'hidden',
        resizeMode: 'contain',
    },
    geoLocationDetailsCaptionText: {
        color: 'black',
        fontSize: FontSize(15),
    },
    geoLocationDetailsDataText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
    },
    geoLocationDetailsSlider : {
        width: '100%',
        paddingTop: 12
    },
    modalContainer: {
        width: '100%',
        height: '100%',
        backgroundColor: 'rgba(0,0,0,0.5)',
        alignItems: 'center',
        justifyContent: 'center',
    },
    modalCard: {
        width: Sizing.isPortableScanner ? Sizing.windowsWidth * 0.9 : Sizing.windowsWidth * 0.9,
        height: Sizing.isPortableScanner ? Sizing.windowsHeight * 0.8 : Sizing.windowsHeight * 0.6,
        backgroundColor: 'white',
        borderRadius: 20,
        overflow: 'hidden',
        elevation: 10,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
        alignItems: 'center',
        justifyContent: 'center',
    },
});

export default PotentialClientsScreen;
