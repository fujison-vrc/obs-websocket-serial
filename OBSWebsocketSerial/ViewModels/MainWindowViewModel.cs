using Prism.Mvvm;
using Prism.Commands;
using System.Collections.Generic;
using System.Diagnostics;
using OBSWebsocketSerial.Models;
using System;

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
            set { SetProperty(ref _obsUsePasswordCheck, value); }
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

        private void ObsToggleConnection()
        {
            //
        }

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
                int baudRate;
                if (int.TryParse(SerialPortBaudRateText, out baudRate))
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
        }
    }
}
