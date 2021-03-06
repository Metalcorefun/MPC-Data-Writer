﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using KBCsv;

using static DataWriter.DianaDevLibDLL;

namespace DataWriter
{
    public partial class MainWindow : Window
    {
        IntPtr pDiana = IntPtr.Zero;

        private CSVSender csvSender;

        public class ComboBoxDeviceItem
        {
            public string Text { get; set; }
            public DeviceInfo Value { get; set; }

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

        private DataReceivedCallback OnDataReceived;
        private ConnectionChangedCallback OnConnectionChanged;
        private DianaInfoCallback OnDianaInfo;
        //private DispChangedCallback OnDispChanged;
        //private AmplChangedCallback OnAmplChanged;
        private OptionalTypeChangedCallback OnOptionalTypeChanged;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += (s, e) => (this.DataContext as IDisposable).Dispose();
        }

        const uint S_OK = 0;
        const uint E_ACCESSDENIED = 0x80070005;

        private void Start_Diana()
        {
            tiEquipment.IsEnabled = true;
            uint res = Init();
            switch (Init())
            {
                case S_OK:

                    break;
                case E_ACCESSDENIED:
                    System.Windows.MessageBox.Show("Отсутствуют права на использование: нужен USB-ключ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                default:
                    System.Windows.MessageBox.Show("Непредвиденная ошибка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
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
            cbDeviceList.Items.Clear();
            if (!GetDevCount(pDiana, out System.UInt32 count))
                return;
            DeviceInfo[] pDevInfo = new DeviceInfo[count];
            if (!GetDevList(pDiana, pDevInfo, ref count))
                return;

            foreach (var di in pDevInfo)
            {
                string sn = new string(di.SerialNumber);
                ComboBoxDeviceItem item = new ComboBoxDeviceItem
                {
                    Text = sn.Substring(0, sn.IndexOf('\0')), //перевод из null terminated string
                    Value = di
                };

                cbDeviceList.Items.Add(item);
            }
            if (cbDeviceList.Items.Count > 0)
                cbDeviceList.SelectedIndex = 0;
        }

        private void UpdateOptionalTypeGUI(Byte value)
        {
            switch (value)
            {
                case OT_TYPE_1:
                    tbOptionalType.Text = "Тип Доп: OT_TYPE_1";
                    break;
                case OT_TYPE_2:
                    tbOptionalType.Text = "Тип Доп: OT_TYPE_2";
                    break;
            }

        }

        private void UpdateTestModeGUI(bool value)
        {
            tbTestMode.Text = value ? "Тестовый режим включен" : "Тестовый режим выключен";
        }

        private void UpdateDianaInfo(string lpstrDianaInfo)
        {
            tbDianaInfo.Text = lpstrDianaInfo;
        }

        private void RefreshAudioDevices(object sender, RoutedEventArgs e)
        {
            List<NAudio.Wave.WaveInCapabilities> sources = new List<NAudio.Wave.WaveInCapabilities>();

            for (int i = 0; i < NAudio.Wave.WaveIn.DeviceCount; i++)
            {
                sources.Add(NAudio.Wave.WaveIn.GetCapabilities(i));
            }

            sourceList.Items.Clear();

            foreach (var source in sources)
            {
                System.Windows.Controls.ListViewItem item = new System.Windows.Controls.ListViewItem();
                item.Content = source.ProductName;
                sourceList.Items.Add(item);
            }
        }

        NAudio.Wave.WaveIn sourceStream = null;
        NAudio.Wave.DirectSoundOut waveOut = null;
        NAudio.Wave.WaveFileWriter waveWriter = null;

        private void StartAudioRecording(string path)
        {
            if (sourceList.SelectedItems.Count == 0) return;

            int deviceNumber = sourceList.Items.IndexOf(sourceList.SelectedItems[0]);

            string timestamp = DateTime.Now.ToString();
            timestamp = timestamp.Replace(':', '.');
            string filename = path + "\\" + timestamp + ".wav";
            

            sourceStream = new NAudio.Wave.WaveIn();
            sourceStream.DeviceNumber = deviceNumber;
            sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(44100, NAudio.Wave.WaveIn.GetCapabilities(deviceNumber).Channels);

            sourceStream.DataAvailable += new EventHandler<NAudio.Wave.WaveInEventArgs>(sourceStream_DataAvailable);
            waveWriter = new NAudio.Wave.WaveFileWriter(filename, sourceStream.WaveFormat);

            sourceStream.StartRecording();
        }

        private void sourceStream_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            if (waveWriter == null) return;

            waveWriter.WriteData(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();
        }

        private void StopAudioRecording()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
            if (sourceStream != null)
            {
                sourceStream.StopRecording();
                sourceStream.Dispose();
                sourceStream = null;
            }
            if (waveWriter != null)
            {
                waveWriter.Dispose();
                waveWriter = null;
            }
        }    

        private void ShowDirectory(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var dialogresult = dialog.ShowDialog();
            if (dialogresult == System.Windows.Forms.DialogResult.OK)
            {
                tbPath.Text = dialog.SelectedPath.ToString();
            }
        }

        private void StartRecording(object sender, RoutedEventArgs e)
        {
            csvSender = new CSVSender(tbPath.Text);
            Start_Diana();
            StartAudioRecording(tbPath.Text);
        }

        private void StopRecording(object sender, RoutedEventArgs e)
        {
            StopAudioRecording();
            Stop_Diana();
        }

        private void Optional_Button_Click(object sender, RoutedEventArgs e)
        {
            if (GetOptionalType(pDiana) == OT_TYPE_1)
                SendOptionalType(pDiana, OT_TYPE_2);
            else
                SendOptionalType(pDiana, OT_TYPE_1);
        }

        private void cbDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbDianaInfo.Text = "";
            System.Windows.Controls.ComboBox cmb = (System.Windows.Controls.ComboBox)sender;
            if (cmb.SelectedItem != null)
            {
                DeviceInfo devInfo = ((ComboBoxDeviceItem)cmb.SelectedItem).Value;
                if (OpenDevice(pDiana, ref devInfo))
                {
                    RequestDianaInfo(pDiana);
                    UpdateGUI();
                }
            }
            else
            {
                CloseDevice(pDiana);
            }
        }

        private void TestMode_Button_Click(object sender, RoutedEventArgs e)
        {
            SetTestMode(pDiana, !GetTestMode(pDiana));
            UpdateTestModeGUI(GetTestMode(pDiana));
        }
    }
}
