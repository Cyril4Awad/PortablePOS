import React from 'react';
import { useState, useEffect } from 'react';
import { View, Text, StyleSheet, FlatList, Image } from 'react-native';
import Colors from '../../../variables/Colors';
import ModalTitle from '../../ModalTitle';
import LoadingData from '../../LoadingData';
import ServerError from '../../ServerError';
import OrdersData from '../../../data/OrdersData';
import { FontSize } from '../../../variables/ResponsiveFont';

const SummaryRow = ({ label, value, valueStyle }) => (
  <View style={styles.summaryRow}>
    <Text style={styles.summaryLabel}>{label}</Text>
    <Text style={[styles.summaryValue, valueStyle]} numberOfLines={2}>{value}</Text>
  </View>
);

const OrderDetails = props => {

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [orderLines, setOrderLines] = useState([]);

  const accentColor = props.isReturn ? '#E53935' : Colors.brandBlue;
  const hasRemark = props.orderHeaderRemark && props.orderHeaderRemark.trim() !== '';
  const hasDiscrepancy = props.isReturn && (props.orderDiscrepancy === true || props.orderDiscrepancy === 1);

  useEffect(() => {

    OrdersData.GetOrderDetails(
      props.orderNumber,
      props.isReturn,
      { setOrderLines },
      { setIsLoading },
      { setError }
    );

    return () => {
      setOrderLines([]);
    };

  }, [props.orderNumber]);

  const modalTitle = (props.isReturn ? 'Return ' : '') + 'Order ' + props.orderNumber;

  if (isLoading) {
    return (
      <View style={styles.container}>
        <ModalTitle
          title={modalTitle}
          onCloseButtonPress={props.onCloseButtonPress}
          clearData={() => setOrderLines([])}
        />
        <LoadingData />
      </View>
    );
  }

  if (error) {
    return (
      <View style={styles.container}>
        <ModalTitle
          title={modalTitle}
          onCloseButtonPress={props.onCloseButtonPress}
          clearData={() => setOrderLines([])}
        />
        <ServerError errorMessage={error.toString()} setError={setError} />
      </View>
    );
  }

  const OrderLineItem = lineData => {
    const line = lineData.item;

    return (
      <View style={styles.lineContainer}>
        <Text style={styles.lineDescriptionText} numberOfLines={2}>
          {line.lineItemReference} - {line.lineItemDescription}
        </Text>
        <View style={styles.lineInfoContainer}>
          <Text style={styles.lineInfoText}>{Intl.NumberFormat('ar-de').format(line.lineItemQty)}</Text>
          <Text style={styles.lineInfoText}>{Intl.NumberFormat('ar-de').format(line.lineItemPrice)}</Text>
          <Text style={styles.lineInfoText}>{Intl.NumberFormat('ar-de').format(line.lineItemDiscount)}</Text>
          <Text style={styles.lineInfoText}>
            {line.lineItemTotal === 0 ? 'Free' : Intl.NumberFormat('ar-de').format(line.lineItemTotal)}
          </Text>
        </View>
        {line.lineItemRemark !== '' &&
          <Text style={styles.lineRemarkText}>{line.lineItemRemark}</Text>
        }
      </View>
    );
  };

  return (
    <View style={styles.container}>

      <ModalTitle
        title={modalTitle}
        onCloseButtonPress={props.onCloseButtonPress}
        clearData={() => setOrderLines([])}
      />

      <View style={styles.summaryContainer}>

        <SummaryRow
          label="Client :"
          value={`${props.orderClientCode} ${props.orderClientName}`}
        />

        <SummaryRow
          label="Date :"
          value={props.orderDate}
        />

        <SummaryRow
          label="Order Currency :"
          value={props.orderCurrency}
          valueStyle={{ color: Colors.brandGreen, fontWeight: 'bold' }}
        />

        {hasRemark &&
          <SummaryRow
            label="Remark :"
            value={props.orderHeaderRemark}
            valueStyle={styles.summaryRemarkValue}
          />
        }

        {hasDiscrepancy &&
          <View style={styles.discrepancyRow}>
            <Text style={styles.summaryLabel}>Discrepancy :</Text>
            <View style={styles.discrepancyBadge}>
              <Image
                source={require('../../../assets/icons/checked.png')}
                style={styles.discrepancyIcon}
                resizeMode='contain'
              />
              <Text style={styles.discrepancyBadgeText}>Yes</Text>
            </View>
          </View>
        }

        <View style={styles.totalsRow}>
          <Text style={[styles.totalText, { color: accentColor }]}>
            Total: {Intl.NumberFormat('ar-de').format(props.orderTotal)} {props.orderCurrency}
          </Text>
          <Text style={[styles.totalText, { color: accentColor }]}>
            VAT: {Intl.NumberFormat('ar-de').format(props.orderTotalVAT)}
          </Text>
        </View>

      </View>

      <View style={styles.linesHeaderContainer}>
        <Text style={styles.linesHeaderDescription}>Item</Text>
        <View style={styles.linesHeaderInfo}>
          <Text style={styles.linesHeaderInfoText}>Qty</Text>
          <Text style={styles.linesHeaderInfoText}>Price</Text>
          <Text style={styles.linesHeaderInfoText}>Disc</Text>
          <Text style={styles.linesHeaderInfoText}>Total</Text>
        </View>
      </View>

      <FlatList
        keyExtractor={(item, index) => index.toString()}
        data={orderLines}
        renderItem={OrderLineItem}
        numColumns={1}
        style={styles.linesList}
        contentContainerStyle={orderLines.length === 0 ? styles.emptyListContent : undefined}
        ListEmptyComponent={
          <Text style={styles.emptyListText}>No items found for this order.</Text>
        }
      />

    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    backgroundColor: Colors.bgColor,
    height: '100%',
    width: '100%',
  },
  summaryContainer: {
    padding: 12,
    borderBottomWidth: 1,
    borderColor: '#E0E0E0',
    backgroundColor: 'white',
  },
  summaryRow: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    marginBottom: 8,
  },
  summaryLabel: {
    width: 120,
    fontSize: FontSize(15),
    fontWeight: 'bold',
    color: Colors.brandBlue,
  },
  summaryValue: {
    flex: 1,
    fontSize: FontSize(15),
    color: '#212121',
  },
  summaryRemarkValue: {
    fontStyle: 'italic',
    color: '#616161',
  },
  discrepancyRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 8,
  },
  discrepancyBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#FFEBEE',
    borderRadius: 12,
    paddingHorizontal: 10,
    paddingVertical: 4,
  },
  discrepancyIcon: {
    width: 14,
    height: 14,
    marginRight: 4,
  },
  discrepancyBadgeText: {
    color: '#E53935',
    fontSize: FontSize(14),
    fontWeight: 'bold',
  },
  totalsRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginTop: 4,
    paddingTop: 8,
    borderTopWidth: 1,
    borderTopColor: '#EEEEEE',
  },
  totalText: {
    fontSize: FontSize(15),
    fontWeight: 'bold',
  },
  linesHeaderContainer: {
    flexDirection: 'row',
    backgroundColor: 'rgba(0,0,0,0.3)',
    height: 35,
    alignItems: 'center',
    paddingHorizontal: 5,
  },
  linesHeaderDescription: {
    flex: 1,
    color: 'white',
    fontWeight: 'bold',
    fontSize: FontSize(15),
  },
  linesHeaderInfo: {
    width: '45%',
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  linesHeaderInfoText: {
    width: '25%',
    textAlign: 'center',
    color: 'white',
    fontWeight: 'bold',
    fontSize: FontSize(14),
  },
  linesList: {
    flex: 1,
    width: '100%',
  },
  emptyListContent: {
    flexGrow: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  emptyListText: {
    color: 'gray',
    fontSize: FontSize(16),
  },
  lineContainer: {
    backgroundColor: 'white',
    borderBottomWidth: 1,
    borderColor: 'silver',
    padding: 8,
  },
  lineDescriptionText: {
    fontSize: FontSize(15),
    color: Colors.brandBlue,
    marginBottom: 4,
  },
  lineInfoContainer: {
    flexDirection: 'row',
    width: '45%',
    alignSelf: 'flex-end',
  },
  lineInfoText: {
    flex: 1,
    textAlign: 'center',
    fontSize: FontSize(15),
    color: 'black',
  },
  lineRemarkText: {
    fontSize: FontSize(14),
    color: 'gray',
    fontStyle: 'italic',
    marginTop: 4,
  },
});

export default OrderDetails;
