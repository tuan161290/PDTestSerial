using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDTestSerial.UCT
{
    public class UCTResult : INotifyPropertyChanged
    {
        public enum TestState { Ready, Testing }
        public int JigID;
        //CHANNEL 1 Properties----------------------------------------------------
        public string Channel1 { get; set; } = "Channel 1";
        //------------------------------------------------------------------------
        private string previousStatus1 = "FINISHED";
        private string channel1TestStatus;
        public string Channel1TestStatus
        {
            get { return channel1TestStatus; }
            set
            {
                channel1TestStatus = value;
                NotifyPropertyChanged("Channel1TestStatus");
                if (previousStatus1 != channel1TestStatus && channel1TestStatus != "NORESP")
                {
                    if (channel1TestStatus == "FINISHED")
                    {
                        ChannelTestResult result = new ChannelTestResult() { Channel = Channel1, JigID = JigID };
                        NotifyTestComplete(result, Channel1TestStatuses);
                    }
                }
                if (channel1TestStatus != "NORESP")
                    previousStatus1 = channel1TestStatus;
            }
        }
        public ObservableCollection<TestStatus> Channel1TestStatuses = new ObservableCollection<TestStatus>()       {
           new TestStatus(){TestItem = "UD3.0",},
           new TestStatus(){TestItem = "UO3.0",},
           new TestStatus(){TestItem = "UO2.0",},
           new TestStatus(){TestItem = "PDC",},
           new TestStatus(){TestItem = "Load",},
           new TestStatus(){TestItem = "VCONN",},
           new TestStatus(){TestItem = "SBU",}
        };

        //CHANNEL 2 Properties----------------------------------------------------
        public string Channel2 { get; set; } = "Channel 2";
        private string previousStatus2 = "FINISHED";
        private string channel2TestStatus;
        public string Channel2TestStatus
        {
            get { return channel2TestStatus; }
            set
            {
                channel2TestStatus = value;
                NotifyPropertyChanged("Channel2TestStatus");
                if (previousStatus2 != channel2TestStatus && channel2TestStatus != "NORESP")
                {
                    if (channel2TestStatus == "FINISHED")
                    {
                        ChannelTestResult result = new ChannelTestResult() { Channel = Channel2, JigID = JigID };
                        NotifyTestComplete(result, Channel1TestStatuses);
                    }
                }
                if (channel2TestStatus != "NORESP")
                    previousStatus2 = channel2TestStatus;
            }
        }
        public ObservableCollection<TestStatus> Channel2TestStatuses = new ObservableCollection<TestStatus>()       {
           new TestStatus(){TestItem = "UD3.0",},
           new TestStatus(){TestItem = "UO3.0",},
           new TestStatus(){TestItem = "UO2.0",},
           new TestStatus(){TestItem = "PDC",},
           new TestStatus(){TestItem = "Load",},
           new TestStatus(){TestItem = "VCONN",},
           new TestStatus(){TestItem = "SBU",}
        };
        //-------------------------NotifyPropertyChanged--------------------------------
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        //-------------------------NotifyTestComplete-----------------------------------
        public delegate void OnChannelTestComplete(object sender, TestCompleteEventArgs e);
        public event OnChannelTestComplete OnTestComplete;

        public void NotifyTestComplete(ChannelTestResult sender, ObservableCollection<TestStatus> ChannelTestResult)
        {
            if (OnTestComplete == null) return;
            TestCompleteEventArgs e = new TestCompleteEventArgs(sender, ChannelTestResult);
            OnTestComplete(this, e);
        }
    }

    public class TestCompleteEventArgs : EventArgs
    {
        public ChannelTestResult Result { get; set; }
        public TestCompleteEventArgs(ChannelTestResult channel, ObservableCollection<TestStatus> channelTestResult)
        {
            Result = channel;
            foreach (TestStatus T in channelTestResult)
            {
                if (T.TestResult == "FAIL")
                {
                    Result.IsNG = true;
                    Result.IsOK = false;
                };
            }
        }
    }

    public class TestStatus : INotifyPropertyChanged
    {
        public string TestItem { get; set; }

        private string testResult;
        public string TestResult
        {
            get { return testResult; }
            set { testResult = value; NotifyPropertyChanged("TestResult"); }
        }

        //-------------------------NotifyPropertyChanged--------------------------------
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class ChannelTestResult
    {
        public int JigID { get; set; }
        public string Channel { get; set; }
        public string FinalResult { get; set; }
        public bool IsNG { get; set; } = false;
        public bool IsOK { get; set; } = true;
        public int Position { get; set; }
    }
}
