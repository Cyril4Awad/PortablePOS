import Sound from 'react-native-sound';

// Scan feedback of the physical count : short beep when found, two tones when not found, rising tone when a quantity is confirmed
// Files are in android/app/src/main/res/raw

Sound.setCategory('Playback');

const loadSound = (fileName) => {
    const sound = new Sound(fileName, Sound.MAIN_BUNDLE, (error) => {
        if (error) {
            console.log('Cannot load sound ' + fileName, error);
        }
    });
    return sound;
};

const scanOkSound = loadSound('physical_count_scan_ok.wav');
const scanErrorSound = loadSound('physical_count_scan_error.wav');
const qtyOkSound = loadSound('physical_count_qty_ok.wav');

const play = (sound) => {
    try {
        sound.stop(() => sound.play());
    } catch (error) {
        console.log(error);
    }
};

export const PlayScanOk = () => play(scanOkSound);

export const PlayScanError = () => play(scanErrorSound);

export const PlayQtyConfirmed = () => play(qtyOkSound);
