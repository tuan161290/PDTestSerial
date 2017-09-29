using PDTestSerial.UCT;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Primitives;
using PDTestSerial.Model;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PDTestSerial
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Auto : Page, INotifyPropertyChanged
    {
        public static SWSetting SW { get; set; } = new SWSetting();

        private ExtendedExecutionSession session = null;
        public static CancellationTokenSource cancelationToken = null;
        public static CancellationTokenSource ReadTestCancelationTokenSoure = null;

        ObservableCollection<PDJig> JigModelsFront = null;
        ObservableCollection<UCTResult> UCTTestStates = new ObservableCollection<UCTResult>();
        object LockObject = new object();

        public Auto()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            using (SettingContext Db = new SettingContext())
            {
                var jigModelsFront = Db.JigModels.Where(x => x.JigModelID > 0 && x.JigModelID <= 11).ToList();
                var PD11 = new PDJig(App.PD_PACK_01, jigModelsFront[0]);
                var PD12 = new PDJig(App.PD_PACK_02, jigModelsFront[1]);
                var PD21 = new PDJig(App.PD_PACK_03, jigModelsFront[2]);
                var PD22 = new PDJig(App.PD_PACK_04, jigModelsFront[3]);
                var PD31 = new PDJig(App.PD_PACK_05, jigModelsFront[4]);
                var PD32 = new PDJig(App.PD_PACK_06, jigModelsFront[5]);
                var PD41 = new PDJig(App.PD_PACK_07, jigModelsFront[6]);
                var PD42 = new PDJig(App.PD_PACK_08, jigModelsFront[7]);
                JigModelsFront = new ObservableCollection<PDJig>() { PD11, PD12, PD21, PD22, PD31, PD32, PD41, PD42 };
                FrontJigGridView.ItemsSource = JigModelsFront.Where(x => x.Jig.JigModelID >= 0 && x.Jig.JigModelID <= 8);
            }
            StopButton_Click(null, null);
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
                session = newSession;
                //------------------------Auto Loop----------------------------
                cancelationToken = new CancellationTokenSource();
                Loop(cancelationToken.Token);
                ListenUCT();
            }
        }

        private async void SessionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
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
            if (ReadTestCancelationTokenSoure != null)
            {
                if (!ReadTestCancelationTokenSoure.IsCancellationRequested)
                {
                    ReadTestCancelationTokenSoure.Cancel();
                }
            }
        }

        private async void ListenUCT()
        {
            try
            {
                ReadTestCancelationTokenSoure = new CancellationTokenSource();
                while (true)
                {
                    await ReadTestStatus(ReadTestCancelationTokenSoure.Token);
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

        List<JigModel> finalResult = new List<JigModel>();

        private async Task ReadTestStatus(CancellationToken UCTReadTocken)
        {
            UCTReadTocken.ThrowIfCancellationRequested();
            int index = 0;
            for (int JigNo = 1; JigNo <= 4; JigNo++)
            {
                for (int JigChannel = 1; JigChannel <= 2; JigChannel++)
                {
                    string s = await App.UCTCOM.GetTestStatus(JigNo, JigChannel);
                    JigModelsFront[index].Jig.JigState = s;
                    if (JigModelsFront[index].Jig.JigState != JigModelsFront[index].Jig.PreviousJigState)
                    {
                        if (JigModelsFront[index].Jig.JigState == "FINISHED")
                        {
                            var Jig = JigModelsFront[index];
                            //finalResult.Add(Jig);

                        };
                    }
                    JigModelsFront[index].Jig.PreviousJigState = JigModelsFront[index].Jig.JigState;
                    index++;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            StopButton_Click(null, null);
        }

        private async void Abort_Click(object sender, RoutedEventArgs e)
        {
            var ClickedButton = sender as Button;
            var JigModel = ClickedButton.Tag as JigModel;
            var TestButton = GetChildren(FrontJigGridView).Where(x => x.Name == JigModel.JigDesciption).First() as Button;
            TestButton.IsEnabled = false;
            //var OutputBitMask = JigModel.OutPutBitmask;
            await App.UCTCOM.TestCommand(JigModel.JigID, JigModel.Channel, 0);
            //await App.ServoCOM.StepGetdata(0x01, Flag.SetIOOutput, DataFrame.EziSetOffBitMask(OutputBitMask));
            await Task.Delay(4000);
            TestButton.IsEnabled = true;
            //await App.UCTCOM.TestCommand(JigModel.JigID, JigModel.Channel, 1);
        }
        private async void Test_Click(object sender, RoutedEventArgs e)
        {
            var ClickedButton = sender as Button;
            var JigModel = ClickedButton.Tag as JigModel;
            //var OutputBitMask = JigModel.OutPutBitmask;
            await App.UCTCOM.TestCommand(JigModel.JigID, JigModel.Channel, 1);
            //await App.ServoCOM.StepGetdata(0x01, Flag.SetIOOutput, DataFrame.EziSetOnBitMask(OutputBitMask));
        }
        private List<FrameworkElement> GetChildren(DependencyObject parent)
        {
            List<FrameworkElement> controls = new List<FrameworkElement>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); ++i)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement)
                {
                    controls.Add(child as FrameworkElement);
                }
                controls.AddRange(GetChildren(child));
            }
            return controls;
        }

        int TD = 0;
        public async void Loop(CancellationToken token)
        {
            if (App.ServoSerialDevice == null)
            {
                return;
            }
            DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
            Timer.Tick += Timer_Tick;
            Timer.Start();
            try
            {

                await LoopExecution(token);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }

        }

        private async Task LoopExecution(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                await App.GPIOBoardF2.GetGPIOsState();
                //await Task.Delay(200);
                //await App.GPIOBoardF0.SetOn(App.GPIOBoardF0.PIN0);
                //await App.GPIOBoardF0.SetOff(App.GPIOBoardF0.PIN0);
                //await App.GPIOBoardF0.BSetOff(0, 1);

            }
        }

        private void Timer_Tick(object sender, object e)
        {
            if (TD > 0)
            {
                TD--;
            }
            DateTimeTextblock.Text = DateTime.Now.ToString();
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
        private void Button_Click(object sender, RoutedEventArgs e)
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

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            int index = 0;
            for (int JigNo = 1; JigNo <= 4; JigNo++)
            {
                for (int JigChannel = 1; JigChannel <= 2; JigChannel++)
                {
                    index++;
                    string s = await App.UCTCOM.TestCommand(JigNo, JigChannel, 1);
                }
            }
            ResetButton.IsEnabled = false;
            ORGButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            BeginExtendedExecution();
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ClearExtendedExecution();
            StopButton.IsEnabled = false;
            StartButton.IsEnabled = false;
            int index = 0;
            for (int JigNo = 1; JigNo <= 4; JigNo++)
            {
                for (int JigChannel = 1; JigChannel <= 2; JigChannel++)
                {
                    index++;
                    string s = await App.UCTCOM.TestCommand(JigNo, JigChannel, 0);
                }
            }
            await Task.Delay(2000);
            StartButton.IsEnabled = true;
            ResetButton.IsEnabled = true;
            ORGButton.IsEnabled = true;
        }


    }
}
