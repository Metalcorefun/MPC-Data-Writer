using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NAudio.Wave;

namespace DataWriter
{
    public class AudioModel: IDisposable
    {
        private List<WaveInCapabilities> sources;
        private int _selectedIndex;
        private WaveIn sourceStream = null;
        private DirectSoundOut waveOut = null;
        private WaveFileWriter waveWriter = null;

        public int SelectedIndex { get { return _selectedIndex; } set { this._selectedIndex = value; } }

        public List<WaveInCapabilities> Sources { get; }

        public void RefreshAudioDevices()
        {
            sources = new List<WaveInCapabilities>();

            for (int i = 0; i <WaveIn.DeviceCount; i++)
            {
                sources.Add(WaveIn.GetCapabilities(i));
            }
        }

        private void StartAudioRecording(string path)
        {
            string timestamp = DateTime.Now.ToString();
            timestamp = timestamp.Replace(':', '.');
            string filename = path + "\\" + timestamp + ".wav";
            sourceStream = new NAudio.Wave.WaveIn();
            sourceStream.DeviceNumber = _selectedIndex;
            sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(44100, NAudio.Wave.WaveIn.GetCapabilities(_selectedIndex).Channels);

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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
