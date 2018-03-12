using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Accord.Video.FFMPEG;
using AForge.Video;
using AForge.Video.DirectShow;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;

namespace DataWriter
{
    internal class MainWindowViewModel : ObservableObject, IDisposable
    {
        #region //Private Fields//
        private BitmapImage _image;
        private BitmapImage _image2;

        private string _ipCameraUrl;
        private string _ipCameraUrl2;

        private string _folderPath;
        
        private IVideoSource _videoSource;
        private IVideoSource _videoSource2;

        private VideoFileWriter _writer;
        private VideoFileWriter _writer2;

        private bool _recording;

        private DateTime? _firstFrameTime;
        private DateTime? _firstFrameTime2;
        #endregion


        public MainWindowViewModel()
        {
            StartSourceCommand = new RelayCommand(StartVideoSource);
            StopSourceCommand = new RelayCommand(StopVideoSource);
            StartRecordingCommand = new RelayCommand(StartVideoRecording);
            StopRecordingCommand = new RelayCommand(StopVideoRecording);
            IpCameraUrl = "http://88.53.197.250/axis-cgi/mjpg/video.cgi";
            IpCameraUrl2 = "http://212.162.177.75/axis-cgi/mjpg/video.cgi";
            FolderPath = "C:\\Users\\pingv\\Desktop\\Test";
        }

        public ICommand StartRecordingCommand { get; private set; }

        public ICommand StopRecordingCommand { get; private set; }

        public ICommand StartSourceCommand { get; private set; }

        public ICommand StopSourceCommand { get; private set; }

        public string FolderPath
        {
            get { return _folderPath; }
            set { Set(ref _folderPath, value); }
        }

        public void Dispose()
        {
            if ((_videoSource != null && _videoSource.IsRunning) || (_videoSource2 != null && _videoSource2.IsRunning))
            {
                _videoSource.Stop();
                _videoSource2.Stop();
            }
            _writer?.Dispose();
            _writer2?.Dispose();
        }

        #region //Camera 1//

        public BitmapImage Image
        {
            get { return _image; }
            set { Set(ref _image, value); }
        }

        public string IpCameraUrl
        {
            get { return _ipCameraUrl; }
            set { Set(ref _ipCameraUrl, value); }
        }
        
        private void StartCamera()
        {
            _videoSource = new MJPEGStream(IpCameraUrl);
            _videoSource.NewFrame += video_NewFrame;
            _videoSource.Start();
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (_recording)
                {
                    using (var bitmap = (Bitmap) eventArgs.Frame.Clone())
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
                using (var bitmap = (Bitmap) eventArgs.Frame.Clone())
                {
                    var bi = bitmap.ToBitmapImage();
                    bi.Freeze();
                    Dispatcher.CurrentDispatcher.Invoke(() => Image = bi);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StopCamera();
            }
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.Stop();
                _videoSource.NewFrame -= video_NewFrame;
            }
            Image = null;
        }

        private void StopRecording()
        {
            _writer.Close();
            _writer.Dispose();
        }

        private void StartRecording()
        {
            string timestamp = DateTime.Now.ToString();
            timestamp = timestamp.Replace(':', '_');

            string FileName = FolderPath + "\\1_" + timestamp + ".avi";
            _firstFrameTime = null;
            _writer = new VideoFileWriter();
            _writer.Open(FileName, (int)Math.Round(Image.Width, 0), (int)Math.Round(Image.Height, 0));
        }
        #endregion

        #region //Camera 2//
        public BitmapImage Image2
        {
            get { return _image2; }
            set { Set(ref _image2, value); }
        }

        public string IpCameraUrl2
        {
            get { return _ipCameraUrl2; }
            set { Set(ref _ipCameraUrl2, value); }
        }
        
        private void StartCamera2()
        {
            _videoSource2 = new MJPEGStream(IpCameraUrl2);
            _videoSource2.NewFrame += video_NewFrame2;
            _videoSource2.Start();
        }

        private void video_NewFrame2(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (_recording)
                {
                    using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                    {
                        if (_firstFrameTime2 != null)
                        {
                            _writer2.WriteVideoFrame(bitmap, DateTime.Now - _firstFrameTime2.Value);
                        }
                        else
                        {
                            _writer2.WriteVideoFrame(bitmap);
                            _firstFrameTime2 = DateTime.Now;
                        }
                    }
                }
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    var bi = bitmap.ToBitmapImage();
                    bi.Freeze();
                    Dispatcher.CurrentDispatcher.Invoke(() => Image2 = bi);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StopCamera2();
            }
        }

        private void StopCamera2()
        {
            if (_videoSource2 != null && _videoSource2.IsRunning)
            {
                _videoSource2.Stop();
                _videoSource2.NewFrame -= video_NewFrame2;
            }
            Image2 = null;
        }

        private void StopRecording2()
        {
            _writer2.Close();
            _writer2.Dispose();
        }

        private void StartRecording2()
        {
            string timestamp = DateTime.Now.ToString();
            timestamp = timestamp.Replace(':', '_');
            string FileName = FolderPath + "\\2_" + timestamp + ".avi";
            _firstFrameTime2 = null;
            _writer2 = new VideoFileWriter();
            _writer2.Open(FileName, (int)Math.Round(Image2.Width, 0), (int)Math.Round(Image2.Height, 0)); ;
        }
        #endregion

        private void StartVideoSource()
        {
            StartCamera();
            StartCamera2();
        }

        private void StopVideoSource()
        {
            StopCamera();
            StopCamera2();
        }

        private void StartVideoRecording()
        {
            _recording = true;
            StartRecording();
            StartRecording2();
        }

        private void StopVideoRecording()
        {
            _recording = false;
            StopRecording();
            StopRecording2();
        }
    }
}