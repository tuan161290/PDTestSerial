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

namespace PDTestSerial.UCT
{
    public class UCTControl : IDisposable
    {
        SerialHelper Serial = null;

        public byte[] ReceivedBytes;

        public UCTControl(ref SerialDevice SerialPort)
        {
            Serial = new SerialHelper(SerialPort, 10, 10);
            Serial.OnDataReceived += Serial_OnDataReceived;
        }

        private void Serial_OnDataReceived(object sender, SerialReceivedDataEventArgs e)
        {
            ReceivedBytes = e.ReceivedByte;
        }

        public enum TestRequest { UD3, UO3, UO2, PDC, LOAD, VCON, SBU, STS };
        SemaphoreSlim WriteSync = new SemaphoreSlim(1, 1);

        public async Task<string> TestCommand(int JigNo, int channel, int OnOff)
        {
            try
            {
                await WriteSync.WaitAsync();
                char JigID = Convert.ToChar(JigNo.ToString());
                char Channel = Convert.ToChar(channel.ToString());
                List<byte> cmd = new List<byte>() { (byte)':', (byte)'0', (byte)JigID, (byte)'S', (byte)Channel };
                if (OnOff > 0)
                    cmd.AddRange(Command.Start);
                else
                    cmd.AddRange(Command.Stop);
                cmd.Add(CheckSumGenerate(cmd));
                string S = Encoding.ASCII.GetString(cmd.ToArray());
                ReceivedBytes = new byte[16];
                int Retry = 2;
                while (Retry > 0)
                {
                    await Serial.WriteAsync(cmd.ToArray());
                    await Task.Delay(20);
                    if (CheckSum(ReceivedBytes))
                    {
                        string ReceivedString = Encoding.ASCII.GetString(ReceivedBytes);
                        if (ReceivedString[2] == JigID && ReceivedString[4] == Channel)
                        {
                            Retry = 0;
                            if (ReceivedString.Contains("REDY")) return "READY";
                            else if (ReceivedString.Contains("15MD")) return "15MODE";
                            else if (ReceivedString.Contains("UD31")) return "UD3.1";
                            else if (ReceivedString.Contains("UO31")) return "UO3.1";
                            else if (ReceivedString.Contains("UO20")) return "UO2.0";
                            else if (ReceivedString.Contains("PDCT")) return "PDC";
                            else if (ReceivedString.Contains("LOAD")) return "LOAD";
                            else if (ReceivedString.Contains("VCON")) return "VCONN";
                            else if (ReceivedString.Contains("SBUT")) return "SBU";
                            else if (ReceivedString.Contains("FINI")) return "FINISHED";
                        }
                    }
                    Retry--;
                }
                return "NORESP";
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return s;
            }
            finally
            {
                WriteSync.Release();
            }
        }
        public async Task<string> GetTestStatus(int JigNo, int channel)
        {
            try
            {
                await WriteSync.WaitAsync();
                char JigID = Convert.ToChar(JigNo.ToString());
                char Channel = Convert.ToChar(channel.ToString());
                List<byte> cmd = new List<byte>() { (byte)':', (byte)'0', (byte)JigID, (byte)'S', (byte)Channel };
                List<byte> rqt = new List<byte>();
                cmd.AddRange(Command.CheckTestStatus);
                cmd.Add(CheckSumGenerate(cmd));
                string S = Encoding.ASCII.GetString(cmd.ToArray());
                ReceivedBytes = new byte[16];
                int Retry = 2;
                while (Retry > 0)
                {
                    await Serial.WriteAsync(cmd.ToArray());
                    await Task.Delay(20);
                    if (CheckSum(ReceivedBytes))
                    {
                        string ReceivedString = Encoding.ASCII.GetString(ReceivedBytes);
                        if (ReceivedString[2] == JigID && ReceivedString[4] == Channel)
                        {
                            Retry = 0;
                            if (ReceivedString.Contains("REDY")) return "READY";
                            else if (ReceivedString.Contains("15MD")) return "15MODE";
                            else if (ReceivedString.Contains("UD31")) return "UD3.1";
                            else if (ReceivedString.Contains("UO31")) return "UO3.1";
                            else if (ReceivedString.Contains("UO20")) return "UO2.0";
                            else if (ReceivedString.Contains("PDCT")) return "PDC";
                            else if (ReceivedString.Contains("LOAD")) return "LOAD";
                            else if (ReceivedString.Contains("VCON")) return "VCONN";
                            else if (ReceivedString.Contains("SBUT")) return "SBU";
                            else if (ReceivedString.Contains("STOP")) return "STOP";                            
                            else if (ReceivedString.Contains("FINI")) return "FINISHED";
                        }
                    }
                    Retry--;
                }
                return "NORESP";
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return s;
            }
            finally
            {
                WriteSync.Release();
            }
        }
        public async Task<string> CheckTestResult(int JigNo, int channel, TestRequest RQ)
        {
            try
            {
                await WriteSync.WaitAsync();
                char JigID = Convert.ToChar(JigNo.ToString());
                char Channel = Convert.ToChar(channel.ToString());
                List<byte> cmd = new List<byte>() { (byte)':', (byte)'0', (byte)JigID, (byte)'S', (byte)Channel };
                List<byte> rqt = new List<byte>();
                if (RQ == TestRequest.UD3) rqt = Command.TestUSB31Dev_RQ;
                else if (RQ == TestRequest.UO3) rqt = Command.TestUSB31OTG_RQ;
                else if (RQ == TestRequest.UO2) rqt = Command.TestUSB20OTG_RQ;
                else if (RQ == TestRequest.PDC) rqt = Command.TestPDC_RQ;
                else if (RQ == TestRequest.LOAD) rqt = Command.TestLOA_RQ;
                else if (RQ == TestRequest.VCON) rqt = Command.TestVCO_RQ;
                else if (RQ == TestRequest.SBU) rqt = Command.TestSBU_RQ;
                //else if (RQ == TestRequest.STS) rqt = Command.CheckTestStatus;
                cmd.AddRange(rqt);
                cmd.Add(CheckSumGenerate(cmd));
                string S = Encoding.ASCII.GetString(cmd.ToArray());
                ReceivedBytes = new byte[16];
                int Retry = 2;
                while (Retry > 0)
                {
                    await Serial.WriteAsync(cmd.ToArray());
                    await Task.Delay(20);

                    if (CheckSum(ReceivedBytes))
                    {
                        string ReceivedString = Encoding.ASCII.GetString(ReceivedBytes);
                        if (ReceivedString[3] == 'R' && ReceivedString[2] == JigID && ReceivedString[4] == Channel)
                        {
                            Retry = 0;
                            if (ReceivedString[8] == 'P')
                                return "PASS";
                            if (ReceivedString[8] == 'F')
                                return "FAIL";
                            if (ReceivedString[8] == 'T')
                                return "TEST...";
                            if (ReceivedString[8] == 'N')
                                return "NO_TEST";
                        }
                    }
                    Retry--;
                }
                return "NORESP";
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return s;
            }
            finally
            {
                WriteSync.Release();
            }
        }

        private byte CheckSumGenerate(List<byte> Bytes)
        {
            int i;
            byte CSUM = 0;
            for (i = 0; i < Bytes.Count(); i++)
            {
                CSUM += Bytes[i];
            }
            return CSUM;
        }
        private bool CheckSum(byte[] bytes)
        {
            if (bytes == null) return false;
            List<byte> Bytes = bytes.ToList();
            int i;
            if (Bytes.Count >= 12)
            {
                Bytes.RemoveRange(10, 2);
            }
            byte CSUM = 0;
            for (i = 0; i < Bytes.Count - 1; i++)
            {
                CSUM += Bytes[i];
            }
            if (CSUM == Bytes.Last())
                return true;
            else return false;
        }
        public void Dispose()
        {
            if (Serial != null)
                Serial.closeDevice();
            Serial = null;
        }
    }
}
