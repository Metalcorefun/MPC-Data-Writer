using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataWriter
{
    public static class DianaDevLibDLL
    {

        #region Constants
        public const System.UInt16 CH_OPTIONAL_INDEX = 0; // ............ Дополнительный канал ................. 
        public const System.UInt16 CH_EDA_INDEX = 1; // .................КГР .................................. 
        public const System.UInt16 CH_TR_INDEX = 2; // .................. Верхнее дыхание, ВДХ ................. 
        public const System.UInt16 CH_AR_INDEX = 3; // .................. Нижнее дыхание, НДХ .................. 
        public const System.UInt16 CH_PLE_INDEX = 4; // .................Плетизма, ПГ ........................ 
        public const System.UInt16 CH_TREMOR_INDEX = 5; // ..............Тремор, ТРМ .......................... 
        public const System.UInt16 CH_BV_INDEX = 6; // .................. Артериальное давление, АД ............ 
        public const System.UInt16 CH_TEDA_INDEX = 7; // ................Тоническая составляющая КГР .......... 
        public const System.UInt16 CH_ABSBV_INDEX = 8; // ............... Абсолютное значение давления ......... 

        //Типы Доп-канала
        public const Byte OT_TYPE_1 = 0; //ПГ, ТРМ
        public const Byte OT_TYPE_2 = 1; //КГР
        #endregion

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DeviceInfo
        {

            public System.UInt32 DeviceIndex;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public char[] SerialNumber;

        }

        public delegate void DataReceivedCallback(System.UInt32 dwUser, [MarshalAs(UnmanagedType.LPArray, SizeConst = 9)] System.UInt16[] pDataPacket);
        public delegate void ConnectionChangedCallback(System.UInt32 dwUser, System.UInt32 dwChangeType);
        public delegate void DianaInfoCallback(System.UInt32 dwUser, [MarshalAs(UnmanagedType.LPStr)] string lpstrDianaInfo);
        public delegate void DispChangedCallback(System.UInt32 dwUser, UInt16 wChannelIndex, UInt16 wValue);
        public delegate void AmplChangedCallback(System.UInt32 dwUser, UInt16 wChannelIndex, Byte bValue);
        public delegate void OptionalTypeChangedCallback(System.UInt32 dwUser, Byte bValue);

        [DllImport("DianaDevLib.dll", EntryPoint = "Init", CallingConvention = CallingConvention.StdCall)]
        public static extern uint Init();

        [DllImport("DianaDevLib.dll", EntryPoint = "Free", CallingConvention = CallingConvention.StdCall)]
        public static extern void Free();

        [DllImport("DianaDevLib.dll", EntryPoint = "CreateDiana", CallingConvention = CallingConvention.StdCall)]
        public static extern void CreateDiana(out IntPtr pDiana);

        [DllImport("DianaDevLib.dll", EntryPoint = "FreeDiana", CallingConvention = CallingConvention.StdCall)]
        public static extern void FreeDiana(IntPtr pDiana);

        [DllImport("DianaDevLib.dll", EntryPoint = "GetDevCount", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDevCount(IntPtr pAkpe, out UInt32 count);

        [DllImport("DianaDevLib.dll", EntryPoint = "GetDevList", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDevList(IntPtr pAkpe, [Out] DeviceInfo[] pDevInfo, ref System.UInt32 count);

        [DllImport("DianaDevLib.dll", EntryPoint = "OpenDevice", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenDevice(IntPtr pAkpe, ref DeviceInfo pDeviceInfo);

        [DllImport("DianaDevLib.dll", EntryPoint = "CloseDevice", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseDevice(IntPtr pAkpe);

        [DllImport("DianaDevLib.dll", EntryPoint = "RequestDianaInfo", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RequestDianaInfo(IntPtr pAkpe);

        [DllImport("DianaDevLib.dll", EntryPoint = "GetDisp", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt16 GetDisp(IntPtr pAkpe, UInt16 wChannelIndex);

        [DllImport("DianaDevLib.dll", EntryPoint = "SendDisp", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SendDisp(IntPtr pAkpe, UInt16 wChannelIndex, UInt16 wValue);

        [DllImport("DianaDevLib.dll", EntryPoint = "GetAmpl", CallingConvention = CallingConvention.StdCall)]
        public static extern Byte GetAmpl(IntPtr pAkpe, UInt16 wChannelIndex);

        [DllImport("DianaDevLib.dll", EntryPoint = "SendAmpl", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SendAmpl(IntPtr pAkpe, UInt16 wChannelIndex, Byte bValue);

        [DllImport("DianaDevLib.dll", EntryPoint = "GetOptionalType", CallingConvention = CallingConvention.StdCall)]
        public static extern Byte GetOptionalType(IntPtr pAkpe);

        [DllImport("DianaDevLib.dll", EntryPoint = "SendOptionalType", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SendOptionalType(IntPtr pAkpe, Byte bValue);

        [DllImport("DianaDevLib.dll", EntryPoint = "GetTestMode", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetTestMode(IntPtr pAkpe);

        [DllImport("DianaDevLib.dll", EntryPoint = "SetTestMode", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetTestMode(IntPtr pAkpe, [MarshalAs(UnmanagedType.Bool)] bool bValue);


        [DllImport("DianaDevLib.dll", EntryPoint = "SetConnectionChangedCallback", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetConnectionChangedCallback(IntPtr pDiana, UInt32 dwUser, ConnectionChangedCallback pCallback);

        [DllImport("DianaDevLib.dll", EntryPoint = "SetDataReceivedCallback", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetDataReceivedCallback(IntPtr pDiana, UInt32 dwUser, DataReceivedCallback pCallback);

        [DllImport("DianaDevLib.dll", EntryPoint = "SetDianaInfoCallback", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetDianaInfoCallback(IntPtr pDiana, UInt32 dwUser, DianaInfoCallback pCallback);

        [DllImport("DianaDevLib.dll", EntryPoint = "SetDispChangedCallback", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetDispChangedCallback(IntPtr pDiana, UInt32 dwUser, DispChangedCallback pCallback);

        [DllImport("DianaDevLib.dll", EntryPoint = "SetAmplChangedCallback", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetAmplChangedCallback(IntPtr pDiana, UInt32 dwUser, AmplChangedCallback pCallback);

        [DllImport("DianaDevLib.dll", EntryPoint = "SetOptionalTypeChangedCallback", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetOptionalTypeChangedCallback(IntPtr pDiana, UInt32 dwUser, OptionalTypeChangedCallback pCallback);

    }
}
