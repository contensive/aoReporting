
Public Module Constants
    '
    ' -- sample
    Public Const Version As Integer = 1
    '
    Public Const cr As String = vbCrLf & vbTab
    Public Const cr2 As String = vbCrLf & vbTab & vbTab
    Public Const cr3 As String = vbCrLf & vbTab & vbTab & vbTab
    '
    Public Const VisitDesc As String = "VISITS - number of browsers that have been to your site within the charted time period."
    Public Const PageDesc As String = "PAGES - number of pages viewed on your site within the charted time period."
    Public Const VisitorDesc As String = "NEW VISITORS - percentage of visits that had never visited your site within the charted time period."
    Public Const BounceDesc As String = "BOUNCE RATE - percentage of visits that viewed only one page during thier visit within the charted time period."
    Public Const PVDesc As String = "PAGES/VISIT - average page views per visit within the charted time period."
    Public Const LogInDesc As String = "LOG IN - visits where someone logged in with a username/password within the charted time period."
    '
    ' -- errors for resultErrList
    Public Enum ResultErrorEnum
        errPermission = 50
        errDuplicate = 100
        errVerification = 110
        errRestriction = 120
        errInput = 200
        errAuthentication = 300
        errAdd = 400
        errSave = 500
        errDelete = 600
        errLookup = 700
        errLoad = 710
        errContent = 800
        errMiscellaneous = 900
    End Enum
    '
    ' -- http errors
    Public Enum HttpErrorEnum
        badRequest = 400
        unauthorized = 401
        paymentRequired = 402
        forbidden = 403
        notFound = 404
        methodNotAllowed = 405
        notAcceptable = 406
        proxyAuthenticationRequired = 407
        requestTimeout = 408
        conflict = 409
        gone = 410
        lengthRequired = 411
        preconditionFailed = 412
        payloadTooLarge = 413
        urlTooLong = 414
        unsupportedMediaType = 415
        rangeNotSatisfiable = 416
        expectationFailed = 417
        teapot = 418
        methodFailure = 420
        enhanceYourCalm = 420
        misdirectedRequest = 421
        unprocessableEntity = 422
        locked = 423
        failedDependency = 424
        upgradeRequired = 426
        preconditionRequired = 428
        tooManyRequests = 429
        requestHeaderFieldsTooLarge = 431
        loginTimeout = 440
        noResponse = 444
        retryWith = 449
        redirect = 451
        unavailableForLegalReasons = 451
        sslCertificateError = 495
        sslCertificateRequired = 496
        httpRequestSentToSecurePort = 497
        invalidToken = 498
        clientClosedRequest = 499
        tokenRequired = 499
        internalServerError = 500
    End Enum
    '
    Public Const rnButton As String = "button"
    '
    Public Const rnSrcFormId As String = "srcFormId"
    Public Const rnDstFormId As String = "dstFormId"
    '
    Public Const rnFrameName As String = "frameName"
    Public Const rnFrameRqs As String = "frameRqs"
    Public Const frameMyAccountMain As String = "abFrameMain"
    '
    Public Const buttonPrintAll As String = " Print All "
    Public Const buttonPrintSelected As String = " Print Selected "
    Public Const buttonClearPrintQueue As String = " Clear Print Queue "
    Public Const buttonOK As String = " OK "
    Public Const buttonSave As String = " Save "
    Public Const buttonSaveAndNew As String = " Save and New "
    Public Const buttonCancel As String = " Cancel "
    Public Const ButtonRefresh As String = "  Refresh  "
    Public Const buttonAdd As String = " Add "
    Public Const buttonClearSelectedAlerts As String = " Clear Selected Alerts "
    Public Const buttonClearAlerts As String = " Clear All Alerts "
    Public Const buttonCloseAccount As String = " Close Account "
    Public Const buttonSendInvoice As String = " Send Selected Invoices "
    Public Const buttonSendStatements As String = " Send Statements "
    Public Const buttonSendStatement As String = " Send Statement "
    Public Const buttonCreateInvoice As String = " Create Invoice "
    Public Const buttonCancelInvoice As String = " Cancel Selected Invoices "
    Public Const buttonCreateCredit As String = " Create Credit Memo "
    Public Const buttonCreateCharge As String = " Create Account Charge "
    Public Const buttonCancelSubscription As String = " Cancel Selected "
    Public Const buttonDeleteSubscription As String = " Delete Selected "
    Public Const buttonProcessHousekeepNow As String = " Process Housekeep Now "
    Public Const buttonProcessHousekeepBackground As String = " Process Housekeep In The Background "
    Public Const buttonProcessPayment As String = " Process Payment "
    Public Const buttonUpdate As String = " Update "
    Public Const buttonAddUser As String = " Add User "
    Public Const buttonDeleteUser As String = " Delete User "
    Public Const buttonExportData As String = " Export Now "
    Public Const buttonAddItem As String = " Add Item "
    Public Const buttonRemoveFromAccount As String = "Remove From Account"
    '
    ' -- if there are multiple forms to an addon, use these values to multiple between them
    Public Const formIdDefault As Integer = 1
    Public Const formIdreportMax As Integer = 1
    '
    Public Const rnSrcFeatureGuid As String = "srcFeatureGuid"
    Public Const rnDstFeatureGuid As String = "dstFeatureGuid"
    Public Const rnAccountId As String = "accountId"
    Public Const pageViewGuid As String = "4AA883AA-21BC-4310-9E40-166B42D3C79C"
End Module
