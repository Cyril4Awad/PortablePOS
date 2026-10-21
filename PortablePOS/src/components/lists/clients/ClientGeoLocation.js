import React, {useState, useEffect} from 'react';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, PermissionsAndroid, Modal } from 'react-native';
import Colors from '../../../variables/Colors';
import Sizing from '../../../variables/Sizing';
import { FontSize } from '../../../variables/ResponsiveFont';
import ClientsData from '../../../data/ClientsData';
import Slider from '@react-native-community/slider';

import GeoLocationInfo from '../../../variables/GeoLocationInfo';
import MapView from '../../MapView';

//import Geolocation from 'react-native-geolocation-service';

// -1 : Geo Location is not defined
//  1 : Inside area
//  0 : Outside area

const ClientGeoLocation = props => {

    const [mapStoredVisible, setMapStoredVisible] = useState(false);
    const [mapCurrentVisible, setMapCurrentVisible] = useState(false);

    const [location, setLocation] = useState(false);
    const [clientGeoLocation, setClientGeoLocation] = useState(false);
    const [insideClientArea, setInsideClientArea] = useState(false);
    const [sliderRadiusValue, setSliderRadiusValue] = useState(50);

    useEffect(() => {

        GeoLocationInfo.GetLocation().then((loc) => {
            setLocation(loc)
        }
        );

    }, []);


    useEffect(() => {

        if (location) {
        ClientsData.GetClientGeoLocation(props.clientRecID,
            location.coords.latitude,
            location.coords.longitude, 
            { setClientGeoLocation })

        }

        }, [location,setLocation]);

        return (

        <View style={styles.container} >

                {/* Map Stored */}
                <Modal
                    style={styles.modalStyle}
                    visible={mapStoredVisible}
                    animationType='fade'
                    transparent={true}
                    presentationStyle='overFullScreen'>

                    <View style={styles.modalContainer}>
                        <View style={styles.modalCard}>
                            <MapView clientName={props.clientName} latitude={clientGeoLocation[0] ? clientGeoLocation[0].latitude : null} longitude={clientGeoLocation[0] ? clientGeoLocation[0].longitude : null} setMapVisible={setMapStoredVisible} />
                        </View>
                    </View>

                </Modal>

                {/* Map Current */}
                <Modal
                    style={styles.modalStyle}
                    visible={mapCurrentVisible}
                    animationType='fade'
                    transparent={true}
                    presentationStyle='overFullScreen'>

                    <View style={styles.modalContainer}>
                        <View style={styles.modalCard}>
                            <MapView clientName={props.clientName} latitude={location ? location.coords.latitude : null} longitude={location ? location.coords.longitude : null} setMapVisible={setMapCurrentVisible} />
                        </View>
                    </View>

                </Modal>

            <View style={styles.headerContainer}>

                <View style={styles.headerTextContainer}>

                    <Text style={styles.clientNameText} numberOfLines={2}>{props.clientName}</Text>

                    <Text style={[styles.clientICAText, {
                        color: clientGeoLocation[0] ? clientGeoLocation[0].status === 0 ? 'red' : Colors.brandBlue : 'red'
                    }]}>{clientGeoLocation[0] ? clientGeoLocation[0].status === 0 ? 'Your are outside client area' : 'You are inside client area' : null}</Text>

                </View>

                <View style={styles.headerImageContainer}>
                    <Image
                        source={clientGeoLocation[0] ? clientGeoLocation[0].status === 0 ? require('../../../assets/icons/geoLocationBW.png') : require('../../../assets/icons/geoLocation.png') : require('../../../assets/icons/geoLocationEmpty.png')}
                        style={styles.headerImage}
                        resizeMode="contain"
                    />
                </View>

            </View>

            <View style={styles.detailsContainer}>

                <View style={styles.detailsContainerSplit}>

                    <Text style={styles.detailsContainerSplitTitleText}>Stored Geo Location : </Text>

                    <View style={styles.geoLocationDetailsDataAndButton}>

                            <View style={styles.geoLocationDetailsData}>

                                <Text style={styles.detailsContainerSplitLLCaption}>
                                    Accuracy :
                                    <Text style={styles.detailsContainerSplitLLData}> {clientGeoLocation[0] ? clientGeoLocation[0].accuracy : null}</Text>
                                </Text>

                                <Text style={styles.detailsContainerSplitLLCaption}>
                                    Latitude :
                                    <Text style={styles.detailsContainerSplitLLData}> {clientGeoLocation[0] ? clientGeoLocation[0].latitude : null}</Text>
                                </Text>

                                <Text style={styles.detailsContainerSplitLLCaption}>
                                    Longitude :
                                    <Text style={styles.detailsContainerSplitLLData}> {clientGeoLocation[0] ? clientGeoLocation[0].latitude : null}</Text>
                                </Text>

                                <Text style={styles.detailsContainerSplitLLCaption}>
                                    Radius In Meters :
                                    <Text style={styles.detailsContainerSplitLLData}> {clientGeoLocation[0] ? Intl.NumberFormat("ar-de").format(clientGeoLocation[0].radiusInMeters) : null} </Text>
                                </Text>

                                <Text style={styles.detailsContainerSplitLLCaption}>
                                    Distance In Meters :
                                    <Text style={styles.detailsContainerSplitLLData}> {clientGeoLocation[0] ? Intl.NumberFormat("ar-de").format(clientGeoLocation[0].distanceInMeters) : null} </Text>
                                </Text>

                            </View>

                            <TouchableOpacity style={styles.geoLocationDetailsButton} onPress={() => { setMapStoredVisible(true) }} disabled={!clientGeoLocation[0]}>

                                <Image
                                    source={require('../../../assets/icons/openMap.png')}
                                    style={styles.geoLocationDetailsButtonImage}
                                    resizeMode="contain"
                                />

                            </TouchableOpacity>

                    </View>

                </View>

                    <View style={styles.detailsContainerSplit}>

                        <Text style={styles.detailsContainerSplitTitleText}>Current Geo Location : </Text>

                        <View style={[styles.geoLocationDetailsDataAndButton,{height: 75}]}>

                            <View style={styles.geoLocationDetailsData}>

                                <Text style={styles.detailsContainerSplitLLCaption}>Accuracy :
                                    <Text style={styles.detailsContainerSplitLLData}> {location ? location.coords.accuracy : null}</Text>
                                </Text>

                                <Text style={styles.detailsContainerSplitLLCaption}>
                                    Latitude :
                                    <Text style={styles.detailsContainerSplitLLData}> {location ? location.coords.latitude : null}</Text>
                                </Text>

                                <Text style={styles.detailsContainerSplitLLCaption}>
                                    Longitude :
                                    <Text style={styles.detailsContainerSplitLLData}> {location ? location.coords.latitude : null}</Text>
                                </Text>

                                <Text style={styles.detailsContainerSplitLLCaption}>
                                    Radius In Meters :
                                    <Text style={styles.detailsContainerSplitLLData}> {sliderRadiusValue} </Text>
                                </Text>

                                <Slider
                                    style={styles.detailsContainerSplitSlider}
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

                            <TouchableOpacity style={styles.geoLocationDetailsButton} onPress={() => { setMapCurrentVisible(true) }}>

                                <Image
                                    source={require('../../../assets/icons/openMap.png')}
                                    style={styles.geoLocationDetailsButtonImage}
                                    resizeMode="contain"
                                />

                            </TouchableOpacity>

                        </View>

                </View>

            </View>

            <View style={styles.footerContainer}>

                <TouchableOpacity style={styles.mainButton} onPress={() => {
                    if (location) {

                        ClientsData.SaveClientGeoLocation(props.clientRecID,
                            location.coords.latitude,
                            location.coords.longitude,
                            location.coords.accuracy,
                            sliderRadiusValue);
                        props.setGeoLocationModalVisible(false);

                    }
                    else {
                        alert('Current geo location is not defined')
                    }
                }}>
                    <Text style={styles.mainButtonText}>Save</Text>
                </TouchableOpacity>

                <TouchableOpacity style={[styles.mainButton, 
                                        {backgroundColor: clientGeoLocation[0] ? clientGeoLocation[0].status === 1 ? Colors.brandBlue : 'gray' : 'gray'}]} 
                                        disabled={clientGeoLocation[0] ? clientGeoLocation[0].status !== 1 : false} onPress={() => {
                    
                        ClientsData.SaveClientVisit(props.clientRecID);

                }}>
                    <Text style={styles.mainButtonText}>Check In</Text>
                </TouchableOpacity>

                <TouchableOpacity style={styles.mainButton} onPress={() => {
                    props.setGeoLocationModalVisible(false);
                }}>
                    <Text style={styles.mainButtonText}>Ok</Text>
                </TouchableOpacity>

            </View>

        </View>

    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: Colors.bgColor,
        width: '100%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'flex-start',
    },
    headerContainer: {
        backgroundColor: 'gainsboro',
        height: 80,
        width: '100%',
        flexDirection: 'row'
    },
    headerTextContainer: {
        //backgroundColor: 'red',
        alignItems: 'center',
        justifyContent: 'center',
        paddingLeft: 14,
        paddingRight: 14,
        height: '100%',
        flex: 1
    },
    headerImageContainer: {
        //backgroundColor: 'yellow',
        width: 100,
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    headerImage: {
        width: '75%',
        height: '75%',
        overflow: 'hidden',
        resizeMode: 'contain',
    },
    clientNameText: {
        color: 'black',
        width: '100%',
        //fontWeight: 'bold',
        fontSize: FontSize(28),
    },
    clientICAText: {
        fontSize: FontSize(16),
        color: 'red',
        fontStyle: 'italic',
        paddingBottom: 5,
        width: '100%',
    },
    detailsContainer: {
        width: '100%',
        flex: 1,
        backgroundColor: 'white',
        alignItems: 'center',
        justifyContent: 'space-around',
    },
    detailsContainerSplit: {
        //backgroundColor: 'red',
        width: '100%',
        alignItems: 'flex-start',
        padding : 10,
        // borderWidth: 2,
        // borderColor:'red'
    },
    detailsContainerSplitTitleText: {
        fontSize: FontSize(16),
        color: Colors.brandBlue,
        fontWeight: 'bold',
        paddingBottom: 5,
    },
    detailsContainerSplitLLCaption: {
        fontSize: FontSize(16),
        color: Colors.brandGreen,
    },
    detailsContainerSplitLLData: {
        fontSize: FontSize(16),
        color: 'black',
    },
    detailsContainerSplitSlider : {
        width: '100%',
        paddingTop: 12
    },
    footerContainer: {
        backgroundColor: 'gainsboro',
        height: 60,
        width: '100%',
        alignItems: 'center',
        justifyContent: 'space-around',
        flexDirection: 'row',
    },
    mainButton: {
        backgroundColor: Colors.brandBlue,
        width: 70,
        height: 40,
        alignItems: 'center',
        justifyContent: 'center',
        borderRadius: 15
    },
    mainButtonText: {
        fontSize: FontSize(15),
        color: 'white',
    },
    //Common
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
        height: 85,
        alignItems: 'center',
        justifyContent: 'center',
    },
    geoLocationDetailsButtonImage: {
        width: '75%',
        height: '75%',
        overflow: 'hidden',
        resizeMode: 'contain',
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

export default ClientGeoLocation;
