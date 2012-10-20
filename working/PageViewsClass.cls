VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "PVChartClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
Option Explicit

Private Main As Object
Private CSV As Object
'
'========================================================================
'   v3.3 Compatibility
'       To make an Add-on that works the same in v3.3 and v3.4, use this adapter instead of the execute above
'========================================================================
'
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
    Call HandleError("PageViewsClass", "Init", Err.Number, Err.Source, Err.Description, True, False)
    End Sub

Public Function GetContent(OptionString As String) As String
    On Error GoTo ErrorTrap
    
    Dim Stream As String
    Dim CS As Long
    Dim cmn As New CommonClass
    Dim dblDateStart As Double
    Dim dblDateEnd As Double
    Dim DateStart As Date
    Dim DateEnd As Date
    
    DateStart = KmaEncodeDate(Main.GetAggrOption("StartDate", OptionString))
    DateEnd = KmaEncodeDate(Main.GetAggrOption("EndDate", OptionString))
    
    If IsDate(DateStart) And IsDate(DateEnd) Then
    
        dblDateStart = CDbl(DateStart)
        dblDateEnd = CDbl(DateEnd)
        
        CS = Main.OpenCSContent("Visit Summary", "(TimeDuration=24) AND (DateNumber>=" & dblDateStart & ") AND (DateNumber<=" & dblDateEnd & ")", "TimeNumber desc", , , , "DateNumber,TimeNumber,PagesViewed")
        If Main.CSOK(CS) Then
            
            Stream = cmn.GetChart(Main, CS, "page-views", False, "100%", "400px")
        
        Else
        
            Stream = "<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>"
        
        End If
        Call Main.CloseCS(CS)
    
    Else
    
        Stream = Main.GetAdminHintWrapper("Please enter a valid Start and End Date to view the Page Views Chart.")
    
    End If
    
    GetContent = Stream
    
    Exit Function
ErrorTrap:
    Call HandleError("PageViewsClass", "GetContent", Err.Number, Err.Source, Err.Description, True, False)
    End Function



