VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "CommonClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True

Option Explicit
'
'
'
Friend Function GetChart(Main As Object, CS As Long, Div As String, isVisitData As Boolean, Optional Width As String, Optional Height As String, Optional AllowHourly As Boolean) As String

    Dim Stream As String
    Dim Value As String
    Dim Pointer As Long
    Dim nrmDate As Date
    Dim dblDate As Double
    Dim dblTime As Double
    Dim Caption As String
    
    If Width = "" Then
        Width = "100%"
    End If
    If Height = "" Then
        Height = "400px"
    End If

    If isVisitData Then
        Caption = "Visits"
    Else
        Caption = "Page Views"
    End If
    
    Stream = vbCrLf & vbCrLf
    
    Stream = Stream & "<script type='text/javascript'>" & vbCrLf
    Stream = Stream & "google.load('visualization', '1', {'packages':['annotatedtimeline']});" & vbCrLf
    Stream = Stream & "google.setOnLoadCallback(drawChart);" & vbCrLf
    Stream = Stream & "function drawChart() {" & vbCrLf
    Stream = Stream & "var data = new google.visualization.DataTable();" & vbCrLf
    Stream = Stream & "data.addColumn('date', 'Date');" & vbCrLf
    Stream = Stream & "data.addColumn('number', '" & Caption & "');" & vbCrLf
    Stream = Stream & "data.addRows(" & Main.GetCSRowCount(CS) & ");" & vbCrLf
    
    Do While Main.CSOK(CS)
        dblDate = Main.GetCSNumber(CS, "DateNumber")
        dblTime = Main.GetCSNumber(CS, "TimeNumber")
Call Main.TestPoint("dblTime: " & dblTime)
        If (dblTime <> 0) And (AllowHourly) Then
            nrmDate = CDate(dblDate + (dblTime / 24!))
        Else
            nrmDate = CDate(dblDate)
        End If
        
        If isVisitData Then
            Value = Main.GetCS(CS, "Visits")
        Else
            Value = Main.GetCS(CS, "PagesViewed")
        End If
        
        Stream = Stream & "data.setValue(" & Pointer & ", 0, new Date(" & Year(nrmDate) & "," & Month(nrmDate) - 1 & "," & Day(nrmDate) & "," & DatePart("H", nrmDate) & ",00,00));" & vbCrLf
        Stream = Stream & "data.setValue(" & Pointer & ", 1, " & Value & ");" & vbCrLf
        Call Main.NextCSRecord(CS)
        Pointer = Pointer + 1
    Loop
    
    Stream = Stream & "var chart = new google.visualization.AnnotatedTimeLine(document.getElementById('" & Div & "'));" & vbCrLf
    Stream = Stream & "chart.draw(data, {displayAnnotations: false});" & vbCrLf
    Stream = Stream & "}" & vbCrLf
    
    Stream = Stream & "</script>" & vbCrLf & vbCrLf

    Stream = Stream & "<div id='" & Div & "' style='width: " & Width & "; height: " & Height & ";'></div>"
    
    GetChart = Stream

End Function
'
'
'   RSData is a 3xlength array of the data
'       a(0,n) = DateNumber (int of Dbl Date)
'       a(1,n) = TimeNumber (0-23)
'       a(2,n) = value to plot
' AllowHourly - if true, there must be all 24 time numbers for each date number
'
Friend Function GetChart2(Main As Object, RSData As Variant, Div As String, isVisitData As Boolean, Optional Width As String, Optional Height As String, Optional AllowHourly As Boolean) As String

    Dim Stream As String
    Dim Value As String
    Dim Pointer As Long
    Dim nrmDate As Date
    Dim dblDate As Double
    Dim dblTime As Double
    Dim Caption As String
    Dim Ptr As Long
    Dim Max As Long
    Dim TimeNumber As Double
    Dim DateNumber As Double
    Dim PlotDate As Date
    Dim PlotValue As Double
    
    If Width = "" Then
        Width = "100%"
    End If
    If Height = "" Then
        Height = "400px"
    End If

    If isVisitData Then
        Caption = "Visits"
    Else
        Caption = "Page Views"
    End If
    On Error Resume Next
    Max = UBound(RSData, 2)
    If Err.Number <> 0 Then
        '
        ' No data
        '
        Err.Clear
    Else
        Stream = vbCrLf & vbCrLf
        
        Stream = Stream & "<script type='text/javascript'>" & vbCrLf
        Stream = Stream & "google.load('visualization', '1', {'packages':['annotatedtimeline']});" & vbCrLf
        Stream = Stream & "google.setOnLoadCallback(drawChart);" & vbCrLf
        Stream = Stream & "function drawChart() {" & vbCrLf
        Stream = Stream & "var data = new google.visualization.DataTable();" & vbCrLf
        Stream = Stream & "data.addColumn('date', 'Date');" & vbCrLf
        Stream = Stream & "data.addColumn('number', '" & Caption & "');" & vbCrLf
        Stream = Stream & "data.addRows(" & (Max + 1) & ");" & vbCrLf
        '
        ' Plot what we got
        '
        For Ptr = 0 To Max
            DateNumber = Int(KmaEncodeNumber(RSData(RS_DateNumber, Ptr)) + 0.5)
            TimeNumber = Int(KmaEncodeNumber(RSData(RS_TimeNumber, Ptr)) + 0.5)
            If isVisitData Then
                PlotValue = KmaEncodeNumber(RSData(RS_Visits, Ptr))
            Else
                PlotValue = KmaEncodeNumber(RSData(RS_PagesViewed, Ptr))
            End If
            If (TimeNumber <> 0) And (AllowHourly) Then
                PlotDate = CDate(DateNumber + (TimeNumber / 24!))
            Else
                PlotDate = CDate(DateNumber)
            End If
            Stream = Stream & "data.setValue(" & Ptr & ", 0, new Date(" & Year(PlotDate) & "," & Month(PlotDate) - 1 & "," & Day(PlotDate) & "," & DatePart("H", PlotDate) & ",00,00));" & vbCrLf
            Stream = Stream & "data.setValue(" & Ptr & ", 1, " & PlotValue & ");" & vbCrLf
        Next
    End If
'
'    Do While Main.CSOK(CS)
'        dblDate = Main.GetCSNumber(CS, "DateNumber")
'        dblTime = Main.GetCSNumber(CS, "TimeNumber")
'Call Main.TestPoint("dblTime: " & dblTime)
'        If (dblTime <> 0) And (AllowHourly) Then
'            nrmDate = CDate(dblDate + (dblTime / 24!))
'        Else
'            nrmDate = CDate(dblDate)
'        End If
'
'        If isVisitData Then
'            Value = Main.GetCS(CS, "Visits")
'        Else
'            Value = Main.GetCS(CS, "PagesViewed")
'        End If
'
'        Stream = Stream & "data.setValue(" & Pointer & ", 0, new Date(" & Year(nrmDate) & "," & Month(nrmDate) - 1 & "," & Day(nrmDate) & "," & DatePart("H", nrmDate) & ",00,00));" & vbCrLf
'        Stream = Stream & "data.setValue(" & Pointer & ", 1, " & Value & ");" & vbCrLf
'        Call Main.NextCSRecord(CS)
'        Pointer = Pointer + 1
'    Loop
    
    Stream = Stream & "var chart = new google.visualization.AnnotatedTimeLine(document.getElementById('" & Div & "'));" & vbCrLf
    Stream = Stream & "chart.draw(data, {displayAnnotations: false});" & vbCrLf
    Stream = Stream & "}" & vbCrLf
    
    Stream = Stream & "</script>" & vbCrLf & vbCrLf

    Stream = Stream & "<div id='" & Div & "' style='width: " & Width & "; height: " & Height & ";'></div>"
    
    GetChart2 = Stream

End Function

