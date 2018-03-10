using System;
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

        void DataReceivedCallback(System.UInt32 dwUser, [MarshalAs(UnmanagedType.LPArray, SizeConst = 8)] System.UInt16[] pDataPacket)
        {
            Dispatcher.InvokeAsync(() => UpdateDataUI(pDataPacket));
        }

        private DataReceivedCallback OnDataReceived;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += (s, e) => (this.DataContext as IDisposable).Dispose();
        }

        const uint S_OK = 0;
        const uint E_ACCESSDENIED = 0x80070005;

        private void Start_Diana()
        {
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
    }
}
