import React from 'react';
import { useState, useEffect } from 'react';
import { View, Text, ScrollView, StyleSheet, Button, FlatList } from 'react-native';
import ClientGroups from '../components/lists/clients/ClientGroups';

const ClientsScreen = props => {
  return (
    <ClientGroups {...props} />
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  }
});

export default ClientsScreen;
