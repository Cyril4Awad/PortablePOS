' NOTE: You can use the "Rename" command on the context menu to change the interface name "IService1" in both code and config file together.
<ServiceContract()>
Public Interface IDataService

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function UserLogin(userCode As String, userPassword As String, deviceName As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function UserLogout(deviceName As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetOfflineUsers(deviceName As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function Ping() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetClientGroups(clientGroupType As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetClients(clientType As String, clientCountryRegion As String, searchString As String, latitude As Double, longitude As Double, userCode As String, onlyHavingVisits As Boolean, visitDate As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetItemProductGroups(searchFor As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetItems(itemProductGroup As String, itemPromotionCriteria As String, searchString As String, searchFor As String, userCode As String, supplierRecID As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetROSuppliers(userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetROLocations() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function FindROItem(userCode As String, supplierRecID As String, code As String, roDate As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetROItemLots(itemRecID As String, locationCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetROPendingItems(userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function CheckROBarcode(itemRecID As String, barcode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GenerateROBarcode() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function CheckRORef(roRef As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetROPurchaseOrders(userCode As String, supplierRecID As String, roDate As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetROPurchaseOrderLines(userCode As String, supplierRecID As String, poRecID As String, roDate As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetReceivingOrders(userCode As String, forChange As String, searchString As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetReceivingOrder(userCode As String, roRecID As String, forChange As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveReceivingOrder(userCode As String, saveMode As String, receivingOrder As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetItemImage(itemRecID As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetItemSpecs(itemRecID As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetItemPriceDiscount(itemRecID As String, clientRecID As String, currency As String, qty As Double) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetVatType(clientPriceCat As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetCurrencies() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetItemVatValue(amount As String, itemVatType As String, itemVatPerc As String, clientPriceCat As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetOrders(userCode As String, isReturn As Boolean) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetOrderDetails(orderNumber As String, isReturn As Boolean, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveMissingItem(description As String, brand As String, requestedBy As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SavePotentialClient(clientName As String, business As String, contact As String, phoneMobile As String, address As String, remarks As String, latitude As Double, longitude As Double, accuracy As Double, radiusInMeters As Double, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveClientVisit(clientRecID As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveDeliveryProof(clientRecID As String, remarks As String, photoBase64 As String, latitude As Double, longitude As Double, accuracy As Double, radiusInMeters As Double, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveOrder(isReturn As Boolean, clientRecID As String, orderVatType As String, orderDetails As Object, orderHeaderRemark As String, orderFooterRemark As String, orderCurrency As String, orderTotal As String, orderTotalVAT As String, orderDiscrepancy As Boolean, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetSalesOrders() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetSalesOrderItems(headerRecID As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetSalesOrderDetailedQty(headerRecID As String, counter As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveSalesOrderDetailedQty(headerRecID As String, counter As String, itemRecID As String, expiryLotString As String, qty As String, machine As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function DeleteSalesOrderDetailedQty(recID As String, headerRecID As String, counter As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function OpenSalesOrder(headerRecID As String, machine As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function CloseSalesOrder(headerRecID As String, machine As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SendSalesOrderToPOS(headerRecID As String, machine As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetClientDues(clientRecID As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveClientReceipt(cashRegister As String, clientRecID As String, clientName As String, paidAmountLC As String, paidAmountCC As String, userCode As String, duesList As Object, invoiceRate As Double, cashLC As Double, cashCC As Double) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveClientGeoLocation(clientRecID As String, latitude As Double, longitude As Double, accuracy As Double, radiusInMeters As Double, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetClientGeoLocation(clientRecID As String, latitude As Double, longitude As Double) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetClientLastPurchases(clientRecID As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetClientStatement(clientRecID As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetPromotion(itemRecID As String, itemQty As Double, clientRecID As String, takenPromotions As Object, transactionCurrency As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetPromotions() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveInvoice(cashRegister As String, isReturn As Boolean, clientRecID As String, orderVatType As String, orderDetails As Object, orderHeaderRemark As String, orderFooterRemark As String, orderCurrency As String, orderTotal As String, orderTotalVAT As String, userCode As String, invoiceRate As Double, paidAmount As Double, clientName As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function RegisterDevice(deviceName As String, deviceID As String, deviceUniqueID As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveCashCount(userCode As String, deviceName As String, cashRegister As String, amountLC As Double, amountCC As Double) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetCashRegister(deviceName As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetWorkingDay() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetLocationPretty() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetCashRegisterPretty(deviceName As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetControlizedMenu() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveControlizedMenu(userCode As String, menuItems As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetUserPrivileges(userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetPhysicalCountHeader(location As String, countNumber As String, machineName As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function CheckPhysicalCountLocation(location As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetPhysicalCountDetails(location As String, countNumber As String, machineName As String, startFromRow As Integer, pageSize As Integer, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetPhysicalCountBarcodes(location As String, countNumber As String, machineName As String, startFromRow As Integer, pageSize As Integer) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SavePhysicalCountLines(location As String, countNumber As String, machineName As String, lines As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SavePhysicalCountLogs(location As String, countNumber As String, machineName As String, logs As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetPhysicalCountFilters(location As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetPhysicalCountItems(location As String, filters As String, selection As String, searchText As String, onlySelected As Boolean, afterReference As String, afterItemRecID As String, pageSize As Integer, exactMatch As Boolean, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetPhysicalCountNegations(location As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function CreatePhysicalCount(location As String, machineName As String, filters As String, selection As String, memo As String, requestID As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetMachinePhysicalCounts(location As String, machineName As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetLabelItem(barcode As String, itemRecID As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SearchLabelItems(searchText As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetLabelDesigns() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function PrintLabel(itemRecID As String, printCode As String, labelDesign As String, qty As Integer) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function PreviewLabel(itemRecID As String, printCode As String, labelDesign As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetLabelPrintData(itemRecID As String, printCode As String, labelDesign As String, qty As Integer) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetLabelRulerData() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetReceivingInfo() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetReceivingItems(afterRecID As String, pageSize As Integer) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetReceivingItemCodes(codeKind As String, afterRecID As String, pageSize As Integer) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetReceivingSuppliers() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveFreeReceiving(userCode As String, receivingOrders As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetReceivingLots(afterRecID As String, pageSize As Integer) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetTransferLocations() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetTransferInfo(location As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetTransferStock(location As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function FindTransferItem(location As String, code As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetTransferLotsAvailable(location As String, lotRecIDs As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveTransfer(location As String, machineName As String, saveID As String, transfer As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetReprintDocuments(docType As String, fromDate As String, toDate As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetReprintDocumentPdf(docType As String, recID As String, userCode As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function PrintReprintDocument(docType As String, recID As String, userCode As String, printerName As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function GetReprintPrinters() As String

End Interface

