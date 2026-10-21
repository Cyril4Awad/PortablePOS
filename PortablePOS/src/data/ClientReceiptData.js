import ServerInfo from "../configuration/ServerInfo";
import { getLoginUserCode } from "../variables/LoginInfo";
import DeviceInfo from "react-native-device-info";
import { getCashRegisterCode } from "../variables/CashierInfo";

const GetClientDues = (
  clientRecID,
  { setDuesList },
  { setIsLoading },
  { setError }) => {

  ServerInfo.serverURL().then((serverURL) => {

    setIsLoading(true);

    fetch(serverURL + 'GetClientDues',
      {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
          token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
        },
        body: JSON.stringify({
          clientRecID: clientRecID
        })
      },
    )
      .then(response => response.json())
      .then(results => {

        var strResult = results.GetClientDuesResult;

        if (strResult == '') {
          setDuesList([]);
        } else {
          setDuesList(JSON.parse(strResult));
        }

        setIsLoading(false);

      })
      .catch(err => {
        setIsLoading(false);
        setError(err);
      });

  }).catch((error) => {
    console.log(error)
  });

};

const SaveClientReceipt = (
  clientRecID,
  clientName,
  paidAmountLC,
  paidAmountCC,
  duesList,
  invoiceRate,
  cashLC,
  cashCC,
  PrintReceipt
) => {

  ServerInfo.serverURL().then((serverURL) => {

    //setIsLoading(true);
    //console.log(orderDetails);

    fetch(serverURL + 'SaveClientReceipt',
      {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
          token : DeviceInfo.getDeviceNameSync() + "|" + DeviceInfo.getDeviceId() + "_" + DeviceInfo.getUniqueIdSync()
        },
        body: JSON.stringify({
          cashRegister: getCashRegisterCode(),
          clientRecID: clientRecID,
          clientName: clientName,
          paidAmountLC: paidAmountLC,
          paidAmountCC: paidAmountCC,
          userCode: getLoginUserCode(),
          duesList: duesList,
          invoiceRate: invoiceRate,
          cashLC: cashLC,
          cashCC: cashCC
        })
      },
    )
      .then(response => response.json())
      .then(results => {

        var result = results.SaveClientReceiptResult;

        if (result.startsWith('Success')) {
          alert('Receipt(s) Saved - Number(s) : ' + result.replace('Success|','').replace('Success|',' - '));
          PrintReceipt(result.replace('Success|','').replace('Success|',' - ')
          );
        }
        else {
          alert(result);
          //alert('Cannot Save The Receipt, Please Try Again Later');
        }

        //setIsLoading(false);

      })
      .catch(err => {
        //setIsLoading(false);
        //setError(err);
        alert(err);
      });

  }).catch((error) => {
    console.log(error)
    alert(error);
  });
};

export default {
  GetClientDues: GetClientDues,
  SaveClientReceipt: SaveClientReceipt
}