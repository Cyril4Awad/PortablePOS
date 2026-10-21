import React from 'react';
import { useState, useEffect } from 'react';
import { View, Text, ScrollView, StyleSheet, Button, FlatList } from 'react-native';
import Colors from '../variables/Colors';
import SalesOrders from '../components/lists/SalesOrders/SalesOrders';

const SalesOrdersScreen = props => {
  return (
      <SalesOrders />
    );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  }
});

export default SalesOrdersScreen;
