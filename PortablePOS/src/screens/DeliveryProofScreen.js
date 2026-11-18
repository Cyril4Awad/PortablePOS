import React, { useRef, useState, useEffect } from 'react';
import { View, Text, StyleSheet, Modal, Image, TouchableOpacity, TextInput } from 'react-native';
import { RNCamera } from 'react-native-camera';
import Colors from '../variables/Colors';
import Sizing from '../variables/Sizing';
import { FontSize } from '../variables/ResponsiveFont';
import ScreenHeader from '../components/ScreenHeader';
import MainButton from '../components/controls/MainButton';
import ClientSearchBox from '../components/searchBoxes/ClientSearchBox';
import MessageBox from '../components/MessageBox';
import ClientsData from '../data/ClientsData';
import GeoLocationInfo from '../variables/GeoLocationInfo';

// Saved in DP_RadiusInMeters : the radius is no longer chosen on the screen
const DEFAULT_RADIUS_IN_METERS = 50;

// Clients > Delivery Proof (C4) : the client delivered, a remark and an optional photo, saved online in tbl_CPP_DeliveryProof
// with the geo location of the device. The location is not shown : it is read in the background (on open and when a client
// is picked) and read again on Save when it is missing. It is read even when UseLocation (AppConfig) is false : it is mandatory
// Mandatory : the client, the remark and the geo location
// The screen does not scroll : the photo takes the space left and shrinks to fit, so the buttons always stay on the screen
const DeliveryProofScreen = props => {

    const cameraRef = useRef(null);

    const [selectedClientRecID, setSelectedClientRecID] = useState('');
    const [selectedClientCode, setSelectedClientCode] = useState('');
    const [selectedClientName, setSelectedClientName] = useState('');
    const [selectedClientPriceCat, setSelectedClientPriceCat] = useState('');

    const [remarks, setRemarks] = useState('');
    const [remarkModalVisible, setRemarkModalVisible] = useState(false);

    // { uri, base64 } of the photo taken, null when none
    const [photo, setPhoto] = useState(null);
    const [cameraVisible, setCameraVisible] = useState(false);
    const [takingPhoto, setTakingPhoto] = useState(false);

    // The last position read (null until one is read), kept in a ref so Save uses the latest one
    const location = useRef(null);

    const [saving, setSaving] = useState(false);

    // A fresh position (not the one kept by GeoLocationInfo) : the delivery is where the device is now
    // Resolves the position or null, never rejects
    const ReadLocation = () =>
        GeoLocationInfo.GetLocation(true, true)
            .then(loc => {
                if (loc) location.current = loc;
                return loc;
            })
            .catch(() => null);

    useEffect(() => {
        ReadLocation();
    }, []);

    // The client is picked where the delivery is done : the position is read again
    useEffect(() => {
        if (selectedClientRecID !== '' && selectedClientRecID !== null) ReadLocation();
    }, [selectedClientRecID]);

    const ClearData = () => {
        setSelectedClientRecID('');
        setSelectedClientCode('');
        setSelectedClientName('');
        setSelectedClientPriceCat('');
        setRemarks('');
        setPhoto(null);
    };

    const TakePhoto = async () => {
        if (!cameraRef.current || takingPhoto) return;
        try {
            setTakingPhoto(true);
            // Compressed and resized on the device : the photo is saved in the table
            const data = await cameraRef.current.takePictureAsync({
                quality: 0.5,
                width: 1024,
                base64: true,
                fixOrientation: true,
            });
            setPhoto({ uri: data.uri, base64: data.base64 });
            setCameraVisible(false);
        } catch (err) {
            MessageBox.alert('Delivery Proof', 'Cannot take the photo, please try again');
        } finally {
            setTakingPhoto(false);
        }
    };

    const Save = async () => {
        if (saving) return;

        if (selectedClientRecID === '' || selectedClientRecID === null) {
            MessageBox.alert('Delivery Proof', 'Client is Mandatory');
            return;
        }
        if (remarks.trim() === '') {
            MessageBox.alert('Delivery Proof', 'Remark is Mandatory');
            return;
        }

        setSaving(true);
        try {
            // Not read yet (GPS slow or off when the screen opened) : one more try before giving up
            const loc = location.current ?? await ReadLocation();
            if (!loc) {
                MessageBox.alert('Delivery Proof', 'The location of the device could not be read. Turn on Location (GPS) and allow it for the app, then press Save again.');
                return;
            }

            const result = await ClientsData.SaveDeliveryProof(
                selectedClientRecID,
                remarks.trim(),
                photo ? photo.base64 : '',
                loc.coords.latitude,
                loc.coords.longitude,
                loc.coords.accuracy,
                DEFAULT_RADIUS_IN_METERS
            );

            if (result === 'Success') {
                MessageBox.alert('Delivery Proof', 'Delivery Proof Saved');
                ClearData();
            }
            else {
                MessageBox.alert('Delivery Proof', result && result.startsWith('Error') ? 'Cannot Save The Delivery Proof, Please Try Again Later' : result);
            }
        } catch (err) {
            MessageBox.alert('Delivery Proof', 'Cannot reach the server, please try again');
        } finally {
            setSaving(false);
        }
    };

    return (
        <View style={styles.container}>

            {/* Camera */}
            <Modal visible={cameraVisible} onRequestClose={() => setCameraVisible(false)}>

                <View style={styles.cameraContainer}>
                    <RNCamera
                        ref={cameraRef}
                        style={styles.camera}
                        type={RNCamera.Constants.Type.back}
                        flashMode={RNCamera.Constants.FlashMode.auto}
                        captureAudio={false}
                        androidCameraPermissionOptions={{
                            title: 'Camera',
                            message: 'Can we use your camera to take the delivery photo?',
                            buttonPositive: 'OK',
                            buttonNegative: 'Cancel',
                        }}
                    />
                </View>

                <View style={styles.cameraFooter}>
                    <TouchableOpacity style={styles.cameraButton} onPress={() => setCameraVisible(false)}>
                        <Text style={styles.cameraButtonText}>CANCEL</Text>
                    </TouchableOpacity>
                    <TouchableOpacity style={[styles.cameraButton, styles.cameraCaptureButton]} disabled={takingPhoto} onPress={TakePhoto}>
                        <Text style={styles.cameraButtonText}>{takingPhoto ? 'TAKING...' : 'TAKE PHOTO'}</Text>
                    </TouchableOpacity>
                </View>

            </Modal>

            {/* Remark */}
            <Modal
                visible={remarkModalVisible}
                transparent={true}
                presentationStyle='overFullScreen'
                onRequestClose={() => setRemarkModalVisible(false)}>
                <View style={styles.modalContainer}>
                    <View style={styles.remarkModalCard}>
                        <Text style={styles.remarkModalTitle}>Remark</Text>
                        <TextInput
                            style={styles.remarkModalInput}
                            value={remarks}
                            onChangeText={setRemarks}
                            maxLength={4000}
                            multiline={true}
                            placeholder='Enter the delivery remark...'
                            placeholderTextColor='silver'
                            autoFocus={true}
                        />
                        <View style={styles.remarkModalButtons}>
                            <TouchableOpacity style={styles.remarkModalClearButton} onPress={() => setRemarks('')}>
                                <Text style={styles.remarkModalClearButtonText}>Clear</Text>
                            </TouchableOpacity>
                            <TouchableOpacity style={styles.remarkModalDoneButton} onPress={() => setRemarkModalVisible(false)}>
                                <Text style={styles.remarkModalDoneButtonText}>Done</Text>
                            </TouchableOpacity>
                        </View>
                    </View>
                </View>
            </Modal>

            {/* The header only holds a description : left out on the small portable scanners */}
            {!Sizing.isPortableScanner ?
                <ScreenHeader
                    description='Proof of delivery to a client'
                    imageSource={require('../assets/icons/deliveryProof.png')}
                /> :
                <></>}

            <View style={styles.content}>

                <ClientSearchBox
                    selectedClientName={selectedClientName}
                    setSelectedClientRecID={setSelectedClientRecID}
                    setSelectedClientCode={setSelectedClientCode}
                    setSelectedClientName={setSelectedClientName}
                    setSelectedClientPriceCat={setSelectedClientPriceCat}
                    disabled={saving}
                />

                {/* Remark : opens the remark box to type in it (fixed height : a long remark is cut, not scrolled) */}
                <TouchableOpacity style={styles.remarkField} disabled={saving} onPress={() => setRemarkModalVisible(true)}>
                    <Text style={styles.sectionTitleText}>Remark</Text>
                    <Text style={remarks.trim() === '' ? styles.placeholderText : styles.dataText} numberOfLines={2}>
                        {remarks.trim() === '' ? 'Tap to write the remark' : remarks}
                    </Text>
                </TouchableOpacity>

                {/* Photo : takes the space left, the picture shrinks to fit it */}
                <View style={styles.photoSection}>

                    <View style={styles.photoTitleRow}>
                        <Text style={styles.sectionTitleText}>Photo</Text>
                        {photo ?
                            <View style={styles.photoButtons}>
                                <TouchableOpacity style={styles.smallButton} disabled={saving} onPress={() => setCameraVisible(true)}>
                                    <Text style={styles.smallButtonText}>Retake</Text>
                                </TouchableOpacity>
                                <TouchableOpacity style={[styles.smallButton, styles.smallButtonRemove]} disabled={saving} onPress={() => setPhoto(null)}>
                                    <Text style={styles.smallButtonText}>Remove</Text>
                                </TouchableOpacity>
                            </View>
                            : null}
                    </View>

                    {photo ?
                        <Image source={{ uri: photo.uri }} style={styles.photoPreview} resizeMode='contain' />
                        :
                        <TouchableOpacity style={styles.photoPlaceholder} disabled={saving} onPress={() => setCameraVisible(true)}>
                            <Image source={require('../assets/icons/add.png')} style={styles.photoPlaceholderImage} resizeMode='contain' />
                            <Text style={styles.placeholderText}>Tap to take a photo (optional)</Text>
                        </TouchableOpacity>
                    }

                </View>

                <View style={styles.buttonsContainer}>

                    <MainButton title={saving ? 'Saving...' : 'Save'} imageSource={require('../assets/icons/upload.png')} onPress={Save} />

                    <MainButton title='Clear' imageSource={require('../assets/icons/deleteLine.png')} onPress={() => { if (!saving) ClearData(); }} />

                </View>

            </View>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: Colors.bgColor,
    },
    content: {
        flex: 1,
        padding: 10,
    },
    sectionTitleText: {
        fontSize: FontSize(14),
        fontWeight: 'bold',
        color: Colors.brandGreen,
    },
    dataText: {
        color: Colors.brandBlue,
        fontSize: FontSize(15),
    },
    placeholderText: {
        color: 'silver',
        fontSize: FontSize(15),
    },
    remarkField: {
        width: '100%',
        height: 62,
        borderLeftWidth: 2,
        borderBottomWidth: 2,
        borderColor: 'silver',
        paddingLeft: 8,
        paddingRight: 5,
        marginBottom: 10,
    },
    photoSection: {
        flex: 1,
        minHeight: 0,
        borderLeftWidth: 2,
        borderBottomWidth: 2,
        borderColor: 'silver',
        paddingLeft: 8,
        paddingRight: 5,
        paddingBottom: 8,
    },
    photoTitleRow: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        minHeight: 30,
    },
    photoButtons: {
        flexDirection: 'row',
    },
    photoPreview: {
        flex: 1,
        width: '100%',
        backgroundColor: '#eee',
    },
    photoPlaceholder: {
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
        borderWidth: 1,
        borderStyle: 'dashed',
        borderColor: 'silver',
        borderRadius: 8,
    },
    photoPlaceholderImage: {
        width: 40,
        height: 40,
        marginBottom: 6,
    },
    smallButton: {
        backgroundColor: Colors.brandBlue,
        borderRadius: 5,
        paddingVertical: 5,
        paddingHorizontal: 12,
        marginLeft: 8,
    },
    smallButtonRemove: {
        backgroundColor: 'firebrick',
    },
    smallButtonText: {
        color: 'white',
        fontSize: FontSize(13),
    },
    // Save and Clear next to each other (as the buttons of Free Receiving)
    buttonsContainer: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: Sizing.isTablet ? 'flex-end' : 'space-between',
        width: '100%',
        paddingTop: 10,
    },
    modalContainer: {
        width: '100%',
        height: '100%',
        backgroundColor: 'rgba(0,0,0,0.5)',
        alignItems: 'center',
        justifyContent: 'center',
    },
    remarkModalCard: {
        width: Sizing.windowsWidth * 0.85,
        height: Sizing.windowsHeight * 0.35,
        backgroundColor: 'white',
        borderRadius: 20,
        overflow: 'hidden',
        elevation: 10,
        borderWidth: 5,
        borderColor: Colors.brandBlue,
        padding: 15,
    },
    remarkModalTitle: {
        fontSize: FontSize(18),
        fontWeight: 'bold',
        color: Colors.brandBlue,
        marginBottom: 10,
    },
    remarkModalInput: {
        flex: 1,
        borderWidth: 1,
        borderColor: 'silver',
        borderRadius: 8,
        padding: 10,
        fontSize: FontSize(16),
        color: 'black',
        textAlignVertical: 'top',
    },
    remarkModalButtons: {
        flexDirection: 'row',
        justifyContent: 'flex-end',
        marginTop: 12,
    },
    remarkModalClearButton: {
        paddingVertical: 8,
        paddingHorizontal: 16,
        marginRight: 10,
    },
    remarkModalClearButtonText: {
        color: 'gray',
        fontSize: FontSize(16),
    },
    remarkModalDoneButton: {
        backgroundColor: Colors.brandBlue,
        paddingVertical: 8,
        paddingHorizontal: 20,
        borderRadius: 6,
    },
    remarkModalDoneButtonText: {
        color: 'white',
        fontSize: FontSize(16),
        fontWeight: 'bold',
    },
    cameraContainer: {
        flex: 1,
        backgroundColor: 'black',
    },
    camera: {
        flex: 1,
    },
    cameraFooter: {
        flexDirection: 'row',
        height: 60,
        backgroundColor: Colors.brandBlue,
    },
    cameraButton: {
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
    },
    cameraCaptureButton: {
        backgroundColor: Colors.brandGreen,
    },
    cameraButtonText: {
        color: 'white',
        fontSize: FontSize(16),
        fontWeight: 'bold',
    },
});

export default DeliveryProofScreen;
