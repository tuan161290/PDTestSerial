using Newtonsoft.Json;
using PDTestSerial.Manual;
using PDTestSerial.Model;
using PDTestSerial.Servo;
using PDTestSerial.UCT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409


namespace PDTestSerial
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<DeviceInformation> listOfDevices = new ObservableCollection<DeviceInformation>();
        public SWSetting SW = new SWSetting() { SWSettingID = 1 };
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public static Main Auto;
        public static Button autoButton, settingManualButton, serialSettingButton;

        public MainPage()
        {
            this.InitializeComponent();
            //MainFrame.Navigate(typeof(Manual.ManualFrame));
            Loaded += MainPage_Loaded;
            autoButton = AutoButton;
            settingManualButton = SettingManualButton;
            serialSettingButton = SerialSettingButton;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            using (var Db = new SettingContext())
            {
                #region GetSWSetting
                var SavedSWSetting = Db.SWSettings.Where(x => x.SWSettingID == 1).FirstOrDefault(); //Search for SWSetting if not found create a new one.
                if (SavedSWSetting == null)
                {
                    var sw = new SWSetting();
                    Db.Add(sw);
                    Db.SaveChanges();
                }
                else //If Saved setting is found load savedsetting
                {
                    SW = SavedSWSetting;
                    Bindings.Update();
                    Main.SW = SavedSWSetting;
                }
                #endregion               

                #region GetSavedPDJigSetting
                var SavedPD11Jig = Db.JigModels.Where(x => x.JigDesciption == "PD_11").FirstOrDefault();
                if (SavedPD11Jig == null)
                {
                    var PD11 = new JigModel() { JigDesciption = "PD_11", Channel = 1, JigID = 1 };
                    Db.Add(PD11);
                    //await Db.SaveChangesAsync();
                }
                var SavedPD12Jig = Db.JigModels.Where(x => x.JigDesciption == "PD_12").FirstOrDefault();
                if (SavedPD12Jig == null)
                {
                    var PD12 = new JigModel() { JigDesciption = "PD_12", Channel = 2, JigID = 1 };
                    Db.Add(PD12);
                    //await Db.SaveChangesAsync();
                }
                var SavedPD21Jig = Db.JigModels.Where(x => x.JigDesciption == "PD_21").FirstOrDefault();
                if (SavedPD21Jig == null)
                {
                    var PD21 = new JigModel() { JigDesciption = "PD_21", Channel = 1, JigID = 2 };
                    Db.Add(PD21);
                    //await Db.SaveChangesAsync();
                }
                var SavedPD22Jig = Db.JigModels.Where(x => x.JigDesciption == "PD_22").FirstOrDefault();
                if (SavedPD22Jig == null)
                {
                    var PD22 = new JigModel() { JigDesciption = "PD_22", Channel = 2, JigID = 2 };
                    Db.Add(PD22);
                    //await Db.SaveChangesAsync();
                }
                var SavedPD31Jig = Db.JigModels.Where(x => x.JigDesciption == "PD_31").FirstOrDefault();
                if (SavedPD31Jig == null)
                {
                    var PD31 = new JigModel() { JigDesciption = "PD_31", Channel = 1, JigID = 3 };
                    Db.Add(PD31);
                    //await Db.SaveChangesAsync();
                }
                var SavedPD32Jig = Db.JigModels.Where(x => x.JigDesciption == "PD_32").FirstOrDefault();
                if (SavedPD32Jig == null)
                {
                    var PD32 = new JigModel() { JigDesciption = "PD_32", Channel = 2, JigID = 3 };
                    Db.Add(PD32);
                    //await Db.SaveChangesAsync();
                }
                var SavedPD41Jig = Db.JigModels.Where(x => x.JigDesciption == "PD_41").FirstOrDefault();
                if (SavedPD41Jig == null)
                {
                    var PD41 = new JigModel() { JigDesciption = "PD_41", Channel = 1, JigID = 4 };
                    Db.Add(PD41);
                    //await Db.SaveChangesAsync();
                }
                var SavedPD42Jig = Db.JigModels.Where(x => x.JigDesciption == "PD_42").FirstOrDefault();
                if (SavedPD42Jig == null)
                {
                    var PD42 = new JigModel() { JigDesciption = "PD_42", Channel = 2, JigID = 4 };
                    Db.Add(PD42);
                }
                await Db.SaveChangesAsync();
                #endregion
                #region GetSerialDeviceID
                var UCTPortID = Db.ValueSettings.Where(x => x.Key == "UCTPortID").FirstOrDefault();
                var ServoPortID = Db.ValueSettings.Where(x => x.Key == "ServoPortID").FirstOrDefault();
                var GPIOPortID = Db.ValueSettings.Where(x => x.Key == "GPIOPortID").FirstOrDefault();
                if (UCTPortID != null && ServoPortID != null && GPIOPortID != null)
                {
                    try
                    {
                        App.UCTSerialDevice = await SerialDevice.FromIdAsync(UCTPortID.Value);
                        App.ServoSerialDevice = await SerialDevice.FromIdAsync(ServoPortID.Value);
                        App.GPIOSerialDevice = await SerialDevice.FromIdAsync(GPIOPortID.Value);

                        if (App.UCTSerialDevice == null || App.ServoSerialDevice == null || App.GPIOSerialDevice == null)
                        {
                            await SettingDialog.ShowAsync();
                        }
                        else
                        {
                            App.UCTCOM = new UCTControl(ref App.UCTSerialDevice);
                            App.ServoCOM = new ServoControl(ref App.ServoSerialDevice);
                            App.GPIOCOM = new GPIOControl(ref App.GPIOSerialDevice);
                        }
                        MainFrame.Navigate(typeof(Auto));
                        return;
                    }
                    catch (Exception)
                    {
                        Setting_Clicked(null, null);
                        return;
                    }
                }
                Setting_Clicked(null, null);
                #endregion
            }
        }

        private void COMPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SettingDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                var UctSerialPort = ListCOMPort1.SelectedItem as DeviceInformation;
                var ServoSerialPort = ListCOMPort2.SelectedItem as DeviceInformation;
                var GPIOSerialPort = ListCOMPort3.SelectedItem as DeviceInformation;
                using (var Db = new SettingContext())//Save current setting to SQLite DB
                {
                    //-----------------------------------------------------------------
                    var UCTPortID = Db.ValueSettings.Where(x => x.Key == "UCTPortID").FirstOrDefault();
                    if (UCTPortID == null)
                    {
                        Db.ValueSettings.Add(new ValueSetting() { Key = "UCTPortID", Value = UctSerialPort.Id });
                        Db.SaveChanges();
                    }
                    else
                    {
                        UCTPortID.Value = UctSerialPort.Id;
                    }
                    //-----------------------------------------------------------------
                    var ServoPortID = Db.ValueSettings.Where(x => x.Key == "ServoPortID").FirstOrDefault();
                    if (ServoPortID == null)
                    {
                        Db.ValueSettings.Add(new ValueSetting() { Key = "ServoPortID", Value = ServoSerialPort.Id });
                        Db.SaveChanges();
                    }
                    else
                    {
                        ServoPortID.Value = ServoSerialPort.Id;
                    }
                    //-----------------------------------------------------------------
                    var GPIOPortID = Db.ValueSettings.Where(x => x.Key == "GPIOPortID").FirstOrDefault();
                    if (GPIOPortID == null)
                    {
                        Db.ValueSettings.Add(new ValueSetting() { Key = "GPIOPortID", Value = GPIOSerialPort.Id });
                        Db.SaveChanges();
                    }
                    else
                    {
                        GPIOPortID.Value = GPIOSerialPort.Id;
                    }
                    var SWSetting = Db.SWSettings.Where(x => x.SWSettingID == 1).FirstOrDefault();
                    if (SWSetting == null)
                    {
                        Db.SWSettings.Add(new SWSetting());
                    }
                    else
                    {
                        //SWSetting = SW;
                        //var sourProps = typeof(SWSetting).GetProperties().Where(x => x.CanRead);
                        SWSetting.UD3Test = SW.UD3Test;
                        SWSetting.UO3Test = SW.UO3Test;
                        SWSetting.UO2Test = SW.UO2Test;
                        SWSetting.LOADTest = SW.LOADTest;
                        SWSetting.PDCTest = SW.PDCTest;
                        SWSetting.VCONNTest = SW.VCONNTest;
                        SWSetting.SBUTest = SW.SBUTest;
                    }
                    Db.SaveChanges();
                }
                Main.SW = SW;
                MainFrame.Navigate(typeof(Auto));
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        private async void Try_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as Button;
            if (clickedButton == TryButton1)
            {
                if (clickedButton.Content.ToString() == "Connect")
                {
                    DeviceInformation entry = ListCOMPort1.SelectedItem as DeviceInformation;
                    if (entry == null)
                    {
                        UCTStatus.Text = "No SerialDevice selected";
                        return;
                    }
                    App.UCTSerialDevice = await SerialDevice.FromIdAsync(entry.Id);

                    if (App.UCTSerialDevice != null)
                    {
                        App.UCTCOM = new UCT.UCTControl(ref App.UCTSerialDevice);
                        UCTStatus.Text = "OK";
                        clickedButton.Content = "Disconnect";
                    }
                    else UCTStatus.Text = "Failed";
                }
                else if (clickedButton.Content.ToString() == "Disconnect")
                {
                    if (App.UCTCOM != null) //Disconnect Device
                    {
                        App.UCTCOM.Dispose();
                        App.UCTCOM = null;
                    }
                    UCTStatus.Text = "Disconnected";
                    clickedButton.Content = "Connect";
                }
            }
            if (clickedButton == TryButton2)
            {
                if (clickedButton.Content.ToString() == "Connect") //Connect button Clicked
                {
                    DeviceInformation entry = ListCOMPort2.SelectedItem as DeviceInformation;
                    if (entry == null)
                    {
                        MotorStatus.Text = "No SerialDevice selected";
                        return;
                    }
                    App.ServoSerialDevice = await SerialDevice.FromIdAsync(entry.Id);

                    if (App.ServoSerialDevice != null)
                    {
                        MotorStatus.Text = "OK";
                        App.ServoCOM = new ServoControl(ref App.ServoSerialDevice);
                        clickedButton.Content = "Disconnect";
                    }
                    else MotorStatus.Text = "Failed";
                }
                else if (clickedButton.Content.ToString() == "Disconnect") //Disconnect Button Clicked
                {

                    if (App.ServoCOM != null) //Disconnect Device
                    {
                        App.ServoCOM.Dispose();
                        App.ServoCOM = null;
                    }
                    MotorStatus.Text = "Disconnected";
                    clickedButton.Content = "Connect";
                }
            }
            if (clickedButton == TryButton3)
            {
                if (clickedButton.Content.ToString() == "Connect") //Connect button Clicked
                {
                    DeviceInformation entry = ListCOMPort3.SelectedItem as DeviceInformation;
                    if (entry == null)
                    {
                        GPIOStatus.Text = "No SerialDevice selected";
                        return;
                    }
                    App.GPIOSerialDevice = await SerialDevice.FromIdAsync(entry.Id);

                    if (App.GPIOSerialDevice != null)
                    {
                        GPIOStatus.Text = "OK";
                        App.GPIOCOM = new GPIOControl(ref App.GPIOSerialDevice);
                        clickedButton.Content = "Disconnect";
                    }
                    else GPIOStatus.Text = "Failed";
                }
                else if (clickedButton.Content.ToString() == "Disconnect") //Disconnect Button Clicked
                {

                    if (App.GPIOCOM != null) //Disconnect Device
                    {
                        App.GPIOCOM.Dispose();
                        App.GPIOCOM = null;
                    }
                    GPIOStatus.Text = "Disconnected";
                    clickedButton.Content = "Connect";
                }
            }
            if (App.ServoSerialDevice != null && App.UCTSerialDevice != null && App.GPIOSerialDevice != null)
                SettingDialog.IsPrimaryButtonEnabled = true;
            else SettingDialog.IsPrimaryButtonEnabled = false;
        }

        private async void Setting_Clicked(object sender, RoutedEventArgs e)
        {
            if (Auto != null)
                Auto.ClearExtendedExecution();
            listOfDevices.Clear();
            string aqs = SerialDevice.GetDeviceSelector();
            var dis = await DeviceInformation.FindAllAsync(aqs);
            for (int i = 0; i < dis.Count; i++)
            {
                listOfDevices.Add(dis[i]);
            }
            using (var Db = new SettingContext())
            {
                var uct = Db.ValueSettings.Where(x => x.Key == "UCTPortID").FirstOrDefault();
                if (uct != null)
                {
                    foreach (DeviceInformation deviceinfo in ListCOMPort1.Items)
                    {
                        if (deviceinfo.Id == uct.Value)
                            ListCOMPort1.SelectedItem = deviceinfo;
                    }
                    if (App.UCTCOM != null)
                        TryButton1.Content = "Disconnect";
                    else TryButton1.Content = "Connect";
                }
                var servo = Db.ValueSettings.Where(x => x.Key == "ServoPortID").FirstOrDefault();
                if (servo != null)
                {
                    foreach (DeviceInformation deviceinfo in ListCOMPort2.Items)
                    {
                        if (deviceinfo.Id == servo.Value)
                            ListCOMPort2.SelectedItem = deviceinfo;
                    }
                    if (App.ServoCOM != null)
                        TryButton2.Content = "Disconnect";
                    else TryButton2.Content = "Connect";
                }
                var gpio = Db.ValueSettings.Where(x => x.Key == "GPIOPortID").FirstOrDefault();
                if (gpio != null)
                {
                    foreach (DeviceInformation deviceinfo in ListCOMPort3.Items)
                    {
                        if (deviceinfo.Id == gpio.Value)
                            ListCOMPort3.SelectedItem = deviceinfo;
                    }
                    if (App.GPIOCOM != null)
                        TryButton3.Content = "Disconnect";
                    else TryButton3.Content = "Connect";
                }
            }
            SettingDialog.IsPrimaryButtonEnabled = false;
            if (App.ServoSerialDevice != null && App.UCTSerialDevice != null && App.GPIOSerialDevice != null)
                SettingDialog.IsPrimaryButtonEnabled = true;
            else SettingDialog.IsPrimaryButtonEnabled = false;
            SettingDialog.Visibility = Visibility.Visible;
            await SettingDialog.ShowAsync();
        }

        private async void OverlayButton_Click(object sender, RoutedEventArgs e)
        {
            var ClickedButton = sender as Button;
            if (ClickedButton == AutoButton) MainFrame.Navigate(typeof(Auto));
            if (ClickedButton == IOMonitorButton) await IOMonitorDialog.ShowAsync();
            if (ClickedButton == SettingManualButton) MainFrame.Navigate(typeof(ManualFrame));
        }
    }
}
