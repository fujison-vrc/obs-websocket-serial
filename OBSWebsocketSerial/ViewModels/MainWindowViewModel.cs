using Prism.Mvvm;
using Prism.Commands;
using System.Collections.Generic;
using System.Diagnostics;
using OBSWebsocketSerial.Models;
using System;
using Newtonsoft.Json.Linq;
using System.Windows;

namespace OBSWebsocketSerial.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "OBS Websocket Serial";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _statusBarText = string.Empty;
        public string StatusBarText
        {
            get { return _statusBarText; }
            set { SetProperty(ref _statusBarText, value); }
        }

        #region obs connection property

        // OBSホスト名/IPのテキスト
        private string _obsHostText = "localhost";
        public string ObsHostText
        {
            get { return _obsHostText; }
            set { SetProperty(ref _obsHostText, value); }
        }

        // OBSポートのテキスト
        private string _obsPortText = "4444";
        public string ObsPortText
        {
            get { return _obsPortText; }
            set { SetProperty(ref _obsPortText, value); }
        }

        // OBSパスワードを利用するかのチェックボックス
        private bool _obsUsePasswordCheck = false;
        public bool ObsUsePasswordCheck
        {
            get { return _obsUsePasswordCheck; }
            set
            {
                // OBSパスワード入力欄の表示/非表示を切り替え
                if (value)
                {
                    ObsPasswordControlVisibility = Visibility.Visible;
                }
                else
                {
                    ObsPasswordControlVisibility = Visibility.Collapsed;
                }

                SetProperty(ref _obsUsePasswordCheck, value);
            }
        }

        // OBSパスワードの入力欄の可視性
        private Visibility _obsPasswordControlVisibility = Visibility.Collapsed;
        public Visibility ObsPasswordControlVisibility
        {
            get { return _obsPasswordControlVisibility; }
            set { SetProperty(ref _obsPasswordControlVisibility, value); }
        }

        // OBSパスワードのテキスト
        private string _obsPasswordText = string.Empty;
        public string ObsPasswordText
        {
            get { return _obsPasswordText; }
            set { SetProperty(ref _obsPasswordText, value); }
        }

        // OBSステータスのテキスト
        private string _obsStatusText = "Not connected";
        public string ObsStatusText
        {
            get { return _obsStatusText; }
            set { SetProperty(ref _obsStatusText, value); }
        }

        // OBS接続/切断ボタンのテキスト
        private string _obsToggleConnectionButtonText = "Connect";
        public string ObsToggleConnectionButtonText
        {
            get { return _obsToggleConnectionButtonText; }
            set { SetProperty(ref _obsToggleConnectionButtonText, value); }
        }

        // OBS接続設定の入力可否
        private bool _obsConnectionInputable = true;
        public bool ObsConnectionInputable
        {
            get { return _obsConnectionInputable; }
            set { SetProperty(ref _obsConnectionInputable, value); }
        }

        // OBS接続ボタンの押下可否
        private bool _obsToggleConnectionButtonEnable = true;
        public bool ObsToggleConnectionButtonEnable
        {
            get { return _obsToggleConnectionButtonEnable; }
            set { SetProperty(ref _obsToggleConnectionButtonEnable, value); }
        }

        #endregion

        #region serial connection property

        // シリアルポート名のリスト
        private List<string> _serialPortNameList = new();
        public List<string> SerialPortNameList
        {
            get { return _serialPortNameList; }
            set { SetProperty(ref _serialPortNameList, value); }
        }

        // 選択中のシリアルポート
        private string _serialPortNameSelected;
        public string SerialPortNameSelected
        {
            get { return _serialPortNameSelected; }
            set { SetProperty(ref _serialPortNameSelected, value); }
        }

        // シリアルポートのボーレートリスト
        private List<string> _serialPortBaudRateList = new()
        {
            "9600", "19200", "38400", "57600", "115200"
        };
        public List<string> SerialPortBaudRateList
        {
            get { return _serialPortBaudRateList; }
            set { SetProperty(ref _serialPortBaudRateList, value); }
        }

        // ボーレートのテキスト
        private string _serialPortBaudRateText = "9600";
        public string SerialPortBaudRateText
        {
            get { return _serialPortBaudRateText; }
            set { SetProperty(ref _serialPortBaudRateText, value); }
        }

        // シリアルポートステータスのテキスト
        private string _serialPortStatusText = "Not connected";
        public string SerialPortStatusText
        {
            get { return _serialPortStatusText; }
            set { SetProperty(ref _serialPortStatusText, value); }
        }

        // シリアル接続/切断ボタンのテキスト
        private string _serialPortToggleConnectionButtonText = "Connect";
        public string SerialPortToggleConnectionButtonText
        {
            get { return _serialPortToggleConnectionButtonText; }
            set { SetProperty(ref _serialPortToggleConnectionButtonText, value); }
        }

        // シリアル接続設定の入力可否
        private bool _serialPortConnectionInputable = true;
        public bool SerialPortConnectionInputable
        {
            get { return _serialPortConnectionInputable; }
            set { SetProperty(ref _serialPortConnectionInputable, value); }
        }

        #endregion

        #region obs to serial property

        //

        #endregion

        #region serial to obs property

        //

        #endregion

        private ObsWebsocket _obsWebsocket;
        private SerialDevice _serialDevice;

        public MainWindowViewModel()
        {
            ObsToggleConnectionCommand = new DelegateCommand(ObsToggleConnection);
            SerialPortToggleConnectionCommand = new DelegateCommand(SerialPortToggleConnection);

            foreach (string device in SerialDevice.PortList)
            {
                SerialPortNameList.Add(device);
            }
        }

        public DelegateCommand ObsToggleConnectionCommand { get; }
        public DelegateCommand SerialPortToggleConnectionCommand { get; }

        #region obs connection method

        private void ObsToggleConnection()
        {
            if (_obsWebsocket == null)
            {
                // 接続処理

                _obsWebsocket = new();

                // ポート入力値が整数値以外の場合は接続しない
                if (!int.TryParse(ObsPortText, out int port))
                {
                    StatusBarText = "obs port must be an integer";
                    return;
                }

                // ポート入力値が範囲外の場合は接続しない
                if (port < 0 || port > 65535)
                {
                    StatusBarText = "out of port range ( <=0 , >= 65535 )";
                    return;
                }

                _obsWebsocket.Opened += ObsWebsocket_Opened;
                _obsWebsocket.Closed += ObsWebsocket_Closed;
                _obsWebsocket.Errored += ObsWebsocket_Errored;
                _obsWebsocket.Identified += ObsWebsocket_Identified;

                // OBS接続UIの更新
                ObsToggleConnectionButtonEnable = false;
                ObsConnectionInputable = false;
                ObsStatusText = "Connecting...";

                if (ObsUsePasswordCheck && !string.IsNullOrEmpty(ObsPasswordText))
                {
                    _obsWebsocket.Connect("ws", ObsHostText, port, ObsPasswordText);
                }
                else
                {
                    _obsWebsocket.Connect("ws", ObsHostText, port);
                }
            }
            else
            {
                // 切断処理

                if (_obsWebsocket.IsConnected)
                {
                    // OBS接続UIの更新
                    ObsToggleConnectionButtonEnable = false;
                    ObsConnectionInputable = false;
                    ObsStatusText = "Disconnecting...";

                    _obsWebsocket.Disconnect();
                }

                if (_obsWebsocket != null) _obsWebsocket = null;
            }
        }

        private void ObsWebsocket_Opened(object sender, EventArgs e)
        {
            UpdateObsConnectionUI();
        }

        private void ObsWebsocket_Closed(object sender, EventArgs e)
        {
            UpdateObsConnectionUI();

            if (_obsWebsocket != null) _obsWebsocket = null;
        }

        private void ObsWebsocket_Errored(object sender, Exception ex)
        {
            StatusBarText = ex.Message;

            UpdateObsConnectionUI();

            if (_obsWebsocket != null && !_obsWebsocket.IsConnected)
            {
                _obsWebsocket = null;
            }
        }

        private void ObsWebsocket_Identified(object sender, EventArgs e)
        {
            //JObject requestData = new()
            //{
            //    { "sceneName", "Scene02" }
            //};

            //_obsWebsocket.SendRequest("SetCurrentProgramScene", requestData);
        }

            private void UpdateObsConnectionUI()
        {
            bool isConnected = _obsWebsocket != null && _obsWebsocket.IsConnected;

            ObsStatusText = isConnected ? "Connected" : "Not connected";
            ObsToggleConnectionButtonText = isConnected ? "Disconnect" : "Connect";

            // OBS設定の入力無効化
            ObsConnectionInputable = !isConnected;

            // OBS接続ボタンの有効化
            ObsToggleConnectionButtonEnable = true;
        }

        #endregion

        #region serial connection method

        private void SerialPortToggleConnection()
        {
            if (_serialDevice != null && _serialDevice.IsOpen)
            {
                _serialDevice.Disconnect();

                _serialDevice = null;
            }
            else
            {
                if (string.IsNullOrEmpty(SerialPortNameSelected))
                {
                    Debug.WriteLine("serial port is not selected");
                    return;
                }

                if (_serialDevice == null) _serialDevice = new();

                // シリアルポート名を設定
                _serialDevice.PortName = SerialPortNameSelected;

                // ボーレートを設定
                if (int.TryParse(SerialPortBaudRateText, out int baudRate))
                {
                    _serialDevice.BaudRate = baudRate;
                }
                else
                {
                    Debug.WriteLine("baud rate must be an integer");
                    return;
                }

                // イベントを登録
                _serialDevice.Opened += SerialDevice_Opened;
                _serialDevice.Closed += SerialDevice_Closed;
                _serialDevice.Errored += SerialDevice_Errored;
                _serialDevice.MessageReceived += SerialDevice_MessageReceived;

                _serialDevice.Connect();

                if (!_serialDevice.IsOpen) _serialDevice = null;
            }
        }

        private void SerialDevice_Opened(object sender, EventArgs e)
        {
            SerialPortStatusText = "Connected";
            SerialPortToggleConnectionButtonText = "Disconnect";
            SerialPortConnectionInputable = false;

            Debug.WriteLine("SerialDevice_Opened");
        }

        private void SerialDevice_Closed(object sender, EventArgs e)
        {
            SerialPortStatusText = "Not connected";
            SerialPortToggleConnectionButtonText = "Connect";
            SerialPortConnectionInputable = true;

            Debug.WriteLine("SerialDevice_Closed");
        }

        private void SerialDevice_Errored(object sender, Exception ex)
        {
            Debug.WriteLine(ex.Message);
            StatusBarText = ex.Message;
        }

        private void SerialDevice_MessageReceived(object sender, string message)
        {
            Debug.WriteLine(message);
        }

        #endregion

        #region obs to serial method

        //

        #endregion

        #region serial to obs method

        //

        #endregion
    }
}
