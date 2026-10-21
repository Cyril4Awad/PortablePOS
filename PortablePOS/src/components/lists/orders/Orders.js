import React from 'react';
import { useState, useEffect } from 'react';
import { View, Text, ScrollView, StyleSheet, Button, FlatList, TextInput, Modal } from 'react-native';
import Colors from '../../../variables/Colors';
import ModalTitle from '../../ModalTitle';

import LoadingData from '../../LoadingData';
import ServerError from '../../ServerError';

import Order from './Order';
import OrderDetails from './OrderDetails';
import Sizing from '../../../variables/Sizing';
import OrdersData from '../../../data/OrdersData';
import { FontSize } from '../../../variables/ResponsiveFont';

const Orders = props => {

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const [ordersData, setOrdersData] = useState([]);
  const [selectedOrder, setSelectedOrder] = useState(null);
  const [orderDetailsVisible, setOrderDetailsVisible] = useState(false);

  useEffect(() => {

    OrdersData.GetOrders(
      props.isReturn,
      { setOrdersData },
      { setIsLoading },
      { setError }
    );

    return(() => {
      //Cleanup
      setOrdersData([]);
  })

  }, []);

  //While Loading Or Searching Data
  if (isLoading) {
    return (
      <LoadingData />
    );
  };

  //In case of error
  if (error) {
    return (
      <ServerError errorMessage={error.toString()} setError={ setError } />
    );
  };

  const OrderItem = orderData => {
    return (
      <Order
        orderNumber={orderData.item.orderNumber}
        orderDate={orderData.item.orderDate}
        orderClientCode={orderData.item.orderClientCode}
        orderClientName={orderData.item.orderClientName}
        orderHeaderRemark={orderData.item.orderHeaderRemark}
        orderFooterRemark={orderData.item.orderFooterRemark}
        orderDiscrepancy={orderData.item.orderDiscrepancy}
        isReturn={props.isReturn}
        onDetailsPress={() => {
          setSelectedOrder(orderData.item);
          setOrderDetailsVisible(true);
        }}
      />
    );
  };

  return (
    <View style={styles.container}>

      <ModalTitle
        title={props.isReturn? 'Return Orders List' : 'Orders List'}
        onCloseButtonPress={props.onCloseButtonPress}
        clearData={() => {
          setOrdersData([]);
          setSelectedOrder(null);
          setOrderDetailsVisible(false);
        }}
      />

      <Modal visible={orderDetailsVisible}>
        {selectedOrder &&
          <OrderDetails
            orderNumber={selectedOrder.orderNumber}
            orderDate={selectedOrder.orderDate}
            orderClientCode={selectedOrder.orderClientCode}
            orderClientName={selectedOrder.orderClientName}
            orderHeaderRemark={selectedOrder.orderHeaderRemark}
            orderTotal={selectedOrder.orderTotal}
            orderTotalVAT={selectedOrder.orderTotalVAT}
            orderCurrency={selectedOrder.orderCurrency}
            orderDiscrepancy={selectedOrder.orderDiscrepancy}
            isReturn={props.isReturn}
            onCloseButtonPress={() => {
              setOrderDetailsVisible(false);
              setSelectedOrder(null);
            }}
          />
        }
      </Modal>

      <FlatList
        keyExtractor={(item, index) => item.orderNumber.toString()}
        data={ordersData}
        renderItem={OrderItem}
        numColumns={1}
        style={{ width: '100%', backgroundColor: Colors.bgColor }}
        contentContainerStyle={{ padding: 8 }}
        refreshing={false}
        onRefresh={() => {
          OrdersData.GetOrders(
            props.isReturn,
            { setOrdersData },
            { setIsLoading },
            { setError }
          )
        }
        }
      />

    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    backgroundColor: Colors.brandBlue,
    //backgroundColor: 'yellow',
    //padding: 10,
    //width: '100%',
    height: '100%',
    //alignItems: 'center',
    //justifyContent: 'center',
  },
});

export default Orders;
