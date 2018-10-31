Imports System
Imports System.Collections.Generic
Imports System.Text

Public Class LocalFTPRequest
		Public fromAccount As String
		Public toAccount As String
		Public amount As Double
		Public requestCode As String
		Public principalIdentifier As String
		Public referenceCode As String
		Public beneficiaryName As String
		Public paymentReference As String
	End Class

Public Class LocalFTResp
	Public Property availabeBalanceAfterOperation As String
	Public Property responseCode As String
	Public Property responseMessage As String
	Public Property errorCode As String
	Public Property errorMessage As String
	Public Property iMALTransactionCode As String
	Public Property skipProcessing As String
	Public Property originalResponseCode As String
	Public Property skipLog As String
	Public Property transactionID As String
End Class

Public Class AccountDetailsResp
	Public Property branchCode As String
	Public Property subAccountCode As String
	Public Property currencyCode As String
	Public Property CurrencyName As String
	Public Property LedgerName As String
	Public Property glCode As String
	Public Property customerNumber As String
	Public Property name As String
	Public Property BVN As String
	Public Property responseCode As String
	Public Property skipProcessing As String
	Public Property skipLog As String
	Public Property status As String
	Public Property availableBalance As Double
	Public Property ledgerBalance As String
	Public Property lastTransactionDate As String
End Class

Public Class AccountDetailsRequest
	Public account As String
	Public requestCode As String
	Public principalIdentifier As String
	Public referenceCode As String
End Class