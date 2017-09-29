using PDTestSerial.Servo;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PDTestSerial.Model
{
    public enum TestResult { NG, OK };
    public class JigModel : INotifyPropertyChanged
    {
        public int JigModelID { get; set; }
        public string JigDesciption { get; set; }
        public int JigID { get; set; }
        public int Channel { get; set; }
        public TestResult JigTestResult { get; set; }
        //--------------------------------------------------------
        private string _JigState;
        [NotMapped]
        public string JigState { get { return _JigState; } set { _JigState = value; NotifyPropertyChanged("JigState"); } }
        [NotMapped]
        public string PreviousJigState { get; internal set; }
        public bool IsSetInJig { get; internal set; }
        //--------------------------------------------------------
        private int _JigPos;
        public int JigPos { get { return _JigPos; } set { _JigPos = value; NotifyPropertyChanged("JigPos"); } }
        private TimeSpan _ElapseTime;
        [NotMapped]
        public TimeSpan ElapseTime { get { return _ElapseTime; } set { _ElapseTime = value; NotifyPropertyChanged("ElapseTime"); } }
        [NotMapped]
        public object Instance { get { return this; } }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public class PDJig
    {
        public GPIOPin PackingPin { get; set; } = null;
        public JigModel Jig { get; private set; }
        public PDJig(GPIOPin Packing, JigModel Jig)
        {
            PackingPin = Packing;
            this.Jig = Jig;
        }
    }

}
