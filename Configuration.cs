using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PDTestSerial
{

    public class Command
    {
        //Motor Command---------------------------
        public static readonly List<byte> CheckTestStatus = Encoding.ASCII.GetBytes("STS?").ToList();
        public static readonly List<byte> TestUSB31Dev_RQ = Encoding.ASCII.GetBytes("UD3?").ToList();
        public static readonly List<byte> TestUSB31OTG_RQ = Encoding.ASCII.GetBytes("UO3?").ToList();
        public static readonly List<byte> TestUSB20OTG_RQ = Encoding.ASCII.GetBytes("UO2?").ToList();
        public static readonly List<byte> TestPDC_RQ = Encoding.ASCII.GetBytes("PDC?").ToList();
        public static readonly List<byte> TestLOA_RQ = Encoding.ASCII.GetBytes("LOA?").ToList();
        public static readonly List<byte> TestVCO_RQ = Encoding.ASCII.GetBytes("VCO?").ToList();
        public static readonly List<byte> TestSBU_RQ = Encoding.ASCII.GetBytes("SBU?").ToList();
        public static readonly List<byte> Start = Encoding.ASCII.GetBytes("TSON").ToList();
        public static readonly List<byte> Stop = Encoding.ASCII.GetBytes("TSOF").ToList();
        //public static List<byte> TestSBU_RQ = new List<byte>() { (byte)'S', (byte)'B', (byte)'U', (byte)'?' };
    };

    public class Flag
    {
        //Motor Command---------------------------
        public const byte Reset = 0x2B;
        public const byte StepEnable = 0x2A;
        public const byte MoveOrigin = 0x33;
        public const byte GetAxisStatus = 0x40;
        public const byte GetActualPosition = 0x53;
        public const byte MoveSingleAxisAbs = 0x34;
        public const byte MoveSingleAxisInc = 0x35;
        public const byte MoveVelocity = 0x37;
        public const byte MoveStop = 0x31;
        public const byte SetIOOutput = 0x20;
        public const byte GetIOInPut = 0x22;
    }

    public class DataFrame
    {
        public static List<byte> MoveAbcIncData(int Pos, int Vel)
        {
            List<byte> CMD = new List<byte>();
            byte[] pos = BitConverter.GetBytes(Pos);
            byte[] vel = BitConverter.GetBytes(Vel);
            CMD.AddRange(pos);
            CMD.AddRange(vel);
            return CMD;
        }
        public static List<byte> MoveVelocityData(int Vel, byte Direction)
        {
            List<byte> CMD = new List<byte>();
            byte[] vel = BitConverter.GetBytes(Vel);
            CMD.AddRange(vel);
            CMD.Add(Direction);
            return CMD;
        }
        /// <summary>
        /// Set on an OUTPUT
        /// </summary>
        /// <param name="OUT">Flag.UserOut bit masked</param>
        /// <returns></returns>
        public static List<byte> EziSetOnBitMask(uint OUT)
        {
            List<byte> CMD = new List<byte>();
            byte[] Out = BitConverter.GetBytes(OUT);
            CMD.AddRange(Out);
            CMD.AddRange(BitConverter.GetBytes((uint)0));
            return CMD;
        }
        /// <summary>
        /// Set off an OUTPUT
        /// </summary>
        /// <param name="OUT">Flag.UserOut bit masked</param>
        /// <returns></returns>
        public static List<byte> EziSetOffBitMask(uint OUT)
        {
            List<byte> CMD = new List<byte>();
            byte[] Out = BitConverter.GetBytes(OUT);
            CMD.AddRange(BitConverter.GetBytes((uint)0));
            CMD.AddRange(Out);
            return CMD;
        }
        public static async Task<uint> GetIOInputAsync(byte station)
        {
            var Responde = await App.ServoCOM.StepGetdata(station, Flag.GetIOInPut, null);
            if (Responde != null)
            {
                uint io = BitConverter.ToUInt32(Responde, 5);
                return io;
            }
            return 0;
        }
    }

    public class BitMask
    {
        //--------------------------------------------        
        public const byte On = 0x01;
        public const byte Off = 0x00;
        public const byte FrameOK = 0x00;
        public const byte Error = 0x80;
        public const uint ORGReturning = 0x00040000;
        public const uint Motioning = 0x08000000;
        //IO OUTPUT MASK
        public const uint USEROUT1 = 0x00008000;
        public const uint USEROUT2 = 0x00010000;
        public const uint USEROUT3 = 0x00020000;
        public const uint USEROUT4 = 0x00040000;
        public const uint USEROUT5 = 0x00080000;
        public const uint USEROUT6 = 0x00100000;
        public const uint USEROUT7 = 0x00200000;
        public const uint USEROUT8 = 0x00400000;
        public const uint USEROUT9 = 0x00800000;
        //IO USER INPUT MASK
        public const uint INPOSITION = 0x00000002;
        public const uint ALARM = 0x00000004;
        public const uint ALARMBLINK = 0x00000080;
        public const uint ORGSEARCHOK = 0x00000100;
        public const uint SERVOREADY = 0x00000200;
        public const uint USERIN1 = 0x04000000;
        public const uint USERIN2 = 0x08000000;
        public const uint USERIN3 = 0x10000000;
        public const uint USERIN4 = 0x20000000;
        public const uint USERIN5 = 0x40000000;
        public const uint USERIN6 = 0x80000000;
        public const uint USERIN7 = 0x00000200;
        public const uint USERIN8 = 0x00000400;
        public const uint USERIN9 = 0x00000800;
    }

    public enum PinValue { OFF, ON }
    public enum Edge { Rise, Fall }
}

