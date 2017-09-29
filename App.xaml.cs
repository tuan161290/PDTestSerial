using Microsoft.EntityFrameworkCore;
using PDTestSerial.Model;
using PDTestSerial.Servo;
using PDTestSerial.UCT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Devices.SerialCommunication;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PDTestSerial
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        internal static SerialDevice UCTSerialDevice;
        internal static SerialDevice ServoSerialDevice;
        internal static SerialDevice GPIOSerialDevice;
        internal static UCTControl UCTCOM = null;
        internal static ServoControl ServoCOM = null;
        internal static GPIOControl GPIOCOM = null;
        internal static GPIOBoard GPIOBoardF0 = new GPIOBoard()
        {
            GPIOStation = 0xF0,
            GPIOPins = new ObservableCollection<GPIOPin>()
            {
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO0, GPIODesciption = "PD_PACK_00", GPIOLabel = "OUF0_00"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO1, GPIODesciption = "PD_PACK_01", GPIOLabel = "OUF0_01"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO2, GPIODesciption = "PD_PACK_02", GPIOLabel = "OUF0_02"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO3, GPIODesciption = "PD_PACK_03", GPIOLabel = "OUF0_03"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO4, GPIODesciption = "PD_PACK_04", GPIOLabel = "OUF0_04"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO5, GPIODesciption = "PD_PACK_05", GPIOLabel = "OUF0_05"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO6, GPIODesciption = "PD_PACK_06", GPIOLabel = "OUF0_06"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO7, GPIODesciption = "PD_PACK_07", GPIOLabel = "OUF0_07"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO8, GPIODesciption = "LEAK_PACK_01", GPIOLabel = "OUF0_08"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO9, GPIODesciption = "LEAK_PACK_02", GPIOLabel = "OUF0_09"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO10, GPIODesciption = "LEAK_PACK_11", GPIOLabel = "OUF0_10"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO11, GPIODesciption = "LEAK_PACK_12", GPIOLabel = "OUF0_11"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO12, GPIODesciption = "NFC_PACK_01", GPIOLabel = "OUF0_12"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO13, GPIODesciption = "NFC_PACK_02", GPIOLabel = "OUF0_13"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO14, GPIODesciption = "NFC_PACK_11", GPIOLabel = "OUF0_14"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO15, GPIODesciption = "NFC_PACK_12", GPIOLabel = "OUF0_15"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO16, GPIODesciption = "LEAK_01_PRESS_UP_DOWN", GPIOLabel = "OUF0_16"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO17, GPIODesciption = "LEAK_01_TRANS_FO_RV", GPIOLabel = "OUF0_17"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO18, GPIODesciption = "LEAK_02_PRESS_UP_DOWN", GPIOLabel = "OUF0_18"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO19, GPIODesciption = "LEAK_02_TRANS_FO_RV", GPIOLabel = "OUF0_19"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO20, GPIODesciption = "LEAK_11_PRESS_UP_DOWN", GPIOLabel = "OUF0_20"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO21, GPIODesciption = "LEAK_11_TRANS_FO_RV", GPIOLabel = "OUF0_21"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO22, GPIODesciption = "LEAK_12_PRESS_UP_DOWN", GPIOLabel = "OUF0_22"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO23, GPIODesciption = "LEAK_12_TRANS_FO_RV", GPIOLabel = "OUF0_23"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO24, GPIODesciption = "TVOC_START", GPIOLabel = "OUF0_24"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO25, GPIODesciption = "TVOC_UP_DOWN", GPIOLabel = "OUF0_25"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO26, GPIODesciption = "RED_LIGHT", GPIOLabel = "OUF0_26"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO27, GPIODesciption = "ORANGE_LIGHT", GPIOLabel = "OUF0_27"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO28, GPIODesciption = "GREEN_LIGHT", GPIOLabel = "OUF0_28"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO29, GPIODesciption = "BUZZER", GPIOLabel = "OUF0_29"}

            }
        };
        internal static GPIOBoard GPIOBoardF1 = new GPIOBoard() { GPIOStation = 0xF1 };
        internal static GPIOBoard GPIOBoardF2 = new GPIOBoard()
        {
            GPIOStation = 0xF2,
            GPIOPins = new ObservableCollection<GPIOPin>()
            {
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO0, GPIODesciption = "LEAK_01_OK", GPIOLabel = "INF2_00"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO1, GPIODesciption = "LEAK_01_NG", GPIOLabel = "INF2_01"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO2, GPIODesciption = "LEAK_02_OK", GPIOLabel = "INF2_02"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO3, GPIODesciption = "LEAK_02_NG", GPIOLabel = "INF2_03"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO4, GPIODesciption = "LEAK_11_OK", GPIOLabel = "INF2_04"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO5, GPIODesciption = "LEAK_11_NG", GPIOLabel = "INF2_05"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO6, GPIODesciption = "LEAK_12_NG", GPIOLabel = "INF2_06"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO7, GPIODesciption = "LEAK_12_OK", GPIOLabel = "INF2_07"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO8, GPIODesciption = "NFC_01_OK", GPIOLabel = "INF2_08"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO9, GPIODesciption = "NFC_01_NG", GPIOLabel = "INF2_09"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO10, GPIODesciption = "NFC_02_OK", GPIOLabel = "INF2_10"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO11, GPIODesciption = "NFC_02_NG", GPIOLabel = "INF2_11"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO12, GPIODesciption = "NFC_11_OK", GPIOLabel = "INF2_12"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO13, GPIODesciption = "NFC_11_NG", GPIOLabel = "INF2_13"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO14, GPIODesciption = "NFC_12_OK", GPIOLabel = "INF2_14"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO15, GPIODesciption = "NFC_12_NG", GPIOLabel = "INF2_15"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO16, GPIODesciption = "LEAK_01_UP", GPIOLabel = "INF2_16"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO17, GPIODesciption = "LEAK_01_DOWN", GPIOLabel = "INF2_17"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO18, GPIODesciption = "LEAK_01_INSIDE", GPIOLabel = "INF2_18"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO19, GPIODesciption = "LEAK_01_OUTSIDE", GPIOLabel = "INF2_19"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO20, GPIODesciption = "LEAK_02_UP", GPIOLabel = "INF2_20"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO21, GPIODesciption = "LEAK_02_DOWN", GPIOLabel = "INF2_21"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO22, GPIODesciption = "LEAK_02_INSIDE", GPIOLabel = "INF2_22"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO23, GPIODesciption = "LEAK_02_OUTSIDE", GPIOLabel = "INF2_23"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO24, GPIODesciption = "LEAK_11_UP", GPIOLabel = "INF2_24"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO25, GPIODesciption = "LEAK_11_DOWN", GPIOLabel = "INF2_25"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO26, GPIODesciption = "LEAK_11_INSIDE", GPIOLabel = "INF2_26"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO27, GPIODesciption = "LEAK_11_OUTSIDE", GPIOLabel = "INF2_27"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO28, GPIODesciption = "LEAK_12_UP", GPIOLabel = "INF2_28"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO29, GPIODesciption = "LEAK_12_DOWN", GPIOLabel = "INF2_29"}
            }
        };
        internal static GPIOBoard GPIOBoardF3 = new GPIOBoard()
        {
            GPIOStation = 0xF3,
            GPIOPins = new ObservableCollection<GPIOPin>()
            {

                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO0, GPIODesciption = "LEAK_12_INSIDE", GPIOLabel = "INF3_00"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO1, GPIODesciption = "LEAK_12_OUTSIDE", GPIOLabel = "INF3_01"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO2, GPIODesciption = "TVOC_OK", GPIOLabel = "INF3_02"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO3, GPIODesciption = "TVOC_NG", GPIOLabel = "INF3_03"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO4, GPIODesciption = "VCUM1_SWITCH", GPIOLabel = "INF3_04"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO5, GPIODesciption = "VCUM2_SWITCH", GPIOLabel = "INF3_05"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO6, GPIODesciption = "PP1_UP", GPIOLabel = "INF3_06"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO7, GPIODesciption = "PP1_DOWN", GPIOLabel = "INF3_07"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO8, GPIODesciption = "PP2_UP", GPIOLabel = "INF3_08"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO9, GPIODesciption = "PP2_DOWN", GPIOLabel = "INF3_09"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO10, GPIODesciption = "RO_CYLINDER_0", GPIOLabel = "INF3_10"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO11, GPIODesciption = "RO_CYLINDER_90", GPIOLabel = "INF3_11"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO12, GPIODesciption = "INPUT_CV_SENSOR", GPIOLabel = "INF3_12"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO13, GPIODesciption = "TRANSFER_CV_SENSOR_BEGIN", GPIOLabel = "INF3_13"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO14, GPIODesciption = "TRANSFER_CV_SENSOR_END", GPIOLabel = "INF3_14"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO15, GPIODesciption = "NG_CV_SENSOR", GPIOLabel = "INF3_15"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO16, GPIODesciption = "OUTPUT_CV_SENSOR", GPIOLabel = "INF3_16"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO17, GPIODesciption = "DOOR1_SENSOR", GPIOLabel = "INF3_17"},
                new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO18, GPIODesciption = "DOOR2_SENSOR", GPIOLabel = "INF3_18"},
                //new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO19, GPIODesciption = "LEAK_01_OUTSIDE"},
                //new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO20, GPIODesciption = "LEAK_02_UP"},
                //new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO21, GPIODesciption = "LEAK_02_DOWN"},
                //new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO22, GPIODesciption = "LEAK_02_INSIDE"},
                //new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO23, GPIODesciption = "LEAK_02_OUTSIDE"},
                //new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO24, GPIODesciption = "LEAK_11_UP"},
                //new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO25, GPIODesciption = "LEAK_11_DOWN"},
                //new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO26, GPIODesciption = "LEAK_11_INSIDE"},
                //new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO27, GPIODesciption = "LEAK_11_OUTSIDE"},
                //new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO28, GPIODesciption = "LEAK_12_UP"},
                //new GPIOPin(){GPIOBitmask = GPIOBitmask.GPIO29, GPIODesciption = "LEAK_12_DOWN"}
            }
        };
        #region DefineINPUT
        internal static GPIOPin LEAK_01_OK = GPIOBoardF2.GPIOPins[0];
        internal static GPIOPin LEAK_01_NG = GPIOBoardF2.GPIOPins[1];
        internal static GPIOPin LEAK_02_OK = GPIOBoardF2.GPIOPins[2];
        internal static GPIOPin LEAK_02_NG = GPIOBoardF2.GPIOPins[3];
        internal static GPIOPin LEAK_11_OK = GPIOBoardF2.GPIOPins[4];
        internal static GPIOPin LEAK_11_NG = GPIOBoardF2.GPIOPins[5];
        internal static GPIOPin LEAK_12_OK = GPIOBoardF2.GPIOPins[6];
        internal static GPIOPin LEAK_12_NG = GPIOBoardF2.GPIOPins[7];
        internal static GPIOPin NFC_01_OK = GPIOBoardF2.GPIOPins[8];
        internal static GPIOPin NFC_01_NG = GPIOBoardF2.GPIOPins[9];
        internal static GPIOPin NFC_02_OK = GPIOBoardF2.GPIOPins[10];
        internal static GPIOPin NFC_02_NG = GPIOBoardF2.GPIOPins[11];
        internal static GPIOPin NFC_11_OK = GPIOBoardF2.GPIOPins[12];
        internal static GPIOPin NFC_11_NG = GPIOBoardF2.GPIOPins[13];
        internal static GPIOPin NFC_12_OK = GPIOBoardF2.GPIOPins[14];
        internal static GPIOPin NFC_12_NG = GPIOBoardF2.GPIOPins[15];
        internal static GPIOPin LEAK_01_UP = GPIOBoardF2.GPIOPins[16];
        internal static GPIOPin LEAK_01_DOWN = GPIOBoardF2.GPIOPins[17];
        internal static GPIOPin LEAK_01_INSIDE = GPIOBoardF2.GPIOPins[18];
        internal static GPIOPin LEAK_01_OUTSIDE = GPIOBoardF2.GPIOPins[19];
        internal static GPIOPin LEAK_02_UP = GPIOBoardF2.GPIOPins[20];
        internal static GPIOPin LEAK_02_DOWN = GPIOBoardF2.GPIOPins[21];
        internal static GPIOPin LEAK_02_INSIDE = GPIOBoardF2.GPIOPins[22];
        internal static GPIOPin LEAK_02_OUTSIDE = GPIOBoardF2.GPIOPins[23];
        internal static GPIOPin LEAK_11_UP = GPIOBoardF2.GPIOPins[24];
        internal static GPIOPin LEAK_11_DOWN = GPIOBoardF2.GPIOPins[25];
        internal static GPIOPin LEAK_11_INSIDE = GPIOBoardF2.GPIOPins[26];
        internal static GPIOPin LEAK_11_OUTSIDE = GPIOBoardF2.GPIOPins[27];
        internal static GPIOPin LEAK_12_UP = GPIOBoardF2.GPIOPins[28];
        internal static GPIOPin LEAK_12_DOWN = GPIOBoardF2.GPIOPins[29];
        //
        internal static GPIOPin LEAK_12_INSIDE = GPIOBoardF3.GPIOPins[0];
        internal static GPIOPin LEAK_12_OUTSIDE = GPIOBoardF3.GPIOPins[1];
        internal static GPIOPin TVOC_OK = GPIOBoardF3.GPIOPins[2];
        internal static GPIOPin TVOC_NG = GPIOBoardF3.GPIOPins[3];
        internal static GPIOPin VCUM1_SWITCH = GPIOBoardF3.GPIOPins[4];
        internal static GPIOPin VCUM2_SWITCH = GPIOBoardF3.GPIOPins[5];
        internal static GPIOPin PP1_UP = GPIOBoardF3.GPIOPins[6];
        internal static GPIOPin PP1_DOWN = GPIOBoardF3.GPIOPins[7];
        internal static GPIOPin PP2_UP = GPIOBoardF3.GPIOPins[8];
        internal static GPIOPin PP2_DOWN = GPIOBoardF3.GPIOPins[9];
        internal static GPIOPin RO_CYLINDER_0 = GPIOBoardF3.GPIOPins[10];
        internal static GPIOPin RO_CYLINDER_90 = GPIOBoardF3.GPIOPins[11];
        internal static GPIOPin INPUT_CV_SENSOR = GPIOBoardF3.GPIOPins[12];
        internal static GPIOPin TRANSFER_CV_SENSOR_BEGIN = GPIOBoardF3.GPIOPins[13];
        internal static GPIOPin TRANSFER_CV_SENSOR_END = GPIOBoardF3.GPIOPins[14];
        internal static GPIOPin NG_CV_SENSOR = GPIOBoardF3.GPIOPins[15];
        internal static GPIOPin OUTPUT_CV_SENSOR = GPIOBoardF3.GPIOPins[16];
        internal static GPIOPin DOOR1_SENSOR = GPIOBoardF3.GPIOPins[17];
        internal static GPIOPin DOOR2_SENSOR = GPIOBoardF3.GPIOPins[18];
        #endregion
        #region define OUTPUT
        internal static GPIOPin PD_PACK_01 = GPIOBoardF0.GPIOPins[0];
        internal static GPIOPin PD_PACK_02 = GPIOBoardF0.GPIOPins[1];
        internal static GPIOPin PD_PACK_03 = GPIOBoardF0.GPIOPins[2];
        internal static GPIOPin PD_PACK_04 = GPIOBoardF0.GPIOPins[3];
        internal static GPIOPin PD_PACK_05 = GPIOBoardF0.GPIOPins[4];
        internal static GPIOPin PD_PACK_06 = GPIOBoardF0.GPIOPins[5];
        internal static GPIOPin PD_PACK_07 = GPIOBoardF0.GPIOPins[6];
        internal static GPIOPin PD_PACK_08 = GPIOBoardF0.GPIOPins[7];
        internal static GPIOPin LEAK_PACK_01 = GPIOBoardF0.GPIOPins[8];
        internal static GPIOPin LEAK_PACK_02 = GPIOBoardF0.GPIOPins[9];
        internal static GPIOPin LEAK_PACK_11 = GPIOBoardF0.GPIOPins[10];
        internal static GPIOPin LEAK_PACK_12 = GPIOBoardF0.GPIOPins[11];
        internal static GPIOPin NFC_PACK_01 = GPIOBoardF0.GPIOPins[12];
        internal static GPIOPin NFC_PACK_02 = GPIOBoardF0.GPIOPins[13];
        internal static GPIOPin NFC_PACK_11 = GPIOBoardF0.GPIOPins[14];
        internal static GPIOPin NFC_PACK_12 = GPIOBoardF0.GPIOPins[15];
        internal static GPIOPin LEAK_01_PRESS_UP_DOWN = GPIOBoardF0.GPIOPins[16];
        internal static GPIOPin LEAK_01_TRANS_FO_RV = GPIOBoardF0.GPIOPins[17];
        internal static GPIOPin LEAK_02_PRESS_UP_DOWN = GPIOBoardF0.GPIOPins[18];
        internal static GPIOPin LEAK_02_TRANS_FO_RV = GPIOBoardF0.GPIOPins[19];
        internal static GPIOPin LEAK_11_PRESS_UP_DOWN = GPIOBoardF0.GPIOPins[20];
        internal static GPIOPin LEAK_11_TRANS_FO_RV = GPIOBoardF0.GPIOPins[21];
        internal static GPIOPin LEAK_12_PRESS_UP_DOWN = GPIOBoardF0.GPIOPins[22];
        internal static GPIOPin LEAK_12_TRANS_FO_RV = GPIOBoardF0.GPIOPins[23];
        internal static GPIOPin TVOC_START = GPIOBoardF0.GPIOPins[24];
        internal static GPIOPin TVOC_UP_DOWN = GPIOBoardF0.GPIOPins[25];
        internal static GPIOPin RED_LIGHT = GPIOBoardF0.GPIOPins[26];
        internal static GPIOPin ORANGE_LIGHT = GPIOBoardF0.GPIOPins[27];
        internal static GPIOPin GREEN_LIGHT = GPIOBoardF0.GPIOPins[28];
        internal static GPIOPin BUZZER = GPIOBoardF0.GPIOPins[29];
        #endregion


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            using (var db = new SettingContext())
            {
                db.Database.Migrate();
            }
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }
    }
}
