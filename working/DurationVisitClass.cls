VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "DurationVisitClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True

Option Explicit

Const VisitDesc = "VISITS - number of browsers that have been to your site within the charted time period."
Const PageDesc = "PAGES - number of pages viewed on your site within the charted time period."
Const VisitorDesc = "NEW VISITORS - percentage of visits that had never visited your site within the charted time period."
Const BounceDesc = "BOUNCE RATE - percentage of visits that viewed only one page during thier visit within the charted time period."
Const PVDesc = "PAGES/VISIT - average page views per visit within the charted time period."
Const LogInDesc = "LOG IN - visits where someone logged in with a username/password within the charted time period."


Private Main As Object
Private CSV As Object

Public Function Execute(CsvObject As Object, MainObject As Object, OptionString As String, FilterInput As String) As String
    Set CSV = CsvObject
    Call Init(MainObject)
    Execute = GetContent(OptionString)
End Function

Public Sub Init(MainObject As Object)
    On Error GoTo ErrorTrap
    
    Set Main = MainObject
    
    Exit Sub
    
ErrorTrap:
    Call HandleError("VisitClass", "Init", Err.Number, Err.Source, Err.Description, True, False)
    End Sub

Public Function GetContent(OptionString As String) As String
    On Error GoTo ErrorTrap
    
    Dim RS As Recordset
    Dim SQL As String
    Dim RSData As Variant
    Dim Stream As String
    Dim CS As Long
    Dim cmn As New CommonClass
    Dim dblDateStart As Double
    Dim dblDateEnd As Double
    Dim DateStart As Date
    Dim DateEnd As Date
    Dim Duration As Long
    Dim DivName As String
    Dim Rate As String
    Dim intRate As Long
    Dim AllowHourly As Boolean
    Dim Width As String
    Dim Height As String
    Dim CacheData As String
    Dim CacheFilename As String
    Dim CacheExpires As String
    Dim Hint As String
    Dim AllowCache As Boolean
    '
    Width = Main.GetAggrOption("Chart Width", OptionString)
    If Width = "" Then
        Width = "100%"
    End If
    '
    Height = Main.GetAggrOption("Chart Height", OptionString)
    If Height = "" Then
        Height = "400px"
    ElseIf InStr(1, Height, "%") <> 0 Then
        Height = "400px"
    End If
    '
    Rate = Main.GetAggrOption("Daily or Hourly", OptionString)
    If Rate = "Hourly" Then
        intRate = 1
        AllowHourly = True
    Else
        intRate = 24
    End If
    '
    Duration = kmaEncodeInteger(Main.GetAggrOption("Days to Display", OptionString))
    If Duration = 0 Then
        Duration = 365
    End If
    '
    DivName = Main.GetAggrOption("Target Div", OptionString)
    If DivName = "" Then
        DivName = Main.GetAggrOption("TargetDiv", OptionString)
        If DivName = "" Then
            DivName = "durationChart"
        End If
    End If
    
    DateStart = Date - Duration
    DateEnd = Date
    '
    ' Added cache to flush when new data is available
    '
    CacheFilename = "DurationVisit-" & Width & "-" & Height & "-" & DivName & "-" & CStr(AllowHourly) & "-" & intRate & "-" & Duration & "htm"
    CacheFilename = "AppCache\" & EncodeFilename(CacheFilename)
    CacheData = Main.ReadVirtualFile(CacheFilename)
    If CacheData <> "" Then
        CacheExpires = KmaGetLine(CacheData)
        If IsDate(CacheExpires) Then
            If CDate(CacheExpires) > Now() Then
                Stream = CacheData
            End If
        End If
    End If
    AllowCache = True
    If Stream = "" Then
        If IsDate(DateStart) And IsDate(DateEnd) Then
            dblDateStart = CDbl(DateStart)
            dblDateEnd = CDbl(DateEnd)
            '
            SQL = "select " & FieldListVisitSummary & " from ccVisitSummary where (TimeDuration=" & intRate & ") AND (DateNumber>=" & dblDateStart & ") AND (DateNumber<=" & dblDateEnd & ") order by DateNumber, TimeNumber"
            Set RS = Main.ExecuteSQL("default", SQL)
            If RS Is Nothing Then
                AllowCache = False
                Stream = "<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>"
            Else
                If RS.EOF Then
                    AllowCache = False
                    Stream = "<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>"
                Else
                    RSData = RS.GetRows()
                    Stream = cmn.GetChart2(Main, RSData, DivName, True, Width, Height, AllowHourly)
                    Stream = Stream & GetSummary2(RSData, AllowHourly)
                End If
            End If
            Set RS = Nothing
            CacheData = ""
            If AllowCache Then
                CacheData = Now() + (CDbl(intRate) / 24#) & vbCrLf & Stream
            End If
            Call Main.SaveVirtualFile(CacheFilename, CacheData)
        Else
            Stream = Main.GetAdminHintWrapper("Please enter a valid Start and End Date to view the Visit Chart.")
        End If
    End If
    GetContent = Stream
    
    Exit Function
ErrorTrap:
    Call HandleError("VisitClass", "GetContent", Err.Number, Err.Source, Err.Description, True, False)
End Function
'
'Public Function GetSummary(Pointer As Long) As String
'    On Error GoTo ErrorTrap
'
'    Dim Stream As String
'
'    Dim Visits As Double
'    Dim Viewings As Double
'    Dim PagePerVisit As Double
'    Dim NewVisits As Double
'    Dim TimeOnSite As Double
'    Dim BounceRate As Double
'    Dim Authenticated As Double
'
'    Dim RecordCount As Double
'
'    Call Main.FirstCSRecord(Pointer)
'
'    If Main.CSOK(Pointer) Then
'        RecordCount = Main.GetCSRowCount(Pointer)
'        Do While Main.CSOK(Pointer)
'
'            Visits = Main.GetCSInteger(Pointer, "Visits") + Visits
'            Viewings = Main.GetCSInteger(Pointer, "PagesViewed") + Viewings
'            NewVisits = Main.GetCSInteger(Pointer, "NewVisitorVisits") + NewVisits
'            TimeOnSite = Main.GetCSInteger(Pointer, "AveTimeOnSite") + TimeOnSite
'            BounceRate = Main.GetCSInteger(Pointer, "SinglePageVisits") + BounceRate
'            Authenticated = Main.GetCSInteger(Pointer, "AuthenticatedVisits") + Authenticated
'
'            Call Main.NextCSRecord(Pointer)
'        Loop
'
'        If (Viewings <> 0) And (Visits <> 0) Then
'            PagePerVisit = FormatNumber((Viewings / Visits), 1)
'            TimeOnSite = (TimeOnSite / Visits)
'            BounceRate = FormatNumber(((BounceRate / Visits) * 100), 1)
'            NewVisits = FormatNumber(((NewVisits / Visits) * 100), 1)
'            Authenticated = FormatNumber(((Authenticated / Visits) * 100), 1)
'        End If
'
'        Stream = Stream & "<div class=""summaryContainer"">"
'        Stream = Stream & "<table border=""0"" width=""100%"" cellpadding=""3"" cellspacing=""0"">"
'        Stream = Stream & "<tr>"
'        Stream = Stream & "<td class=""summaryHeader"" colspan=""2"" width=""100%"">Summary*</td>"
'        Stream = Stream & "</tr>"
'        Stream = Stream & "<tr>"
'        Stream = Stream & "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & VisitDesc & """ onClick=""return false;""><span class=""summaryValue"">" & Visits & "</span> <span class=""summaryCaption"">Visits</span></a></td>"
'        Stream = Stream & "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & BounceDesc & """ onClick=""return false;""><span class=""summaryValue"">" & BounceRate & "%</span> <span class=""summaryCaption"">Bounce Rate</span></a></td>"
'        Stream = Stream & "</tr>"
'        Stream = Stream & "<tr>"
'        Stream = Stream & "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & PageDesc & """ onClick=""return false;""><span class=""summaryValue"">" & Viewings & "</span> <span class=""summaryCaption"">Pages</span></a></td>"
'        Stream = Stream & "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & PVDesc & """ onClick=""return false;""><span class=""summaryValue"">" & PagePerVisit & "</span> <span class=""summaryCaption"">Pages/Visit</span></a></td>"
'        Stream = Stream & "</tr>"
'        Stream = Stream & "<tr>"
'        Stream = Stream & "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & VisitorDesc & """ onClick=""return false;""><span class=""summaryValue"">" & NewVisits & "%</span> <span class=""summaryCaption"">New Visitors</span></a></td>"
'        Stream = Stream & "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & LogInDesc & """ onClick=""return false;""><span class=""summaryValue"">" & Authenticated & "%</span> <span class=""summaryCaption"">Log In</span></a></td>"
'        Stream = Stream & "</tr>"
'        Stream = Stream & "</table>"
'        Stream = Stream & "</div>"
'    End If
'    Call Main.CloseCS(Pointer)
'
'    GetSummary = Stream
'
'    Exit Function
'ErrorTrap:
'    Call HandleError("DurationVisitClass", "GetSummary", Err.Number, Err.Source, Err.Description, True, False)
'    End Function

Public Function GetSummary2(RSData As Variant, IsHourlyChart As Boolean) As String
    On Error GoTo ErrorTrap
    
    Dim Stream As String
    
    Dim Visits As Double
    Dim Viewings As Double
    Dim PagePerVisit As Double
    Dim NewVisits As Double
    Dim TimeOnSite As Double
    Dim BounceRate As Double
    Dim Authenticated As Double
    Dim Max As Long
    Dim Ptr As Long
    Dim DayCnt As Double
    '
    On Error Resume Next
    Max = UBound(RSData, 2)
    If Err.Number <> 0 Then
        '
        ' No data
        '
        Err.Clear
    Else
        DayCnt = Max + 1
        If IsHourlyChart Then
            DayCnt = DayCnt / 24
        End If
        For Ptr = 0 To Max
            Visits = Visits + kmaEncodeInteger(RSData(RS_Visits, Ptr))
            Viewings = Viewings + kmaEncodeInteger(RSData(RS_PagesViewed, Ptr))
            NewVisits = NewVisits + kmaEncodeInteger(RSData(RS_NewVisitorVisits, Ptr))
            TimeOnSite = TimeOnSite + kmaEncodeInteger(RSData(RS_AveTimeOnSite, Ptr))
            BounceRate = BounceRate + kmaEncodeInteger(RSData(RS_SinglepageVisits, Ptr))
            Authenticated = Authenticated + kmaEncodeInteger(RSData(RS_AuthenticatedVisits, Ptr))
        Next
        
        If (Viewings <> 0) And (Visits <> 0) Then
            PagePerVisit = FormatNumber((Viewings / Visits), 1)
            TimeOnSite = (TimeOnSite / Visits)
            BounceRate = FormatNumber(((BounceRate / Visits) * 100), 1)
            NewVisits = FormatNumber(((NewVisits / Visits) * 100), 1)
            Authenticated = FormatNumber(((Authenticated / Visits) * 100), 1)
        End If

        Stream = Stream & "<div class=""summaryContainer"">"
        Stream = Stream & "<table border=""0"" width=""100%"" cellpadding=""3"" cellspacing=""0"">"
        Stream = Stream & "<tr>"
        Stream = Stream & "<td class=""summaryHeader"" colspan=""2"" width=""100%"">Summary: Last " & DayCnt & " Days</td>"
        Stream = Stream & "</tr>"
        Stream = Stream & "<tr>"
        Stream = Stream & "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & VisitDesc & """ onClick=""return false;""><span class=""summaryValue"">" & Visits & "</span> <span class=""summaryCaption"">Visits</span></a></td>"
        Stream = Stream & "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & BounceDesc & """ onClick=""return false;""><span class=""summaryValue"">" & BounceRate & "%</span> <span class=""summaryCaption"">Bounce Rate</span></a></td>"
        Stream = Stream & "</tr>"
        Stream = Stream & "<tr>"
        Stream = Stream & "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & PageDesc & """ onClick=""return false;""><span class=""summaryValue"">" & Viewings & "</span> <span class=""summaryCaption"">Pages</span></a></td>"
        Stream = Stream & "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & PVDesc & """ onClick=""return false;""><span class=""summaryValue"">" & PagePerVisit & "</span> <span class=""summaryCaption"">Pages/Visit</span></a></td>"
        Stream = Stream & "</tr>"
        Stream = Stream & "<tr>"
        Stream = Stream & "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & VisitorDesc & """ onClick=""return false;""><span class=""summaryValue"">" & NewVisits & "%</span> <span class=""summaryCaption"">New Visitors</span></a></td>"
        Stream = Stream & "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & LogInDesc & """ onClick=""return false;""><span class=""summaryValue"">" & Authenticated & "%</span> <span class=""summaryCaption"">Log In</span></a></td>"
        Stream = Stream & "</tr>"
        Stream = Stream & "</table>"
        Stream = Stream & "</div>"
    End If
    
    GetSummary2 = Stream
    
    Exit Function
ErrorTrap:
    Call HandleError("DurationVisitClass", "GetSummary2", Err.Number, Err.Source, Err.Description, True, False)
    End Function

