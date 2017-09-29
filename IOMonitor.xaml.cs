using PDTestSerial.Servo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PDTestSerial
{
    public sealed partial class IOMonitor : UserControl
    {
        ObservableCollection<GPIOPin> OUTPUT0 = App.GPIOBoardF0.GPIOPins;
        ObservableCollection<GPIOPin> OUTPUT1 = App.GPIOBoardF1.GPIOPins;
        ObservableCollection<GPIOPin> INPUT0 = App.GPIOBoardF2.GPIOPins;
        ObservableCollection<GPIOPin> INPUT1 = App.GPIOBoardF3.GPIOPins;
        public IOMonitor()
        {

            this.InitializeComponent();
        }



        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
