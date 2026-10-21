//https://blog.logrocket.com/react-native-geolocation-complete-tutorial/
import { PermissionsAndroid } from 'react-native';
import Geolocation from 'react-native-geolocation-service';
import { UseLocation } from './AppConfig';

const requestLocationPermission = async () => {
    try {
      const granted = await PermissionsAndroid.request(
        PermissionsAndroid.PERMISSIONS.ACCESS_FINE_LOCATION,
        {
          title: 'Geolocation',
          message: 'Can we access your location?',
          buttonNeutral: 'Ask Me Later',
          buttonNegative: 'Cancel',
          buttonPositive: 'OK',
        },
      );
      console.log('granted', granted);
      if (granted === 'granted') {
        //console.log('You can use Geolocation');
        return true;
      } else {
        //console.log('You cannot use Geolocation');
        return false;
      }
    } catch (err) {
      return false;
    }
  };

  // const GetLocation = ({ setLocation }) => {
  //   const result = requestLocationPermission();
  //   result.then(res => {
  //     if (res) {
  //       Geolocation.getCurrentPosition(
  //         position => {
  //           setLocation(position);
  //         },
  //         error => {
  //           console.log(error.code, error.message);
  //           setLocation(false);
  //         },
  //         {enableHighAccuracy: true, timeout: 15000, maximumAge: 10000},
  //       );
  //     }
  //   });
  // };

  var savedPosition = false;

  const GetLocation = (force = false) => {
    return new Promise((resolve, reject) => {

      if (!UseLocation) {
        resolve(null);
        return;
      }

      if (!savedPosition || force) {

        console.log('im still not saved')
        const result = requestLocationPermission();
        result.then(res => {
          if (res) {
            Geolocation.getCurrentPosition(
              position => {
                savedPosition = position;
                resolve(position);
              },
              error => {
                console.log(error.code, error.message);
                reject(error);
              },
              { enableHighAccuracy: true, timeout: 15000, maximumAge: 10000 },
            );
          } else {
            reject(new Error('Location permission not granted.'));
          }
        });

      }
      else {
        console.log('im already saved')
        resolve(savedPosition);
      }

    });
  };

export default {
    GetLocation: GetLocation,
    UseLocation: UseLocation,
}