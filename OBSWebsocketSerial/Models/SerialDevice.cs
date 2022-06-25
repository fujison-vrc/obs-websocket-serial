using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Diagnostics;

namespace OBSWebsocketSerial.Models
{
    public class SerialDevice
    {
        private SerialPort _serialPort;

        public string PortName { get; set; } = null;
        public int BaudRate { get; set; } = 9600;

        public delegate void MessageEventHandler(object sender, string data);
        public delegate void ErrorEventHandler(object sender, Exception ex);

        public event EventHandler Opened;
        public event EventHandler Closed;
        public event MessageEventHandler MessageReceived;
        public event ErrorEventHandler Errored;

        public bool IsOpen
        {
            get
            {
                if (_serialPort == null) return false;
                return _serialPort.IsOpen;
            }
        }

        public static List<string> PortList
        {
            get => SerialPort.GetPortNames().ToList();
        }

        public SerialDevice()
        {
            _serialPort = new SerialPort();
        }

        public void Connect()
        {
            if (string.IsNullOrEmpty(PortName)) return;

            if (_serialPort == null) _serialPort = new SerialPort();

            try
            {
                _serialPort.PortName = PortName;
                _serialPort.BaudRate = BaudRate;

                _serialPort.DataReceived += DataReceived;

                _serialPort.Open();
            }
            catch (Exception ex)
            {
                if (Errored != null) Errored(this, ex);
            }
            finally
            {
                if (_serialPort.IsOpen)
                {
                    if (Opened != null) Opened(_serialPort, EventArgs.Empty);
                }
                else
                {
                    if (Closed != null) Closed(_serialPort, EventArgs.Empty);
                    _serialPort = null;
                }
            }
        }

        public void Disconnect()
        {
            if (_serialPort == null) return;

            if (!_serialPort.IsOpen) return;

            try
            {
                _serialPort.Close();
            }
            catch (Exception ex)
            {
                if (Errored != null) Errored(this, ex);
            }
            finally
            {
                if (!_serialPort.IsOpen) Closed(_serialPort, EventArgs.Empty);
                _serialPort = null;
            }
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;

            string message = sp.ReadLine().Replace("\r", "");

            if (MessageReceived != null) MessageReceived(_serialPort, message);
        }
    }
}
