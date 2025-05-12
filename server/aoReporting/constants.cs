
public static class Constants {
    // 
    // -- sample
    public const int Version = 1;
    // 
    public const string cr = Microsoft.VisualBasic.Constants.vbCrLf + Microsoft.VisualBasic.Constants.vbTab;
    public const string cr2 = Microsoft.VisualBasic.Constants.vbCrLf + Microsoft.VisualBasic.Constants.vbTab + Microsoft.VisualBasic.Constants.vbTab;
    public const string cr3 = Microsoft.VisualBasic.Constants.vbCrLf + Microsoft.VisualBasic.Constants.vbTab + Microsoft.VisualBasic.Constants.vbTab + Microsoft.VisualBasic.Constants.vbTab;
    // 
    public const string VisitDesc = "VISITS - number of browsers that have been to your site within the charted time period.";
    public const string PageDesc = "PAGES - number of pages viewed on your site within the charted time period.";
    public const string VisitorDesc = "NEW VISITORS - percentage of visits that had never visited your site within the charted time period.";
    public const string BounceDesc = "BOUNCE RATE - percentage of visits that viewed only one page during thier visit within the charted time period.";
    public const string PVDesc = "PAGES/VISIT - average page views per visit within the charted time period.";
    public const string LogInDesc = "LOG IN - visits where someone logged in with a username/password within the charted time period.";
    // 
    // -- errors for resultErrList
    public enum ResultErrorEnum {
        errPermission = 50,
        errDuplicate = 100,
        errVerification = 110,
        errRestriction = 120,
        errInput = 200,
        errAuthentication = 300,
        errAdd = 400,
        errSave = 500,
        errDelete = 600,
        errLookup = 700,
        errLoad = 710,
        errContent = 800,
        errMiscellaneous = 900
    }
    // 
    // -- http errors
    public enum HttpErrorEnum {
        badRequest = 400,
        unauthorized = 401,
        paymentRequired = 402,
        forbidden = 403,
        notFound = 404,
        methodNotAllowed = 405,
        notAcceptable = 406,
        proxyAuthenticationRequired = 407,
        requestTimeout = 408,
        conflict = 409,
        gone = 410,
        lengthRequired = 411,
        preconditionFailed = 412,
        payloadTooLarge = 413,
        urlTooLong = 414,
        unsupportedMediaType = 415,
        rangeNotSatisfiable = 416,
        expectationFailed = 417,
        teapot = 418,
        methodFailure = 420,
        enhanceYourCalm = 420,
        misdirectedRequest = 421,
        unprocessableEntity = 422,
        locked = 423,
        failedDependency = 424,
        upgradeRequired = 426,
        preconditionRequired = 428,
        tooManyRequests = 429,
        requestHeaderFieldsTooLarge = 431,
        loginTimeout = 440,
        noResponse = 444,
        retryWith = 449,
        redirect = 451,
        unavailableForLegalReasons = 451,
        sslCertificateError = 495,
        sslCertificateRequired = 496,
        httpRequestSentToSecurePort = 497,
        invalidToken = 498,
        clientClosedRequest = 499,
        tokenRequired = 499,
        internalServerError = 500
    }
    // 
    public const string rnButton = "button";
    // 
    public const string rnSrcFormId = "srcFormId";
    public const string rnDstFormId = "dstFormId";
    // 
    public const string rnFrameName = "frameName";
    public const string rnFrameRqs = "frameRqs";
    public const string frameMyAccountMain = "abFrameMain";
    // 
    public const string buttonPrintAll = " Print All ";
    public const string buttonPrintSelected = " Print Selected ";
    public const string buttonClearPrintQueue = " Clear Print Queue ";
    public const string buttonOK = " OK ";
    public const string buttonSave = " Save ";
    public const string buttonSaveAndNew = " Save and New ";
    public const string buttonCancel = " Cancel ";
    public const string ButtonRefresh = "  Refresh  ";
    public const string buttonAdd = " Add ";
    public const string buttonClearSelectedAlerts = " Clear Selected Alerts ";
    public const string buttonClearAlerts = " Clear All Alerts ";
    public const string buttonCloseAccount = " Close Account ";
    public const string buttonSendInvoice = " Send Selected Invoices ";
    public const string buttonSendStatements = " Send Statements ";
    public const string buttonSendStatement = " Send Statement ";
    public const string buttonCreateInvoice = " Create Invoice ";
    public const string buttonCancelInvoice = " Cancel Selected Invoices ";
    public const string buttonCreateCredit = " Create Credit Memo ";
    public const string buttonCreateCharge = " Create Account Charge ";
    public const string buttonCancelSubscription = " Cancel Selected ";
    public const string buttonDeleteSubscription = " Delete Selected ";
    public const string buttonProcessHousekeepNow = " Process Housekeep Now ";
    public const string buttonProcessHousekeepBackground = " Process Housekeep In The Background ";
    public const string buttonProcessPayment = " Process Payment ";
    public const string buttonUpdate = " Update ";
    public const string buttonAddUser = " Add User ";
    public const string buttonDeleteUser = " Delete User ";
    public const string buttonExportData = " Export Now ";
    public const string buttonAddItem = " Add Item ";
    public const string buttonRemoveFromAccount = "Remove From Account";
    // 
    // -- if there are multiple forms to an addon, use these values to multiple between them
    public const int formIdDefault = 1;
    public const int formIdreportMax = 1;
    // 
    public const string rnSrcFeatureGuid = "srcFeatureGuid";
    public const string rnDstFeatureGuid = "dstFeatureGuid";
    public const string rnAccountId = "accountId";
    public const string pageViewGuid = "4AA883AA-21BC-4310-9E40-166B42D3C79C";
}