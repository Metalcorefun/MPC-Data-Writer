using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Runtime.InteropServices;

using static DataWriter.DianaDevLibDLL;

namespace DataWriter
{
    public class DianaModel: IDisposable
    {
        const uint S_OK = 0;
        const uint E_ACCESSDENIED = 0x80070005;

        private bool tabEnabled = false;

        private string tbOptionalType;
        private string tbTestMode;
        private string tbDianaInfo;

        private DataReceivedCallback OnDataReceived;
        private ConnectionChangedCallback OnConnectionChanged;
        private DianaInfoCallback OnDianaInfo;

        //private DispChangedCallback OnDispChanged;
        //private AmplChangedCallback OnAmplChanged;
        private OptionalTypeChangedCallback OnOptionalTypeChanged;

        IntPtr pDiana = IntPtr.Zero;

        private CSVSender csvSender;

        public DeviceInfo Value { get; set; }
        public class ComboBoxDeviceItem
        {
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        void DataReceivedCallback(System.UInt32 dwUser, [MarshalAs(UnmanagedType.LPArray, SizeConst = 8)] System.UInt16[] pDataPacket)
        {
            Dispatcher.InvokeAsync(() => UpdateDataUI(pDataPacket));
        }

        void ConnectionChangedCallback(System.UInt32 dwUser, System.UInt32 dwChangeType)
        {
            Dispatcher.InvokeAsync(() => UpdateDeviceList());
        }

        void DianaInfoCallback(System.UInt32 dwUser, [MarshalAs(UnmanagedType.LPStr)] string lpstrDianaInfo)
        {
            Dispatcher.InvokeAsync(() => UpdateDianaInfo(lpstrDianaInfo));
        }


        void OptionalTypeChangedCallback(System.UInt32 dwUser, Byte bValue)
        {
            Dispatcher.InvokeAsync(() => UpdateOptionalType(bValue));
        }

        private void UpdateOptionalType(byte bValue)
        {
            Dispatcher.InvokeAsync(() => UpdateOptionalTypeGUI(bValue));
        }

        private void Start_Diana()
        {
            tabEnabled = true;
            uint res = Init();
            switch (Init())
            {
                case S_OK:
                    break;
                case E_ACCESSDENIED:
                    throw new Exception("Отсутствуют права на использование: нужен USB-ключ");
                default:
                    throw new Exception("Непредвиденная ошибка");
            }
            CreateDiana(out pDiana);
            if (pDiana == IntPtr.Zero)
                return;

            OnDataReceived = DataReceivedCallback;
            SetDataReceivedCallback(pDiana, 0, OnDataReceived);

            OnConnectionChanged = ConnectionChangedCallback;
            SetConnectionChangedCallback(pDiana, 0, OnConnectionChanged);

            OnDianaInfo = DianaInfoCallback;
            SetDianaInfoCallback(pDiana, 0, OnDianaInfo);

            OnOptionalTypeChanged = OptionalTypeChangedCallback;
            SetOptionalTypeChangedCallback(pDiana, 0, OnOptionalTypeChanged);

            UpdateDeviceList();
        }

        private void Stop_Diana()
        {
            if (pDiana != IntPtr.Zero)
            {
                FreeDiana(pDiana);
                pDiana = IntPtr.Zero;
            }
            Free();
        }

        private void UpdateDataUI(System.UInt16[] pDataPacket)
        {
            Task.Run(() => csvSender.WriteDataToCSV(pDataPacket));
        }

        private void UpdateGUI()
        {
            //UpdateDispGUI();
            //UpdateAmplGUI();
            UpdateOptionalTypeGUI(GetOptionalType(pDiana));
            UpdateTestModeGUI(GetTestMode(pDiana));
        }

        private void UpdateDeviceList()
        {
            //System.UInt32 count;
            //cbDeviceList.Items.Clear();
            //if (!GetDevCount(pDiana, out count))
            //    return;
            //DeviceInfo[] pDevInfo = new DeviceInfo[count];
            //if (!GetDevList(pDiana, pDevInfo, ref count))
            //    return;

            //foreach (var di in pDevInfo)
            //{
            //    string sn = new string(di.SerialNumber);
            //    ComboBoxDeviceItem item = new ComboBoxDeviceItem
            //    {
            //        Text = sn.Substring(0, sn.IndexOf('\0')), //перевод из null terminated string
            //        Value = di
            //    };

            //    cbDeviceList.Items.Add(item);
            //}
            //if (cbDeviceList.Items.Count > 0)
            //    cbDeviceList.SelectedIndex = 0;
        }

        private void UpdateOptionalTypeGUI(Byte value)
        {
            switch (value)
            {
                case OT_TYPE_1:
                    tbOptionalType = "Тип Доп: OT_TYPE_1";
                    break;
                case OT_TYPE_2:
                    tbOptionalType = "Тип Доп: OT_TYPE_2";
                    break;
            }
        }

        private void UpdateTestModeGUI(bool value)
        {
            tbTestMode = value ? "Тестовый режим включен" : "Тестовый режим выключен";
        }

        private void UpdateDianaInfo(string lpstrDianaInfo)
        {
            tbDianaInfo = lpstrDianaInfo;
        }

        public void Optional_Mode()
        {
            if (GetOptionalType(pDiana) == OT_TYPE_1)
                SendOptionalType(pDiana, OT_TYPE_2);
            else
                SendOptionalType(pDiana, OT_TYPE_1);
        }

        //private void cbDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    tbDianaInfo.Text = "";
        //    System.Windows.Controls.ComboBox cmb = (System.Windows.Controls.ComboBox)sender;
        //    if (cmb.SelectedItem != null)
        //    {
        //        DeviceInfo devInfo = ((ComboBoxDeviceItem)cmb.SelectedItem).Value;
        //        if (OpenDevice(pDiana, ref devInfo))
        //        {
        //            RequestDianaInfo(pDiana);
        //            UpdateGUI();
        //        }
        //    }
        //    else
        //    {
        //        CloseDevice(pDiana);
        //    }
        //}

        public void TestMode()
        {
            SetTestMode(pDiana, !GetTestMode(pDiana));
            UpdateTestModeGUI(GetTestMode(pDiana));
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
