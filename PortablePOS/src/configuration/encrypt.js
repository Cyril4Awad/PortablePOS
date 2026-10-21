import CryptoJS from 'react-native-crypto-js';

const ky = CryptoJS.enc.Utf8.parse('TTFront-End-PatApp');
const iv = CryptoJS.enc.Utf8.parse('TT-TY2020');

export const encrypttext = (text) => {
  let myText = CryptoJS.enc.Utf8.parse(text);
  let ciphertext = CryptoJS.AES.encrypt(myText, ky, {
    keySize: 128 / 8
    , iv: iv
    , mode: CryptoJS.mode.CBC
    , padding: CryptoJS.pad.Pkcs7
  });
  return ciphertext.toString();
};
export const encrypttextFile = (text, key) => {
  let myText = CryptoJS.enc.Utf8.parse(text);
  let ciphertext = CryptoJS.AES.encrypt(myText, key, {
    keySize: 128 / 8
    , iv: iv
    , mode: CryptoJS.mode.CBC
    , padding: CryptoJS.pad.Pkcs7
  });
  return ciphertext.toString();
};
export const decrypttext = (encryptedText) => {
  let ciphertext = CryptoJS.AES.decrypt(encryptedText, ky, {
    keySize: 128 / 8,
    iv: iv,
    mode: CryptoJS.mode.CBC,
    padding: CryptoJS.pad.Pkcs7
  });
  return ciphertext.toString(CryptoJS.enc.Utf8).toString();
};
