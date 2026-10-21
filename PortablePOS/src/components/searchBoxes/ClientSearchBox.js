import React from 'react';
import { useState, useEffect, useRef } from 'react';
import Sizing from '../../variables/Sizing';
import Colors from '../../variables/Colors';
import { FontSize } from '../../variables/ResponsiveFont';
import ClientGroups from '../lists/clients/ClientGroups';
import Clients from '../lists/clients/Clients';
import { View, StyleSheet, Text, TextInput, Image, TouchableOpacity, Modal } from 'react-native';

const ClientSearchBox = props => {

    const [clientsScreenVisible, setClientsScreenVisible] = useState(false);
    const [clientsSearchVisible, setClientSearchVisible] = useState(false);

    return (
        <View style={styles.container}>

            <Modal visible={clientsScreenVisible}>
                <ClientGroups
                    setSelectedClientRecID={props.setSelectedClientRecID}
                    setSelectedClientCode={props.setSelectedClientCode}
                    setSelectedClientName={props.setSelectedClientName}
                    setSelectedClientPriceCat={props.setSelectedClientPriceCat}
                    setClientsScreenVisible={setClientsScreenVisible}
                />
            </Modal>

            <Modal visible={clientsSearchVisible}>
                <Clients
                    clientGroupType=''
                    clientGroupCode=''
                    clientGroupDesc='Search...'
                    searchString={props.selectedClientName}
                    setSelectedClientRecID={props.setSelectedClientRecID}
                    setSelectedClientCode={props.setSelectedClientCode}
                    setSelectedClientName={props.setSelectedClientName}
                    setSelectedClientPriceCat={props.setSelectedClientPriceCat}
                    setClientsScreenVisible={setClientSearchVisible}
                    onlyHavingVisits={false}
                    visitDate={null}
                    onCloseButtonPress={() => { setClientSearchVisible(false) }}
                />
            </Modal>

            <View style={styles.infoContainer}>

                <View style={styles.titleContainer}>
                    <Text style={styles.titleText}>Client</Text>
                </View>

                <View style={styles.dataContainer}>

                    <TextInput
                        value={props.selectedClientName}
                        style={styles.dataText}
                        onChangeText={(text) => {
                            props.setSelectedClientName(text);
                            props.setSelectedClientRecID('')
                        }}
                        editable={!props.disabled}
                        onSubmitEditing={() => {
                            
                            if (props.selectedClientName.trim().length > 2) {
                                setClientSearchVisible(true)
                            } 
                            else {
                                if (props.selectedClientName.trim() !== '') {
                                    alert('You must write at least 3 chars');
                                }
                                props.setSelectedClientName('');
                            }

                        }}
                    />

                </View>

            </View>

            <TouchableOpacity style={styles.buttonContainer} disabled={props.disabled} onPress={() => { setClientsScreenVisible(true) }}>
                <Image
                    source={require('./../../assets/icons/clients.png')}
                    style={styles.buttonImage}
                    resizeMode="contain"
                />
            </TouchableOpacity>

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        //backgroundColor: 'red',
        height: 50,
        flexDirection: 'row',
        borderLeftWidth: 2,
        borderBottomWidth: 2,
        borderColor: 'silver',
        marginBottom: 10,
    },
    infoContainer: {
        width: '85%',
    },
    titleContainer: {
        //backgroundColor: 'green',
        alignItems: 'flex-start',
        justifyContent: 'center',
        width: '100%',
        height: 15,
        paddingLeft: 8,
        paddingRight: 5,
    },
    titleText: {
        fontSize: FontSize(15),
        fontWeight: 'bold',
        color: Colors.brandGreen,
    },
    dataContainer: {
        //backgroundColor: 'purple',
        alignItems: 'flex-start',
        justifyContent: 'center',
        width: '100%',
        height: 35,
        paddingLeft: 5,
        paddingRight: 5,
    },
    dataText: {
        color: Colors.brandBlue,
        //backgroundColor: 'red',
        fontSize: FontSize(15),
        width: '100%',
        height: '100%',
    },
    buttonContainer: {
        //backgroundColor: 'yellow',
        alignItems: 'flex-end',
        justifyContent: 'center',
        height: '100%',
        width: '15%',
    },
    buttonImage: {
        //backgroundColor: 'red',
        width: 45,
        height: 35,
        overflow: 'hidden',
        resizeMode: 'contain',
    },
});

export default ClientSearchBox;
