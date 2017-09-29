using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;

namespace PDTestSerial.Servo
{
    public class GPIOBoard
    {
        public GPIOBoard(/*ObservableCollection<GPIOPin> gpioPins*/)
        {
            //GPIOPins = gpioPins;
        }
        private int _GPIORegister;
        public int GPIORegister
        {
            get { return _GPIORegister; }
            set
            {
                _GPIORegister = value;
                int index = 0;
                foreach (GPIOPin GP in GPIOPins)
                {

                    GP.PinValue = ((_GPIORegister & GP.GPIOBitmask) > 0) ? PinValue.ON : PinValue.OFF;
                    index++;
                    if (GP.PinValue != GP.PrePinState)
                    {
                        if (GP.PinValue == PinValue.ON)
                        {
                            NotifyPinValueChanged(GP, Edge.Rise);
                        }
                        else
                        {
                            NotifyPinValueChanged(GP, Edge.Fall);
                        }
                    }
                    GP.PrePinState = GP.PinValue;
                }
            }
        }

        public byte GPIOStation { get; set; }
        public ObservableCollection<GPIOPin> GPIOPins { get; set; } = new ObservableCollection<GPIOPin>();

        public async Task SetOff(GPIOPin Pin)
        {
            var outputRegister = await App.GPIOCOM.GPIOWriteAsync(GPIOStation, Flag.GetIOInPut, null);
            if (outputRegister != null)
            {
                GPIORegister = BitConverter.ToInt32(outputRegister, 5);
                GPIORegister &= ~Pin.GPIOBitmask;
                await App.GPIOCOM.GPIOWriteAsync(GPIOStation, Flag.SetIOOutput, BitConverter.GetBytes(GPIORegister));
            }
        }
        public async Task SetOn(GPIOPin Pin)
        {

            var outputRegister = await App.GPIOCOM.GPIOWriteAsync(GPIOStation, Flag.GetIOInPut, null);
            if (outputRegister != null)
            {
                GPIORegister = BitConverter.ToInt32(outputRegister, 5);
                GPIORegister |= Pin.GPIOBitmask;
                await App.GPIOCOM.GPIOWriteAsync(GPIOStation, Flag.SetIOOutput, BitConverter.GetBytes(GPIORegister));
            }
        }

        public async Task BSetOff(int from, int to)
        {
            var outputRegister = await App.GPIOCOM.GPIOWriteAsync(GPIOStation, Flag.GetIOInPut, null);
            if (outputRegister != null)
            {
                GPIORegister = BitConverter.ToInt32(outputRegister, 5);
                int OutputMask = 0;
                for (int i = from; i <= to; i++)
                {

                    OutputMask |= (1 << i);
                }
                GPIORegister &= ~OutputMask;
                await App.GPIOCOM.GPIOWriteAsync(GPIOStation, Flag.SetIOOutput, BitConverter.GetBytes(GPIORegister));
            }
        }
        public async Task BSetOn(int from, int to)
        {
            var outputRegister = await App.GPIOCOM.GPIOWriteAsync(GPIOStation, Flag.GetIOInPut, null);
            if (outputRegister != null)
            {
                GPIORegister = BitConverter.ToInt32(outputRegister, 5);
                int OutputMask = 0;
                for (int i = from; i <= to; i++)
                {

                    OutputMask |= (1 << i);
                }
                GPIORegister |= OutputMask;
                await App.GPIOCOM.GPIOWriteAsync(GPIOStation, Flag.SetIOOutput, BitConverter.GetBytes(GPIORegister));
            }
        }

        public async Task GetGPIOsState()
        {
            var gpioRegister = await App.GPIOCOM.GPIOWriteAsync(GPIOStation, Flag.GetIOInPut, null);
            if (gpioRegister != null)
            {
                GPIORegister = BitConverter.ToInt32(gpioRegister, 5);
            }
        }

        private void NotifyPinValueChanged(GPIOPin Pin, Edge e)
        {
            // Make sure someone is listening to event
            if (OnPinValueChanged != null)
            {
                PinValueChangedEventArgs args = new PinValueChangedEventArgs(Pin, e);
                OnPinValueChanged(this, args);
            }
        }

        public delegate void PinStateChanged(object sender, PinValueChangedEventArgs e);
        public event PinStateChanged OnPinValueChanged;
        public class PinValueChangedEventArgs : EventArgs
        {
            public Edge Edge { get; private set; }
            public GPIOPin Pin { get; private set; }
            public PinValueChangedEventArgs(GPIOPin Pin, Edge e)
            {
                this.Pin = Pin;
                Edge = e;
            }
        }
    }

    public class GPIOPin : INotifyPropertyChanged
    {
        public string GPIODesciption { get; set; }
        public string GPIOLabel { get; set; }
        public int GPIOBitmask { get; set; }
        public PinValue PrePinState { get; set; }
        private PinValue _PinValue;
        public PinValue PinValue { get { return _PinValue; } set { _PinValue = value; NotifyPropertyChanged("PinValue"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class GPIOControl : IDisposable
    {
        SerialHelper Serial = null;
        private const int TimeDelay = 15;
        public byte[] ReceivedBytes { get; private set; }
        private int MAXRETRY = 3;

        public GPIOControl(ref SerialDevice SerialPort)
        {
            Serial = new SerialHelper(SerialPort, 1, 1);
            Serial.OnDataReceived += Serial_OnDataReceived;
        }

        private void Serial_OnDataReceived(object sender, SerialReceivedDataEventArgs e)
        {
            ReceivedBytes = e.ReceivedByte;
        }
        private List<byte> CRC16Generator(List<byte> Bytes)
        {
            List<byte> CRC = new List<byte>();
            ushort CheckSum = 0xFFFF;
            ushort j;
            byte lowCRC;
            byte highCRC;
            for (j = 0; j < Bytes.Count; j++)
            {
                CheckSum = (ushort)(CheckSum ^ Bytes[j]);
                for (short i = 8; i > 0; i--)
                    if ((CheckSum & 0x0001) == 1)
                        CheckSum = (ushort)((CheckSum >> 1) ^ 0xA001);
                    else
                        CheckSum >>= 1;
            }
            highCRC = (byte)(CheckSum >> 8);
            CheckSum <<= 8;
            lowCRC = (byte)(CheckSum >> 8);
            CRC.Add(lowCRC);
            CRC.Add(highCRC);
            return CRC;
        }
        private bool CRC16ErrorCheck(ref List<byte> Data)
        {
            if (Data.Count < 2)
                return false;
            try
            {
                byte HighCRC = Data[Data.Count - 1];
                byte LowCRC = Data[Data.Count - 2];
                Data.RemoveRange(Data.Count - 2, 2);
                List<byte> CalculatedCRC = CRC16Generator(Data);
                if (HighCRC == CalculatedCRC[1] && LowCRC == CalculatedCRC[0])
                    return true;
                else return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        SemaphoreSlim Writelock = new SemaphoreSlim(1, 1);
        private static readonly List<byte> Header = new List<byte>() { 0xAA, 0xCC };
        private static readonly List<byte> Tail = new List<byte>() { 0xAA, 0xEE };

        public async Task<byte[]> GPIOWriteAsync(byte SlaveID, byte FrameType, byte[] Data)
        {
            try
            {
                await Writelock.WaitAsync();
                int Retry = MAXRETRY;
                List<byte> Frame = new List<byte> { SlaveID, FrameType };
                if (Data != null)
                    Frame.AddRange(Data);
                Frame.AddRange(CRC16Generator(Frame));
                Frame.AddRange(Tail);
                Frame.InsertRange(0, Header);
                bool ErrorCheck = false;
                await Serial.WriteAsync(Frame.ToArray());
                while (Retry > 0)
                {
                    ReceivedBytes = new byte[16];
                    await Task.Delay(TimeDelay);
                    if (ReceivedBytes[0] != 0)
                    {
                        ErrorCheck = ParseResponde(SlaveID, FrameType, ReceivedBytes.ToList());
                        if (ErrorCheck)
                            return ReceivedBytes;
                    }
                    await Serial.WriteAsync(Frame.ToArray());
                    Retry--;
                }
                return null;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                throw;
            }
            finally
            {
                Writelock.Release();
            }
        }

        private bool ParseResponde(byte SlaveID, byte FrameType, List<byte> Data)
        {
            try
            {
                Data.RemoveRange(0, 2);
                Data.RemoveRange(Data.Count - 2, 2);
                if (CRC16ErrorCheck(ref Data))
                {
                    if (SlaveID == Data[0] && FrameType == Data[1] && Data[2] == BitMask.FrameOK)
                        return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void Dispose()
        {
            if (Serial != null)
                Serial.closeDevice();
            Serial = null;
        }
    }
    public class GPIOBitmask
    {
        public const int GPIO0 = 1;
        public const int GPIO1 = 1 << 1;
        public const int GPIO2 = 1 << 2;
        public const int GPIO3 = 1 << 3;
        public const int GPIO4 = 1 << 4;
        public const int GPIO5 = 1 << 5;
        public const int GPIO6 = 1 << 6;
        public const int GPIO7 = 1 << 7;
        public const int GPIO8 = 1 << 8;
        public const int GPIO9 = 1 << 9;
        public const int GPIO10 = 1 << 10;
        public const int GPIO11 = 1 << 11;
        public const int GPIO12 = 1 << 12;
        public const int GPIO13 = 1 << 13;
        public const int GPIO14 = 1 << 14;
        public const int GPIO15 = 1 << 15;
        public const int GPIO16 = 1 << 16;
        public const int GPIO17 = 1 << 17;
        public const int GPIO18 = 1 << 18;
        public const int GPIO19 = 1 << 19;
        public const int GPIO20 = 1 << 20;
        public const int GPIO21 = 1 << 21;
        public const int GPIO22 = 1 << 22;
        public const int GPIO23 = 1 << 23;
        public const int GPIO24 = 1 << 24;
        public const int GPIO25 = 1 << 25;
        public const int GPIO26 = 1 << 26;
        public const int GPIO27 = 1 << 27;
        public const int GPIO28 = 1 << 28;
        public const int GPIO29 = 1 << 29;
        public const int GPIO30 = 1 << 30;
        public const int GPIO31 = 1 << 31;
    }


}
