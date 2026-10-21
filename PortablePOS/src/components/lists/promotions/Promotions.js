import React from 'react';
import { useState, useEffect } from 'react';
import { View, Text, ScrollView, StyleSheet, Button, FlatList, TextInput, Modal } from 'react-native';
import Colors from '../../../variables/Colors';
import ModalTitle from '../../ModalTitle';

import LoadingData from '../../LoadingData';
import ServerError from '../../ServerError';

import Promotion from './Promotion';
import Sizing from '../../../variables/Sizing';
import StockData from '../../../data/StockData';
import { FontSize } from '../../../variables/ResponsiveFont';

const Promotions = props => {

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const [promotions, setPromotions] = useState([]);

  useEffect(() => {

    StockData.GetPromotions(
      { setPromotions },
      { setIsLoading },
      { setError }
    );

    return(() => {
      //Cleanup
      setPromotions([]);
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

  const PromotionItem = promotionData => {
    return (
      <Promotion
        promDescription={promotionData.item.promDescription}
        promType={promotionData.item.promType}
        promBuyGet={promotionData.item.promBuyGet}
        promItemCategoryIdentificationType={promotionData.item.promItemCategoryIdentificationType}
        promItemCategoryIdentification={promotionData.item.promItemCategoryIdentification}
        promGetQty={promotionData.item.promGetQty}
        promGetDisc={promotionData.item.promGetDisc}
      />
    );
  };

  return (
    <View style={styles.container}>

      <ModalTitle
        title={'Promotions List'}
        onCloseButtonPress={props.onCloseButtonPress}
        clearData={() => {
          setPromotions([]);
        }}
      />

      <FlatList
        keyExtractor={(item, index) => item.promRecKey.toString()}
        data={promotions}
        renderItem={PromotionItem}
        numColumns={1}
        style={{ width: '100%', backgroundColor: Colors.bgColor }}
        contentContainerStyle={{ justifyContent: 'center' }}
        refreshing={false}
        onRefresh={() => {
            StockData.GetPromotions(
                { setPromotions },
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

export default Promotions;
