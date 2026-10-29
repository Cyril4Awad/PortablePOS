import AsyncStorage from '@react-native-async-storage/async-storage';
import { DEFAULT_PAGE_SIZE } from '../variables/PhysicalCountRules';
import { MachineName, MachineNameMessage } from './MachineName';
import ServerInfo from './ServerInfo';
import PhysicalCountData from '../data/PhysicalCountData';
import PhysicalCountDatabase from '../database/PhysicalCountDatabase';

// Device settings of the physical count, there is no settings page :
// the server is the one of the login screen and the location is the one of that server (LocCode of its web.config), like Receiving Order
// The machine name is the name of the device (MachineName)
// The downloaded counts are kept by server + location (PC_Profiles of PhysicalCountDatabase), the profile of the server is the selected one

const pageSizeStorageKey = '@app_physicalCountPageSize';

const machineName = MachineName;

// Message of the Machine field
const machineNameMessage = (name) => name === ''
    ? MachineNameMessage()
    : 'The machine name is the name of the device, change it in the Settings of the device : About phone > Device name.';

const pageSize = () => AsyncStorage.getItem(pageSizeStorageKey).then(result => result ? parseInt(result, 10) : DEFAULT_PAGE_SIZE);

const savePageSize = (value) => AsyncStorage.setItem(pageSizeStorageKey, value.toString());

const sameServer = (server1, server2) => (server1 ?? '').trim().toUpperCase() === (server2 ?? '').trim().toUpperCase();

// Resolves { profile, locationDescription, message }
// profile : the server + location the count works on (null when never reached), message : why it is not the location of the server
// Offline the last selected profile is kept, so the count downloaded on the device still works
const serverProfile = async () => {

    const server = (await ServerInfo.serverIP()) ?? '';
    const selected = await PhysicalCountDatabase.GetSelectedProfile();

    if (server === '') {
        return { profile: selected, locationDescription: '', message: selected ? '' : 'Please define the server on the login screen' };
    }

    let serverLocation;
    try {
        serverLocation = await PhysicalCountData.GetServerLocation(server);
    }
    catch (error) {
        return { profile: selected, locationDescription: '', message: selected ? '' : `Server not reached : ${error.message}` };
    }

    if (selected && sameServer(selected.server, server) && selected.location === serverLocation.location) {
        return { profile: selected, locationDescription: serverLocation.locationDescription, message: '' };
    }

    // The counted lines of the previous server + location would be left on the device : uploaded first
    if (selected) {
        const summary = await PhysicalCountDatabase.GetLinesSummary(selected);
        if (summary.pending > 0 || summary.pendingLogs > 0) {
            return {
                profile: selected,
                locationDescription: '',
                message: `Location ${selected.location} has counted lines not uploaded to server ${selected.server}, please upload them before working on location ${serverLocation.location} of server ${server}`,
            };
        }
    }

    const profiles = await PhysicalCountDatabase.GetProfiles();
    const existing = profiles.find(profile => sameServer(profile.server, server) && profile.location === serverLocation.location);

    if (existing) {
        await PhysicalCountDatabase.SelectProfile(existing.profileID);
    }
    else {
        await PhysicalCountDatabase.SaveProfile(0, serverLocation.location, server.trim(), true);
    }

    return { profile: await PhysicalCountDatabase.GetSelectedProfile(), locationDescription: serverLocation.locationDescription, message: '' };

};

export default {
    machineName,
    machineNameMessage,
    pageSize,
    savePageSize,
    serverProfile,
}
