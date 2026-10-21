import React from 'react';
import { useState, useEffect } from 'react';
import { View, Text, ScrollView, StyleSheet, Button, FlatList } from 'react-native';
import Colors from '../variables/Colors';
import ItemProductGroups from '../components/lists/stock/ItemProductGroups';

const StockSearchScreen = props => {
  return (
      <ItemProductGroups />
    );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  }
});

export default StockSearchScreen;
