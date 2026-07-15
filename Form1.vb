Imports System
Imports System.IO
Imports System.Windows.Forms.DataVisualization.Charting
Public Class Form1
    Dim pathlength As Integer
    Dim maxnlagtime As Integer
    ReadOnly StartPath As String = Application.StartupPath
    Dim path As String = "0"
    Dim listfiles(1000) As String
    Dim MSDX(500) As Double
    Dim MSDY(500) As Double
    Dim nonGausx(500) As Double
    Dim nonGausy(500) As Double
    Dim listlag(500) As Integer
    Dim count(500) As Long
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim diag = New FolderBrowserDialog()
        Dim line As String
        Dim temp As String()
        Dim nplot As Integer = 1
        Dim x, y As Single
        diag.Description = "Select folder to analyze"
        diag.SelectedPath = StartPath
        If diag.ShowDialog() = DialogResult.OK Then
            path = diag.SelectedPath & "\"
        End If
        pathlength = path.Length
        listfiles = Directory.GetFiles(path, "*.csv")
        Array.Sort(listfiles)
        For i = 0 To listfiles.Length - 1
            ListBox1.Items.Add(listfiles(i).Remove(0, pathlength))
        Next
        'Dim tot As Integer = Math.Min(100, listfiles.Length)
        Dim total As Integer
        If listfiles.Length > 100 Then
            total = 100
        End If
        If listfiles.Length < 100 Then
            total = listfiles.Length
        End If
        Label16.Text = total
        For i = 0 To total - 1
            Dim reader As New System.IO.StreamReader(listfiles(i))
            reader.ReadLine()
            'temp = line.Split(Chr(9))
            Chart1.Series.Add(listfiles(i).Remove(0, pathlength))
            Do While reader.Peek() <> -1 And nplot < 500
                line = reader.ReadLine()
                temp = line.Split(",")
                x = CSng(temp(1))
                y = CSng(temp(2))
                Chart1.Series(listfiles(i).Remove(0, pathlength)).ChartType = SeriesChartType.Point
                Chart1.Series(listfiles(i).Remove(0, pathlength)).Points.AddXY(x, y)
                nplot += 1
            Loop
            reader.Close()
            nplot = 1
        Next
        Label12.Text = listfiles.Length
        Button4.Enabled = True
    End Sub
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Label3.Text = "In Progress"
        BackgroundWorker1.WorkerSupportsCancellation = True
        BackgroundWorker1.WorkerReportsProgress = True
        BackgroundWorker1.RunWorkerAsync()
        ListBox2.Items.Clear()
        Button4.Enabled = False
        Button2.Enabled = True
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        'aca va calculo de desplazamientos cuadraticos medios
        Dim line As String
        Dim temp As String()
        Dim maxframe As Integer = CSng(TextBox2.Text) - 1
        Dim x(maxframe) As Single
        Dim y(maxframe) As Single
        Dim pixtoum As Single = CSng(TextBox4.Text)
        Dim temporal As Integer
        Dim r As Single
        Dim t As Single
        r = (TextBox5.Text - 1) ^ (1 / (TextBox6.Text))
        Dim jj As Integer
        Dim ij As Integer = 1
        listlag(0) = 1
        For jj = 1 To TextBox6.Text
            t = Math.Round(r ^ (jj))
            If t <> listlag(ij - 1) Then
                listlag(ij) = t
                ij += 1
            End If
            maxnlagtime = ij - 1
        Next
        ReDim Preserve listlag(maxnlagtime)
        'Label7.Text = maxnlagtime
        ReDim MSDX(maxnlagtime)
        ReDim MSDY(maxnlagtime)
        ReDim nonGausx(maxnlagtime)
        ReDim nonGausy(maxnlagtime)
        ReDim count(maxnlagtime)
        temporal = path.Length
        'Label1.Text = "empiezo"
        For i = 0 To listfiles.Length - 1
            Dim reader As New StreamReader(listfiles(i))
            reader.ReadLine()
            Name = listfiles(i).Remove(0, temporal)
            If BackgroundWorker1.CancellationPending Then
                BackgroundWorker1.ReportProgress(CInt(i * 100 / listfiles.Length), "Cancelling...")
                Exit For
            End If
            BackgroundWorker1.ReportProgress(CInt(i * 100 / listfiles.Length), "Running..." & i.ToString)
            ' Dim j As Integer = 0
            For j = 0 To maxframe - 1
                line = reader.ReadLine()
                temp = line.Split(",")
                x(j) = CSng(temp(1)) / pixtoum
                y(j) = CSng(temp(2)) / pixtoum
            Next
            reader.Close()
            For j = 0 To maxnlagtime - 1
                For k = 0 To (maxframe - 1) - listlag(j)
                    MSDX(j) = MSDX(j) + (x(k + listlag(j)) - x(k)) ^ 2
                    MSDY(j) = MSDY(j) + (y(k + listlag(j)) - y(k)) ^ 2
                    nonGausx(j) = nonGausx(j) + (x(k + listlag(j)) - x(k)) ^ 4
                    nonGausy(j) = nonGausy(j) + (y(k + listlag(j)) - y(k)) ^ 4
                    count(j) = count(j) + 1
                Next
            Next
        Next
        For j = 0 To maxnlagtime - 1
            MSDX(j) = MSDX(j) / count(j)
            MSDY(j) = MSDY(j) / count(j)
            nonGausx(j) = (nonGausx(j) / count(j)) / (3 * MSDX(j) * MSDX(j)) - 1
            nonGausy(j) = (nonGausy(j) / count(j)) / (3 * MSDY(j) * MSDY(j)) - 1
        Next
    End Sub
    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        ListBox2.Items.Add(Name)
        ListBox2.TopIndex = ListBox2.Items.Count - 1
    End Sub
    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        '' This event is fired when your BackgroundWorker exits.
        '' It may have exitted Normally after completing its task, 
        '' or because of Cancellation, or due to any Error.
        If e.Error IsNot Nothing Then
            '' if BackgroundWorker terminated due to error
            Label3.Text = "Error occurred!"
        ElseIf e.Cancelled Then
            '' otherwise if it was cancelled
            Label3.Text = "Cancelled!"
        Else
            '' otherwise it completed normally
            Label3.Text = "Completed!"
        End If
        Dim framerate As Integer = CInt(TextBox3.Text)
        'For j = 0 To maxj
        'ListBox2.Items.Add(listlag(j))
        'Next
        Button2.Enabled = False
        Button4.Enabled = True
        Chart2.Series.Add("MSDx")
        Chart3.Series.Add("MSDy")
        Chart4.Series.Add("nonGaussx")
        Chart4.Series.Add("nonGaussy")
        Chart2.ChartAreas(0).AxisX.IsLogarithmic = True
        Chart2.ChartAreas(0).AxisY.IsLogarithmic = True
        Chart3.ChartAreas(0).AxisX.IsLogarithmic = True
        Chart3.ChartAreas(0).AxisY.IsLogarithmic = True
        Chart4.ChartAreas(0).AxisX.IsLogarithmic = True
        Chart2.Series("MSDx").ChartType = SeriesChartType.Point
        Chart3.Series("MSDy").ChartType = SeriesChartType.Point
        Chart4.Series("nonGaussx").ChartType = SeriesChartType.Point
        Chart4.Series("nonGaussy").ChartType = SeriesChartType.Point
        Dim writer1 As New StreamWriter(StartPath & "\MSD" & TextBox1.Text & ".dat")
        Dim writer2 As New StreamWriter(StartPath & "\NG" & TextBox1.Text & ".dat")
        writer1.WriteLine("Time (s)" & Chr(9) & "MSDx (um^2)" & Chr(9) & "MSDy (um^2)")
        writer2.WriteLine("Time (s)" & Chr(9) & "NonGaussx" & Chr(9) & "NonGaussy")
        For i = 0 To maxnlagtime - 1
            Chart2.Series("MSDx").Points.AddXY(listlag(i) / framerate, MSDX(i))
            Chart3.Series("MSDy").Points.AddXY(listlag(i) / framerate, MSDY(i))
            Chart4.Series("nonGaussx").Points.AddXY(listlag(i) / framerate, nonGausx(i))
            Chart4.Series("nonGaussy").Points.AddXY(listlag(i) / framerate, nonGausy(i))
            writer1.WriteLine(listlag(i) / framerate & Chr(9) & MSDX(i) & Chr(9) & MSDY(i))
            writer2.WriteLine(listlag(i) / framerate & Chr(9) & nonGausx(i) & Chr(9) & nonGausy(i))
        Next
        'Label2.Text = "aca3"
        writer1.Close()
        writer2.Close()
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        BackgroundWorker1.CancelAsync()
    End Sub
End Class