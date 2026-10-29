import {Dimensions} from 'react-native';
import { isTablet } from 'react-native-device-info';

//For Android the window dimension will exclude the size used by the status bar (if not translucent) and bottom navigation bar

const lcWindowsWidth = Dimensions.get('window').width;
const lcWindowsHeight = Dimensions.get('window').height;
const lcIsTablet = isTablet();//(Dimensions.get('window').width > 780);

// Portable scanners : the short screens (Android 11 scanners, height < 510) and the narrow ones (width < 400, ex Newland MT67 : 360 x 577)
// A narrow screen gets the compact layout whatever its height, so the taller scanners look like the short ones
const lcIsPortableScanner = !lcIsTablet && ((lcWindowsHeight < 510 && lcWindowsWidth <= 780) || lcWindowsWidth < 400);

// Height the font sizes of the scanners are computed on : the taller scanners keep the text size of a 510 high screen
const lcScannerFontHeight = Math.min(lcWindowsHeight, 510);

export default {
    windowsWidth : lcWindowsWidth,
    windowsHeight : lcWindowsHeight,
    isTablet : lcIsTablet,
    isPortableScanner : lcIsPortableScanner,
    scannerFontHeight : lcScannerFontHeight,
};
