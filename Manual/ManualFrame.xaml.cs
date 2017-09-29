using PDTestSerial.Model;
using PDTestSerial.Servo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PDTestSerial.Manual
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManualFrame : Page, INotifyPropertyChanged
    {
        ObservableCollection<JigModel> JigModels { get; set; }
        CancellationTokenSource cancellationTokenSource = null;
        public ManualFrame()
        {
            this.InitializeComponent();
            App.GPIOBoardF0.OnPinValueChanged += OUTPUT0_OnPinValueChanged;
        }

        private void OUTPUT0_OnPinValueChanged(object sender, GPIOBoard.PinValueChangedEventArgs e)
        {

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            using (SettingContext Db = new SettingContext())
            {
                JigModels = new ObservableCollection<JigModel>(Db.JigModels.ToList());
                Bindings.Update();
                foreach (JigModel J in JigModels)
                {
                    J.PropertyChanged += JigModel_PropertyChanged;
                }

            }
            CancelRunningTask(); //cancel running task if exist just in case
            await App.ServoCOM.StepGetdata(0x01, Flag.StepEnable, new List<byte>() { 0x01 });
            ExecuteLoopAsync();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            CancelRunningTask();
            base.OnNavigatingFrom(e);
        }

        private async void ExecuteLoopAsync()
        {
            try
            {
                PDPositions.IsEnabled = true;
                cancellationTokenSource = new CancellationTokenSource();
                while (true)
                {
                    await Loop(cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException Tce)
            {
                string s = Tce.Message;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }
        bool MovePosEnable = true;
        private async Task Loop(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            bool LeftButtonClicked = false, RightButtonClicked = false;
            byte[] currentPos = await App.ServoCOM.StepGetdata(0x01, Flag.GetActualPosition, null);
            if (currentPos != null)
            {
                CurrentPos = BitConverter.ToInt32(currentPos, 5);
            }
            else CurrentPos = 0;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                LeftButtonClicked = LeftButton.IsPressed;
                RightButtonClicked = RightButton.IsPressed;
            });
            if (LeftButtonClicked)
            {
                MovePosEnable = false;
                await App.ServoCOM.StepGetdata(0x01, Flag.MoveVelocity, DataFrame.MoveVelocityData(20000, 1));
            }
            else if (RightButtonClicked)
            {
                MovePosEnable = false;
                await App.ServoCOM.StepGetdata(0x01, Flag.MoveVelocity, DataFrame.MoveVelocityData(20000, 0));
            }
            else if (MovePosEnable == false) await App.ServoCOM.StepGetdata(0x01, Flag.MoveStop, null);
            //await Task.Delay(50);
        }

        private void CancelRunningTask()
        {
            if (cancellationTokenSource != null)
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                }
            }
        }

        private void JigModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            JigModel Sender = sender as JigModel;
            using (SettingContext Db = new SettingContext())
            {
                var SavedJigModel = Db.JigModels.Where(x => x.JigModelID == Sender.JigModelID).FirstOrDefault();
                if (SavedJigModel != null)
                {
                    SavedJigModel.JigPos = Sender.JigPos;
                    Db.SaveChanges();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        private void GetPos_DoubleClick(object sender, DoubleTappedRoutedEventArgs e)
        {
            var ClickedButton = sender as Button;
            var jigModel = ClickedButton.Tag as JigModel;
            jigModel.JigPos = CurrentPos;
        }
        private void GetPos_Click(object sender, RoutedEventArgs e)
        {

        }

        int _CurrentPos;
        public int CurrentPos
        {
            get { return _CurrentPos; }
            set { _CurrentPos = value; NotifyPropertyChanged("CurrentPos"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private async void MovePos_Click(object sender, RoutedEventArgs e)
        {
            MovePosEnable = true;
            await Task.Delay(100);
            var ClickedButton = sender as Button;
            ClickedButton.IsEnabled = false;
            var jigModel = ClickedButton.Tag as JigModel;
            await App.ServoCOM.StepGetdata(0x01, Flag.MoveSingleAxisAbs, DataFrame.MoveAbcIncData(jigModel.JigPos, 20000));
            while (await GetAxisMotioning(0x01)) ;
            ClickedButton.IsEnabled = true;
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

        private async void Pack_Click(object sender, RoutedEventArgs e)
        {
            await App.GPIOBoardF0.SetOn(App.GPIOBoardF0.GPIOPins[0]);
            //await App.GPIOCOM.GPIOWriteAsync(0xF0, Flag.SetIOOutput, BitConverter.GetBytes((int)(1 << 0)).ToList());
        }

        private async void UnPack_Click(object sender, RoutedEventArgs e)
        {
            await App.GPIOBoardF0.SetOff(App.GPIOBoardF0.GPIOPins[0]);
            //await App.GPIOCOM.GPIOWriteAsync(0xF0, Flag.SetIOOutput, BitConverter.GetBytes((int)(0 << 0)).ToList());
        }
    }
}
