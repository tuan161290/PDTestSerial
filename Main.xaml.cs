using Newtonsoft.Json;
using PDTestSerial.UCT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Devices.SerialCommunication;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Primitives;
using PDTestSerial.Model;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PDTestSerial
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Main : Page, INotifyPropertyChanged
    {

        //private ObservableCollection<DeviceInformation> listOfDevices = new ObservableCollection<DeviceInformation>();

        //public static Main main;
        public static SWSetting SW { get; set; } = new SWSetting();
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        private ExtendedExecutionSession session = null;
        public static CancellationTokenSource cancelationToken = null;
        public static CancellationTokenSource ReadUCTCancelationTokenSoure = null;
        public Main()
        {
            this.InitializeComponent();
            MainPage.Auto = this;
            //object setting = localSettings.Values["UCTTestStates"];
            //if (setting != null)
            //{
            //    UCTTestStates.Clear();
            //    string s = setting.ToString();
            //    var SavedSettings = JsonConvert.DeserializeObject<ObservableCollection<UCTResult>>(s);
            //}
            var uct1 = new UCTResult() { JigID = 1, Channel1TestStatus = "FINISHED" };
            var uct2 = new UCTResult() { JigID = 2, Channel1TestStatus = "FINISHED" };
            var uct3 = new UCTResult() { JigID = 3, Channel1TestStatus = "FINISHED" };
            var uct4 = new UCTResult() { JigID = 4, Channel1TestStatus = "FINISHED" };
            var uct5 = new UCTResult() { JigID = 5, Channel1TestStatus = "FINISHED" };
            var uct6 = new UCTResult() { JigID = 6, Channel1TestStatus = "FINISHED" };
            var uct7 = new UCTResult() { JigID = 7, Channel1TestStatus = "FINISHED" };
            var uct8 = new UCTResult() { JigID = 8, Channel1TestStatus = "FINISHED" };
            UCTTestStates.Add(uct1);
            UCTTestStates.Add(uct1);
            UCTTestStates.Add(uct1);
            UCTTestStates.Add(uct1);
            UCTTestStates.Add(uct2);
            foreach (var R in UCTTestStates)
            {
                //R.PropertyChanged += R_PropertyChanged;
                R.OnTestComplete += R_OnTestComplete;
            }
            //Results.Add(new UCTResult() { JigID = 2 });
            //Results.Add(new UCTResult() { JigID = 3 });
            //Results.Add(new UCTResult() { JigID = 4 });
            //Results.Add(new UCTResult() { JigID = 5 });
            BeginExtendedExecution();
            //Init(cancelationToken.Token);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ClearExtendedExecution();
        }

        private void R_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //string s = JsonConvert.SerializeObject(UCTTestStates);
            //localSettings.Values["UCTTestStates"] = s;
        }

        private void R_OnTestComplete(object sender, TestCompleteEventArgs e)
        {
            FinalResults.Add(e.Result);
        }

        private async void BeginExtendedExecution()
        {
            // The previous Extended Execution must be closed before a new one can be requested.
            // This code is redundant here because the sample doesn't allow a new extended
            // execution to begin until the previous one ends, but we leave it here for illustration.
            ClearExtendedExecution();
            var newSession = new ExtendedExecutionSession();
            newSession.Reason = ExtendedExecutionReason.Unspecified;
            //newSession.Description = "Raising periodic toasts";
            newSession.Revoked += SessionRevoked;
            ExtendedExecutionResult result = await newSession.RequestExtensionAsync();
            if (result == ExtendedExecutionResult.Allowed)
            {
                //ExtentedExcutionTextBlock.Text = "Allowed";
                session = newSession;
                cancelationToken = new CancellationTokenSource();
                Loop(cancelationToken.Token);
                ReadUCTCancelationTokenSoure = new CancellationTokenSource();
                ListenUCT();
                //ResultProcessingLoop();
            }
            //else ExtentedExcutionTextBlock.Text = "Denined";
        }

        private async void SessionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //ExtentedExcutionTextBlock.Text = "Revoked";
                ClearExtendedExecution();
            });
        }

        public void ClearExtendedExecution()
        {
            if (session != null)
            {
                session.Revoked -= SessionRevoked;
                session.Dispose();
                session = null;
            }
            if (cancelationToken != null)
            {
                if (!cancelationToken.IsCancellationRequested)
                {
                    cancelationToken.Cancel();
                }
            }
            if (ReadUCTCancelationTokenSoure != null)
            {
                if (!ReadUCTCancelationTokenSoure.IsCancellationRequested)
                {
                    ReadUCTCancelationTokenSoure.Cancel();
                }
            }
        }

        private async void ListenUCT()
        {
            try
            {
                while (true)
                {
                    await ReadUCTStatusAsync(ReadUCTCancelationTokenSoure.Token);
                }
            }
            catch (OperationCanceledException Oce)
            {
                string s = Oce.Message;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }
        private async Task ReadUCTStatusAsync(CancellationToken UCTReadTocken)
        {
            UCTReadTocken.ThrowIfCancellationRequested();
            foreach (UCTResult UCT in UCTTestStates)
            {
                for (int Channel = 1; Channel <= 2; Channel++)
                {
                    //string TestStatus = string.Empty;
                    string TestStatus = await App.UCTCOM.CheckTestResult(UCT.JigID, Channel, UCTControl.TestRequest.STS);
                    //Get USB 3.1 Device Test Result 
                    string USB3TestResult = "IGNORE";
                    if (SW.UD3Test == true)
                    {
                        USB3TestResult = await App.UCTCOM.CheckTestResult(UCT.JigID, Channel, UCTControl.TestRequest.UD3);
                    }
                    string USB3DTestResult = "IGNORE";
                    if (SW.UO3Test == true)
                        USB3DTestResult = await App.UCTCOM.CheckTestResult(UCT.JigID, Channel, UCTControl.TestRequest.UO3);
                    //Get USB 2.0 OTG Test Result                            
                    string USB2OTGTestResult = "IGNORE";
                    if (SW.UO2Test == true)
                        USB2OTGTestResult = await App.UCTCOM.CheckTestResult(UCT.JigID, Channel, UCTControl.TestRequest.UO2);
                    ////Get PD Charge Test Result                                                             
                    string PDCharegeTestResult = "IGNORE";
                    if (SW.PDCTest == true)
                        PDCharegeTestResult = await App.UCTCOM.CheckTestResult(UCT.JigID, Channel, UCTControl.TestRequest.PDC);
                    ////Get LOAD Test Result                            
                    string LOADTestResult = "IGNORE";
                    if (SW.LOADTest == true)
                        LOADTestResult = await App.UCTCOM.CheckTestResult(UCT.JigID, Channel, UCTControl.TestRequest.LOAD);
                    ////Get VCONN Test Result                            
                    string VCONNTest = "IGNORE";
                    if (SW.VCONNTest == true)
                        VCONNTest = await App.UCTCOM.CheckTestResult(UCT.JigID, Channel, UCTControl.TestRequest.VCON);
                    //Get SBU Test Result                            
                    string SBUTestResult = "IGNORE";
                    if (SW.SBUTest == true)
                        SBUTestResult = await App.UCTCOM.CheckTestResult(UCT.JigID, Channel, UCTControl.TestRequest.SBU);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        lock (LockObject)
                        {
                            if (Channel == 1)
                                UCT.Channel1TestStatus = TestStatus;
                            if (Channel == 2)
                                UCT.Channel2TestStatus = TestStatus;
                            if (Channel == 1)
                                UCT.Channel1TestStatuses[0].TestResult = USB3TestResult;
                            if (Channel == 2)
                                UCT.Channel2TestStatuses[0].TestResult = USB3TestResult;
                            if (Channel == 1)
                                UCT.Channel1TestStatuses[1].TestResult = USB3DTestResult;
                            if (Channel == 2)
                                UCT.Channel2TestStatuses[1].TestResult = USB3DTestResult;
                            if (Channel == 1)
                                UCT.Channel1TestStatuses[2].TestResult = USB2OTGTestResult;
                            if (Channel == 2)
                                UCT.Channel2TestStatuses[2].TestResult = USB2OTGTestResult;
                            if (Channel == 1)
                                UCT.Channel1TestStatuses[3].TestResult = PDCharegeTestResult;
                            if (Channel == 2)
                                UCT.Channel2TestStatuses[3].TestResult = PDCharegeTestResult;
                            if (Channel == 1)
                                UCT.Channel1TestStatuses[4].TestResult = LOADTestResult;
                            if (Channel == 2)
                                UCT.Channel2TestStatuses[4].TestResult = LOADTestResult;
                            if (Channel == 1)
                                UCT.Channel1TestStatuses[5].TestResult = VCONNTest;
                            if (Channel == 2)
                                UCT.Channel2TestStatuses[5].TestResult = VCONNTest;
                            if (Channel == 1)
                                UCT.Channel1TestStatuses[6].TestResult = SBUTestResult;
                            if (Channel == 2)
                                UCT.Channel2TestStatuses[6].TestResult = SBUTestResult;
                        }
                    });
                }
            }
            //await Task.Delay(100);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        ObservableCollection<ChannelTestResult> FinalResults = new ObservableCollection<ChannelTestResult>();
        int TD = 0;
        public async void Loop(CancellationToken token)
        {
            if (App.ServoSerialDevice == null)
            {
                return;
            }
            ChannelTestResult Item = null;
            int STEP = 0;
            DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(50) };
            Timer.Tick += Timer_Tick;
            Timer.Start();
            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (App.ServoCOM == null)
                        {
                            return;
                        }
                        token.ThrowIfCancellationRequested();
                        if (STEP == 0 && FinalResults.Count > 0 && TD == 0)
                        {
                            Item = FinalResults.First();
                            var s = await App.ServoCOM.StepGetdata(1, Flag.MoveSingleAxisAbs, DataFrame.MoveAbcIncData(Item.Position, 50000));
                            if (s[4] == BitMask.FrameOK)
                                STEP = 1;
                        }
                        if (STEP == 1)
                        {
                            if (!await GetAxisMotioning(1))
                            {
                                STEP = 2;
                                TD = 0;
                            }
                        }
                        if (STEP == 2 && TD == 0)
                        {
                            var s = await App.ServoCOM.StepGetdata(1, Flag.SetIOOutput, DataFrame.EziSetOnBitMask(BitMask.USEROUT2));
                            if (s[4] == BitMask.FrameOK)
                            {
                                STEP = 3;
                                TD = 5;
                            }
                        }
                        if (STEP == 3 && TD == 0)
                        {
                            var s = await App.ServoCOM.StepGetdata(1, Flag.SetIOOutput, DataFrame.EziSetOffBitMask(BitMask.USEROUT2));
                            if (s[4] == BitMask.FrameOK)
                            {
                                STEP = 4;
                                TD = 5;
                            }
                        }

                        if (STEP == 4 && TD == 0)
                        {
                            var s = await App.ServoCOM.StepGetdata(1, Flag.MoveSingleAxisAbs, DataFrame.MoveAbcIncData(34000, 50000));
                            if (s[4] == BitMask.FrameOK)
                                STEP = 5;
                        }
                        if (STEP == 5)
                        {
                            if (!await GetAxisMotioning(1))
                            {
                                STEP = 6;
                                TD = 5;
                            }
                        }
                        if (STEP == 6 && TD == 0)
                        {
                            var s = await App.ServoCOM.StepGetdata(1, Flag.SetIOOutput, DataFrame.EziSetOnBitMask(BitMask.USEROUT2));
                            if (s[4] == BitMask.FrameOK)
                            {
                                STEP = 7;
                                TD = 5;
                            }
                        }
                        if (STEP == 7 && TD == 0)
                        {
                            var s = await App.ServoCOM.StepGetdata(1, Flag.SetIOOutput, DataFrame.EziSetOffBitMask(BitMask.USEROUT2));
                            if (s[4] == BitMask.FrameOK)
                            {
                                FinalResults.Remove(Item);
                                STEP = 0;
                                TD = 5;
                            }
                        }

                        if (STEP == 0 && ManualMode == true)
                        {
                            if (Left == true)
                                await App.ServoCOM.StepGetdata(1, Flag.MoveVelocity, DataFrame.MoveVelocityData(2000, 1));
                            else if (Right == true)
                                await App.ServoCOM.StepGetdata(1, Flag.MoveVelocity, DataFrame.MoveVelocityData(2000, 0));
                            else await App.ServoCOM.StepGetdata(1, Flag.MoveStop, null);
                        }
                        await Task.Delay(100);
                    }
                    catch (OperationCanceledException CanceledException)
                    {
                        string s = CanceledException.Message;
                        //if (App.ServoCOM != null)
                        //{
                        //    App.ServoCOM.Dispose();
                        //    App.ServoCOM = null;
                        //}
                        return;
                    }
                    catch (Exception ex)
                    {
                        string s = ex.Message;
                        return;
                    }
                }
            });
        }

        private async Task<bool> GetButtonStateAsync(ToggleButton button)
        {
            bool ButtonState = false;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ButtonState = button.IsPressed;
            });
            return ButtonState;
        }

        private async Task<bool> GetButtonStateAsync(Button button)
        {
            bool ButtonState = false;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ButtonState = button.IsPressed;
            });
            return ButtonState;
        }

        public bool Left;
        public bool Right;
        public bool ManualMode;


        private void Timer_Tick(object sender, object e)
        {
            if (TD > 0)
            {
                TD--;
            }
        }

        private async Task<bool> GetAxisMotioning(byte axis)
        {
            var motioning = await App.ServoCOM.StepGetdata(axis, Flag.GetAxisStatus, null);
            if (motioning != null)
            {
                uint Motioning = BitConverter.ToUInt32(motioning, 5);
                if ((BitMask.Motioning & Motioning) == 0)
                    return false;
            }
            return true;
        }

        private void SettingDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        ObservableCollection<UCTResult> UCTTestStates = new ObservableCollection<UCTResult>();
        Object LockObject = new Object();

        private void ListCOMPort1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void COMPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void ShowMessage(string message)
        {
            var MessageBox = new MessageDialog(message);
            await MessageBox.ShowAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void SONButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
