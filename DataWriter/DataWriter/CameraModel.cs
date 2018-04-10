using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using Accord.Video.FFMPEG;
using AForge.Video;
using Microsoft.Win32;

namespace DataWriter
{
    public class CameraModel: IDisposable
    {
        #region Private fields
        private string _cameraUrl;
        private int _index;
        private bool _recording;

        private IVideoSource _videoSource;

        private BitmapImage _image;
        private VideoFileWriter _writer;
        private DateTime? _firstFrameTime;
        private FrameReceivedCallback SetReceivedFrame;
        #endregion

        #region Properties

        public CameraModel(string CameraURL, int IndexCamera, FrameReceivedCallback OnFrameReceived)
        {
            this._cameraUrl = CameraURL;
            this._index = IndexCamera;
            this.SetReceivedFrame = OnFrameReceived;
        }

        public delegate void FrameReceivedCallback(BitmapImage Frame, int IndexCamera);

        #endregion

        public void StartCamera()
        {
            _videoSource = new MJPEGStream(_cameraUrl);
            _videoSource.NewFrame += video_NewFrame;
            _videoSource.Start();
        }

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
                    _image = bitmap.ToBitmapImage();
                    _image.Freeze();
                    SetReceivedFrame(_image, _index);
                }
            }
            catch (Exception exc)
            {
                StopCamera();
                System.Windows.MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
                throw exc;
            }
        }

        public void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.Stop();
                _videoSource.NewFrame -= video_NewFrame;
            }
            SetReceivedFrame(null, _index);
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
