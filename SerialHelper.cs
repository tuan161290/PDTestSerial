using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace PDTestSerial
{
    public class SerialHelper
    {
        private SerialDevice serialPort;
        private DataWriter dataWriteObject = null;
        private DataReader dataReaderObject = null;
        private CancellationTokenSource ReadCancellationTokenSource;
        private byte[] ReceivedBytes { get; set; } = new byte[16];

        public SerialHelper(SerialDevice SerialPort, int ReadTimeOut, int WriteTimeOut)
        {
            serialPort = SerialPort;
            comPortInit(ReadTimeOut, WriteTimeOut);
        }

        private void comPortInit(int ReadTimeOut, int WriteTimeOut)
        {
            try
            {
                //serialPort = await SerialDevice.FromIdAsync(entry.Id);
                if (serialPort == null) return;
                // Configure serial settings
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(WriteTimeOut);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(ReadTimeOut);
                serialPort.BaudRate = 115200;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;
                // Display configured settings           
                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();
                // Enable 'WRITE' button to allow sending data  
                Listen();
            }
            catch (Exception)
            {

            }
        }
        public async Task WriteAsync(byte[] Bytes)
        {
            try
            {
                if (serialPort == null) return;
                dataWriteObject = new DataWriter(serialPort.OutputStream);
                Task<UInt32> storeAsyncTask;
                if (Bytes.Count() != 0)//sendText.Text.Length != 0)
                {
                    // Load the text from the sendText input text box to the dataWriter object
                    dataWriteObject.WriteBytes(Bytes);
                    // Launch an async task to complete the write operation
                    storeAsyncTask = dataWriteObject.StoreAsync().AsTask();
                    UInt32 bytesWritten = await storeAsyncTask;
                    if (bytesWritten > 0)
                    {
                        if (dataWriteObject != null)
                        {
                            dataWriteObject.DetachStream();
                            dataWriteObject = null;
                        }
                    }
                }
                else
                {
                    //status.Text = "Enter the text you want to write and then click on 'WRITE'";
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                throw;
            }
        }

        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);
                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                //status.Text = "Reading task was cancelled, closing device and cleaning up";
                CloseDevice();
            }
            catch (Exception)
            {
                //status.Text = ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }
        //SemaphoreSlim LockRead = new SemaphoreSlim(1);
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            //LockRead.Wait();
            Task<UInt32> loadAsyncTask;
            uint ReadBufferLength = 64;
            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();
            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Create a task object to wait for data on the serialPort.InputStream
                loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);
                // Launch the task and wait
                UInt32 bytesRead = await loadAsyncTask;
                if (bytesRead > 1)
                {
                    ReceivedBytes = new byte[bytesRead];
                    dataReaderObject.ReadBytes(ReceivedBytes);
                    NotifyDataReceived(ReceivedBytes);
                    //string s = Encoding.ASCII.GetString(ReceivedBytes);
                }
            }
            //LockRead.Release();
        }

        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        public void closeDevice()
        {
            try
            {
                //status.Text = "";
                CancelReadTask();
                CloseDevice();

            }
            catch (Exception)
            {
                //status.Text = ex.Message;
            }
        }

        private void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
        }

        public delegate void DataReceivedComplete(object sender, SerialReceivedDataEventArgs e);
        public event DataReceivedComplete OnDataReceived;

        private void NotifyDataReceived(byte[] receivedByte)
        {
            // Make sure someone is listening to event
            if (OnDataReceived == null) return;
            SerialReceivedDataEventArgs args = new SerialReceivedDataEventArgs(receivedByte);
            OnDataReceived(this, args);
        }
    }

    public class SerialReceivedDataEventArgs : EventArgs
    {
        public byte[] ReceivedByte { get; private set; }

        public SerialReceivedDataEventArgs(byte[] receivedByte)
        {
            ReceivedByte = receivedByte;
        }
    }
}
