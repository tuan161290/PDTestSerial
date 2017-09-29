using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace PDTestSerial
{
    public class ServoControl : IDisposable
    {

        private const int TimeDelay = 20;
        SerialHelper Serial = null;
        private const int MAXRETRY = 3;
        public ServoControl(ref SerialDevice SerialPort)
        {
            Serial = new SerialHelper(SerialPort, 5, 5);
            Serial.OnDataReceived += Serial_OnDataReceived;
        }

        public byte[] ReceivedBytes { get; private set; }
        private void Serial_OnDataReceived(object sender, SerialReceivedDataEventArgs e)
        {
            ReceivedBytes = e.ReceivedByte;
        }

        //Servo motor control--------------------------------------------------        
        //const ushort polynomial = 0xA001;
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
        public async Task<byte[]> StepGetdata(byte SlaveID, byte FrameType, List<byte> Data)
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

        //public async Task<byte> StepWrite(byte SlaveID, byte FrameType, List<byte> Data)
        //{
        //    try
        //    {
        //        await Writelock.WaitAsync();
        //        int Retry = MAXRETRY;
        //        List<byte> Frame = new List<byte> { SlaveID, FrameType };
        //        if (Data != null)
        //            Frame.AddRange(Data);
        //        Frame.AddRange(CRC16Generator(Frame));
        //        Frame.AddRange(Tail);
        //        Frame.InsertRange(0, Header);
        //        ReceivedBytes = new byte[16];
        //        bool ErrorCheck = false;
        //        while (Retry > 0)
        //        {
        //            await Serial.WriteAsync(Frame.ToArray());
        //            await Task.Delay(TimeDelay);
        //            if (ReceivedBytes[0] != 0)
        //            {
        //                ErrorCheck = ParseResponde(SlaveID, FrameType, ReceivedBytes.ToList());
        //                if (ErrorCheck)
        //                {
        //                    Retry = 0;
        //                }
        //            }
        //            Retry--;
        //        }
        //        if (ErrorCheck == false && Retry == 0)
        //        {
        //            return BitMask.Error;
        //        }
        //        return ReceivedBytes[4]; ;
        //    }
        //    catch (Exception ex)
        //    {
        //        string s = ex.Message;
        //        throw ex;
        //    }
        //    finally
        //    {
        //        Writelock.Release();
        //    }
        //}

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
}
