using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using Accord.Video.FFMPEG;
using AForge.Video;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows.Threading;

namespace DataWriter
{
    public class CameraModel: INotifyPropertyChanged, IDisposable
    {
        #region Private fields
        private BitmapImage _image;
        private string _camera_url;
        private int _index;
        private bool _recording;

        private IVideoSource _videoSource;

        private VideoFileWriter _writer;
        private DateTime? _firstFrameTime;
        #endregion

        #region Properties
        public CameraModel(string camera_url, int index)
        {
            this._camera_url = camera_url;
            this._index = index;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                RaisePropertyChanged("Image");
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (_recording)
                {
                    using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                    {
                        if (_firstFrameTime != null)
                        {
                            _writer.WriteVideoFrame(bitmap, DateTime.Now - _firstFrameTime.Value);
                        }
                        else
                        {
                            _writer.WriteVideoFrame(bitmap);
                            _firstFrameTime = DateTime.Now;
                        }
                    }
                }
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    var bi = bitmap.ToBitmapImage();
                    bi.Freeze();
                    Dispatcher.CurrentDispatcher.Invoke(() => Image = bi);
                }
            }
            catch (Exception exc)
            {
                //StopCamera();
                System.Windows.MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
                throw exc;
            }
        }
        public void StartCamera()
        {
            _videoSource = new MJPEGStream(_camera_url);
            _videoSource.NewFrame += video_NewFrame;
            _videoSource.Start();
        }
        public void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.Stop();
                _videoSource.NewFrame -= video_NewFrame;
            }
            Dispatcher.CurrentDispatcher.Invoke(() => Image = null);
        }

        public void StartCameraRecording(string FolderPath)
        {
            var dialog = new SaveFileDialog();
            string timestamp = DateTime.Now.ToString();
            timestamp = timestamp.Replace(':', '.');
            dialog.FileName = FolderPath + "\\" + _index.ToString() + "_" + timestamp + ".avi";
            dialog.DefaultExt = ".avi";
            dialog.AddExtension = true;
            _firstFrameTime = null;
            _writer = new VideoFileWriter();
            _writer.Open(dialog.FileName, (int)Math.Round(_image.Width, 0), (int)Math.Round(_image.Height, 0));
            _recording = true;
        }

        public void StopCameraRecording()
        {
            _recording = false;
            _writer.Close();
            _writer.Dispose();
        }

        public void Dispose()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.Stop();
            }
            _writer?.Dispose();
        }
    }
}
