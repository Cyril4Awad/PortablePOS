import React from 'react';
import { Header, HeaderBackButton, getHeaderTitle } from '@react-navigation/elements';
import Sizing from './Sizing';
import Colors from './Colors';
import { FontSize } from './ResponsiveFont';

const PORTABLE_HEADER_HEIGHT = 36;

export const portableScannerHeaderStyle = Sizing.isPortableScanner
    ? { height: PORTABLE_HEADER_HEIGHT }
    : {};

export const portableScannerHeaderExtraOptions = Sizing.isPortableScanner
    ? {
        headerStatusBarHeight: 0,
        headerTitleStyle: {
            fontSize: FontSize(14),
        },
    }
    : {};

export const baseHeaderScreenOptions = {
    headerStyle: {
        backgroundColor: Colors.brandBlue,
        ...portableScannerHeaderStyle,
    },
    headerTintColor: Colors.navTitleColor,
    ...portableScannerHeaderExtraOptions,
};

export function CompactStackHeader({ options, route, back, navigation }) {
    return (
        <Header
            {...options}
            title={getHeaderTitle(options, route.name)}
            back={back}
            navigation={navigation}
            headerStyle={{
                backgroundColor: Colors.brandBlue,
                height: PORTABLE_HEADER_HEIGHT,
            }}
            headerTitleStyle={{
                fontSize: FontSize(14),
            }}
            headerStatusBarHeight={0}
            headerTintColor={Colors.navTitleColor}
        />
    );
}

// Back arrow at the left of the title (the compact header of the portable scanners has none by itself),
// so the user goes back one screen without pressing the back key of the device many times
// Used as : options={({ navigation }) => ({ title: '...', ...BackArrowOptions(navigation) })}
export const BackArrowOptions = (navigation) => ({
    headerLeft: (props) => (
        <HeaderBackButton
            {...props}
            labelVisible={false}
            tintColor={Colors.navTitleColor}
            onPress={() => navigation.goBack()}
        />
    ),
});

export const portableScannerStackScreenOptions = Sizing.isPortableScanner
    ? {
        header: (props) => <CompactStackHeader {...props} />,
    }
    : {};
