import React from 'react';
import { View, StyleSheet, Text, TouchableOpacity } from 'react-native';
import Colors from '../../../variables/Colors';
import { FontSize } from '../../../variables/ResponsiveFont';

const Order = props => {

    const accentColor = props.isReturn ? '#E53935' : Colors.brandBlue;

    return (

        <View style={[styles.container, { borderLeftColor: accentColor }]} >

            <View style={styles.orderContainer}>

                <View style={styles.orderHeaderContainer}>
                    <Text style={[styles.orderNumberText, { color: accentColor }]}>
                        Order #{props.orderNumber}
                    </Text>
                    <View style={styles.orderHeaderRightContainer}>
                        {props.isReturn && (props.orderDiscrepancy === true || props.orderDiscrepancy === 1) &&
                            <View style={styles.discrepancyFlagContainer}>
                                <Text style={styles.discrepancyFlagText}>Discrepancy</Text>
                            </View>
                        }
                        <Text style={styles.orderDateText}>{props.orderDate}</Text>
                    </View>
                </View>

                <View style={styles.orderClientContainer}>
                    <Text style={styles.orderClientText} numberOfLines={1}>
                        {props.orderClientCode} {props.orderClientName}
                    </Text>
                </View>

                {props.orderHeaderRemark && props.orderHeaderRemark.trim() !== '' &&
                    <Text style={styles.orderRemarkText} numberOfLines={1}>{props.orderHeaderRemark}</Text>
                }

            </View>

            <TouchableOpacity
                style={[styles.detailsButton, { borderColor: accentColor }]}
                activeOpacity={0.7}
                onPress={props.onDetailsPress}>
                <Text style={[styles.detailsButtonIcon, { color: accentColor }]}>›</Text>
            </TouchableOpacity>

        </View>

    );
};

const styles = StyleSheet.create({
    container: {
        backgroundColor: 'white',
        width: '100%',
        minHeight: 72,
        marginBottom: 8,
        borderRadius: 10,
        borderLeftWidth: 4,
        flexDirection: 'row',
        alignItems: 'center',
        paddingVertical: 8,
        paddingLeft: 10,
        paddingRight: 8,
        elevation: 2,
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 1 },
        shadowOpacity: 0.12,
        shadowRadius: 2,
    },
    orderContainer: {
        flex: 1,
        paddingRight: 8,
    },
    orderHeaderContainer: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
    },
    orderHeaderRightContainer: {
        flexDirection: 'row',
        alignItems: 'center',
    },
    discrepancyFlagContainer: {
        backgroundColor: '#FFEBEE',
        borderRadius: 10,
        paddingHorizontal: 8,
        paddingVertical: 2,
        marginRight: 8,
    },
    discrepancyFlagText: {
        color: '#E53935',
        fontSize: FontSize(11),
        fontWeight: 'bold',
    },
    orderClientContainer: {
        marginTop: 4,
    },
    orderNumberText: {
        fontWeight: 'bold',
        fontSize: FontSize(16),
    },
    orderDateText: {
        color: '#757575',
        fontSize: FontSize(14),
    },
    orderClientText: {
        color: '#424242',
        fontSize: FontSize(15),
    },
    orderRemarkText: {
        color: '#9E9E9E',
        fontStyle: 'italic',
        fontSize: FontSize(13),
        marginTop: 3,
    },
    detailsButton: {
        width: 40,
        height: 40,
        borderRadius: 20,
        borderWidth: 1.5,
        backgroundColor: '#FAFAFA',
        alignItems: 'center',
        justifyContent: 'center',
    },
    detailsButtonIcon: {
        fontSize: FontSize(26),
        fontWeight: '300',
        lineHeight: FontSize(28),
        marginTop: -2,
        marginLeft: 2,
    },
});

export default Order;
