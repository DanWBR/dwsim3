Imports Cudafy

Namespace My

    ' The following events are availble for MyApplication:
    ' 
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication

        Public Shared _ResourceManager As System.Resources.ResourceManager
        Public Shared _HelpManager As System.Resources.ResourceManager
        Public Shared _PropertyNameManager As System.Resources.ResourceManager
        Public Shared _CultureInfo As System.Globalization.CultureInfo
        Public Shared CalculatorStopRequested As Boolean = False
        Public Shared CommandLineMode As Boolean = False
        
        Public Shared IsRunningParallelTasks As Boolean = False
        Public Shared IsFlowsheetSolving As Boolean = False
        
        Public ActiveSimulation As FormFlowsheet
        Public CAPEOPENMode As Boolean = False
        Public Shared gpu As Cudafy.Host.GPGPU
        Public Shared gpumod As CudafyModule
        Public Shared prevlang As Integer = 0 '0 = CUDA, 1 = OpenCL

        Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup

            'upgrade settings from previous build, if applicable.
            If My.Settings.UpgradeRequired Then
                My.Settings.Upgrade()
                My.Settings.UpgradeRequired = False
            End If

            'check if the user wants to reset settings.
            If My.Computer.Keyboard.ShiftKeyDown Then
                My.Settings.Reset()
                MessageBox.Show("The settings were reset successfully.")
            End If

            'loads the current language
            _CultureInfo = New Globalization.CultureInfo(My.Settings.CultureInfo)
            My.Application.ChangeUICulture(My.Settings.CultureInfo)

            'loads the resource manager
            _ResourceManager = New System.Resources.ResourceManager("DWSIM.DWSIM", System.Reflection.Assembly.GetExecutingAssembly())

            'loads the help manager
            _HelpManager = New System.Resources.ResourceManager("DWSIM.Help", System.Reflection.Assembly.GetExecutingAssembly())

            'loads the property name manager
            _PropertyNameManager = New System.Resources.ResourceManager("DWSIM.Properties", System.Reflection.Assembly.GetExecutingAssembly())

            For Each s As String In My.Application.CommandLineArgs
                If s.ToLower = "-commandline" Then
                    'Stop the start form from loading.
                    e.Cancel = True
                    CommandLineMode = True
                End If
                If s.ToLower = "-locale" Then
                    Dim clcult As String = My.Application.CommandLineArgs(My.Application.CommandLineArgs.IndexOf(s) + 1)
                    _CultureInfo = New Globalization.CultureInfo(clcult)
                    My.Application.ChangeUICulture(clcult)
                End If
            Next

            If e.Cancel Then
                ' Call the main routine for windowless operation.
                Dim f1 As New FormMain
            End If

            'direct console output to collection
            If My.Settings.RedirectOutput And Not CommandLineMode Then
                Dim txtwriter As New ConsoleRedirection.TextBoxStreamWriter()
                Console.SetOut(txtwriter)
            End If

            'set CUDA params
            CudafyModes.Compiler = eGPUCompiler.All
            CudafyModes.Target = My.Settings.CudafyTarget

        End Sub

        Private Sub MyApplication_UnhandledException(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            Dim frmEx As New FormUnhandledException
            frmEx.TextBox1.Text = e.Exception.ToString
            frmEx.ex = e.Exception
            frmEx.ShowDialog()
            e.ExitApplication = False
        End Sub

    End Class

End Namespace
