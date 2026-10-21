import React, { useState , useEffect } from 'react';
import Sizing from '../../../variables/Sizing';
import Colors from '../../../variables/Colors';
import { View, StyleSheet, Image, Text, FlatList } from 'react-native';
import WebView from 'react-native-webview';
import { FontSize } from '../../../variables/ResponsiveFont';
import ModalTitle from '../../ModalTitle';
import ClientsData from '../../../data/ClientsData';

const ClientStatement = props => {

    const [clientStatementList, setClientStatementList] = useState([]);

    useEffect(() => {

        ClientsData.GetClientStatement(props.clientRecID,
            { setClientStatementList })      

            return(() => {
                //Cleanup
                setClientStatementList([]);
            })

        }
      , []);

    const ClientStatementRecord = stData => {
        return (
            <View style={styles.csrContainer}>

                <View style={styles.csrHeader}>

                    <View style={styles.csrHeaderTransactionLocNum}>
                        <Text style={styles.csrHeaderTransactionLocNumText}>{stData.item.transactionLocationCode}{Sizing.isTablet ? ' - ' + stData.item.transactionLocationName : ''} - {stData.item.transactionNumber}</Text>
                    </View>
                    
                    <View style={styles.csrHeaderTransactionDate}>
                    <Text style={styles.csrHeaderTransactionDateText}>{stData.item.transactionDate}</Text>
                    </View>

                </View>
                
                <View style={styles.csrGridHeader}>

                    <View style={styles.csrGridCell}>
                        <Text style={styles.csrGridHeaderText}>NET</Text>
                    </View>

                    <View style={styles.csrGridCell}>
                        <Text style={styles.csrGridHeaderText}>PAID</Text>
                    </View>

                    <View style={styles.csrGridCell}>
                        <Text style={styles.csrGridHeaderText}>REMAINING</Text>
                    </View>

                    <View style={styles.csrGridCell}>
                        <Text style={styles.csrGridHeaderText}>BALANCE</Text>
                    </View>
                    
                </View>

                <View style={styles.csrGridDetails}>

                    <View style={styles.csrGridCell}>
                        <Text style={styles.csrGridDetailsText}>{Intl.NumberFormat("ar-de").format(stData.item.transactionNet)}</Text>
                    </View>

                    <View style={styles.csrGridCell}>
                        <Text style={styles.csrGridDetailsText}>{Intl.NumberFormat("ar-de").format(stData.item.transactionPaid)}</Text>
                    </View>

                    <View style={styles.csrGridCell}>
                        <Text style={styles.csrGridDetailsText}>{Intl.NumberFormat("ar-de").format(stData.item.transactionRemain)}</Text>
                    </View>

                    <View style={styles.csrGridCell}>
                        <Text style={styles.csrGridDetailsText}>{Intl.NumberFormat("ar-de").format(stData.item.transactionBalance)}</Text>
                    </View>
                    
                </View>

                <View style={[styles.csrRemarkContainer, {height : stData.item.transactionRemarks === '' ? 0 : 30}]}>
                    <Text style={styles.csrRemarkText}>{stData.item.transactionRemarks}</Text>
                </View>

            </View>
        );
    };



    return (
        <View style={styles.container}>

            <ModalTitle
                title={props.clientName}
                onCloseButtonPress={() => { props.setClientStatementModalVisible(false) }}
                clearData={() => {
                    // setClientsData([]);
                    // setClientsDataFiltered([]);
                }}
            />
            
            <View style={styles.titleContainer}>
                <Text style={styles.titleText}>Statement {clientStatementList.length > 0 ? clientStatementList[0].transactionCurrency : ''}</Text>
            </View>

            <FlatList
                    keyExtractor={(item, index) => item.transactionNumber.toString()}
                    data={clientStatementList}
                    renderItem={ClientStatementRecord}
                    numColumns={1}
                    style={{ width: '100%', backgroundColor: Colors.bgColor }}
                    contentContainerStyle={{ alignItems: 'flex-start', justifyContent: 'center' }}
                />

        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        width: '100%',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    titleContainer: {
        backgroundColor: Colors.brandGreen,
        height: 28,
        alignItems: 'center',
        justifyContent: 'center',
        width: '100%',
    },
    titleText: {
        color: Colors.brandBlue,
        fontSize: FontSize(20)
    },
    csrContainer: {
        backgroundColor: Colors.bgColor,
        width: (Sizing.isPortableScanner ? Sizing.windowsWidth * 0.9 : Sizing.windowsWidth * 0.9) - 11 ,
        //borderRadius: 10,
        alignItems: 'flex-start',
        justifyContent: 'center',
        //padding: 5,
        //marginBottom: 5,
        //padding: 2,
    },
    csrHeader:{
        backgroundColor: 'gainsboro',
        width: '100%',
        height: 30,
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        paddingLeft: 7,
        paddingRight: 7
    },
    csrHeaderTransactionLocNum: {
        //backgroundColor: 'yellow',
        width: '50%',
        height: '100%',
        alignItems: 'flex-start',
        justifyContent: 'center',
    },
    csrHeaderTransactionLocNumText: {
        color: Colors.brandBlue,
        fontSize: FontSize(18),
    },
    csrHeaderTransactionDate: {
        //backgroundColor: 'blue',
        width: '50%',
        height: '100%',
        alignItems: 'flex-end',
        justifyContent: 'center',
    },
    csrHeaderTransactionDateText: {
        color: Colors.brandBlue,
        fontSize: FontSize(18),
    },
    csrGridHeader: {
        //backgroundColor: 'red',
        width: '100%',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        paddingTop: 5,
    },
    csrGridDetails: {
        //backgroundColor: 'red',
        width: '100%',
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        paddingBottom: 5,
    },
    csrGridCell: {
        //backgroundColor: 'red',
        width: '25%',
        alignItems: 'center',
        justifyContent: 'center',
    },
    csrGridHeaderText: {
        color: 'dimGray',
        fontSize: FontSize(12),
    },
    csrGridDetailsText: {
        color: 'black',
        fontSize: FontSize(18),
    },
    csrRemarkContainer: {
        width: '100%',
        alignItems: 'flex-start',
        justifyContent: 'center',
        paddingLeft: 5,
        paddingRight: 5,
    },
    csrRemarkText: {
        fontSize: FontSize(15),
        color: Colors.brandGreen
    }
});

export default ClientStatement;
