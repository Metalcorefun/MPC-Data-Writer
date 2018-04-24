using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;

namespace DataWriter
{
    public class ListItem
    {
        public ListItem()
        {
            Name = "Empty Name";
            Index = 0;
        }
        public string Name { get; set; }
        public int Index { get; set; }

    }

    public class MainWindowController : ObservableObject, IDisposable
    {
        #region Private fields
        private BitmapImage _image_1;
        private BitmapImage _image_2;

        private string _camera_url1;
        private string _camera_url2;

        private DateTime _startRecDateTime;
        private DispatcherTimer _dispatcherTimer;
        private string _timer;
        private string _path;

        private CameraModel _camera_1;
        private CameraModel _camera_2;
        private DianaModel _diana;
        private AudioModel _audio;
        #endregion

        #region Public fields
        public MainWindowController()
        {
            // 101 - top, 102 - bottom?
            _camera_url1 = "http://192.168.0.101/axis-cgi/mjpg/video.cgi?fps=25";
            _camera_url2 = "http://192.168.0.102/axis-cgi/mjpg/video.cgi?fps=25";
            /* Для тестирования
             * http://192.168.0.101/axis-cgi/mjpg/video.cgi?fps=25
             * http://192.168.0.102/axis-cgi/mjpg/video.cgi?fps=25
             * http://212.162.177.75/axis-cgi/mjpg/video.cgi
             * http://88.53.197.250/axis-cgi/mjpg/video.cgi
             */
            //Объекты класса
            _camera_1 = new CameraModel(_camera_url1, 1);
            _camera_2 = new CameraModel(_camera_url2, 2);
            _audio = new AudioModel();
            _diana = new DianaModel();
            //Настройка объектов
            _camera_1.PropertyChanged += (sender, args) => Image_1 = _camera_1.Image;
            _camera_2.PropertyChanged += (sender, args) => Image_2 = _camera_2.Image;
            
            //Настроиваем команды
            StartSourceCommand = new RelayCommand(StartCameras);
            StopSourceCommand = new RelayCommand(StopCameras);
            StartRecordingCommand = new RelayCommand(StartRecording);
            StopRecordingCommand = new RelayCommand(StopRecording);
            SelectPathCommand = new RelayCommand(SelectPath);
            RefreshAudioDeviceCommand = new RelayCommand(_audio.RefreshAudioDevices);
            TestModeButtonCommand = new RelayCommand(_diana.TestMode);
            OptionalButtonCommand = new RelayCommand(_diana.Optional_Mode);
        }

        public BitmapImage Image_1
        {
            get { return _image_1; }
            set { this._image_1 = value; }
        }
        public BitmapImage Image_2
        {
            get { return _image_2; }
            set { this._image_2 = value; }
        }
        public string CameraUrl1 { set { this._camera_url1 = value; } get { return this._camera_url1; } }
        public string CameraUrl2 { set { this._camera_url2 = value; } get { return this._camera_url2; } }
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
        public List<ListItem> AudioList { get; set; }
        public List<ListItem> DianaList { get; set; }
        public string Timer { get; set; }
        public ICommand StartRecordingCommand { get; private set; }
        public ICommand StopRecordingCommand { get; private set; }
        public ICommand StartSourceCommand { get; private set; }
        public ICommand StopSourceCommand { get; private set; }
        public ICommand SelectPathCommand { get; private set; }
        public ICommand RefreshAudioDeviceCommand { get; private set; }
        public ICommand TestModeButtonCommand { get; private set; }
        public ICommand OptionalButtonCommand { get; private set; }
        #endregion

        #region Region camera
        private void StartCameras()
        {
            try
            { 
                _camera_1.StartCamera();
                _camera_2.StartCamera();
            }
            catch
            {
                _camera_1.StopCamera();
                _camera_2.StopCamera();
            }
        }

        private void StopCameras()
        {
            _camera_1.StopCamera();
            _camera_2.StopCamera();
        }
        #endregion

        private void StartRecording()
        {
            try
            {
                _startRecDateTime = DateTime.Now;
                _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                _dispatcherTimer.Tick += dispatcherTimer_Tick;
                _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                _dispatcherTimer.Start();

                //csvSender = new CSVSender(tbPath.Text);
                //Start_Diana();
                //StartAudioRecording(tbPath.Text);
                //cameraFace.StartCameraRecording(tbPath.Text);
                //cameraBody.StartCameraRecording(tbPath.Text);

                //RecStatusTextBlock.Text = "Запись";
            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
        }

        private void StopRecording()
        {
            try
            {
                if (_dispatcherTimer != null)
                {
                    _dispatcherTimer.Stop();
                }
                //StopAudioRecording();
                //Stop_Diana();
                //cameraFace.StopCameraRecording();
            //    cameraBody.StopCameraRecording();
            //    RecStatusTextBlock.Text = "Остановлена";
            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show("Error on Stop Recording:\n" + exc.Message, "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
            }
        }
        private void SelectPath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var dialogresult = dialog.ShowDialog();
            if (dialogresult == System.Windows.Forms.DialogResult.OK)
            {
                Path = dialog.SelectedPath.ToString();
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DateTime Now = DateTime.Now;
            Timer = (Now.Subtract(_startRecDateTime).ToString());
        }

        public void Dispose()
        {
            _camera_1.Dispose();
            _camera_2.Dispose();
            _diana.Dispose();
            _audio.Dispose();
        }
    }
}
