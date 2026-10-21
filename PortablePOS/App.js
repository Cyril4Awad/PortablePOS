import React from 'react';
import Colors from './src/variables/Colors';
import Sizing from './src/variables/Sizing';
import {
  SafeAreaView,
  StatusBar,
  StyleSheet,
  useColorScheme,
  Platform, 
  View, 
  Text
} from 'react-native';
import AppNavigator from './src/navigator/AppNavigator';
import { useState, useEffect } from 'react';;
import LoginScreen from './src/screens/LoginScreen';

const App = () => {

  const [isLoggedIn, setIsLoggedIn] = useState(false);

  const isDarkMode = useColorScheme() === 'dark';

  const backgroundStyle = {
    backgroundColor: isDarkMode ? Colors.darker : Colors.lighter,
  };

  if (isLoggedIn) {

    return (

      <SafeAreaView style={styles.container}>

        {/* <StatusBar
          barStyle={isDarkMode ? 'light-content' : 'dark-content'}
          backgroundColor={backgroundStyle.backgroundColor}
        /> */}

        <AppNavigator setIsLoggedIn={setIsLoggedIn}/>

      </SafeAreaView>

    );

  }
  else {

    return (

      <SafeAreaView style={styles.container}>

        {/* <StatusBar
          barStyle={isDarkMode ? 'light-content' : 'dark-content'}
          backgroundColor={backgroundStyle.backgroundColor}
        /> */}

        <LoginScreen setIsLoggedIn={setIsLoggedIn}/>

      </SafeAreaView>

    );

  }

};

const styles = StyleSheet.create({
  container: {
    height: '100%'
  },
});

export default App;
