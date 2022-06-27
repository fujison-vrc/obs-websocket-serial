using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using WebSocket4Net;

namespace OBSWebsocketSerial.Models
{
    public class ObsWebsocket
    {
        private WebSocket _websocket;

        public delegate void ErrorEventHandler(object sender, Exception ex);

        public event EventHandler Opened;
        public event EventHandler Closed;
        public event ErrorEventHandler Errored;

        public bool IsConnected
        {
            get
            {
                if (_websocket == null) return false;
                if (_websocket.State != WebSocketState.Open) return false;

                return true;
            }
        }

        public void Connect(string scheme, string host, int port)
        {
            try
            {
                UriBuilder ub = new(scheme, host, port);
                _websocket = new(ub.ToString());

                _websocket.Opened += Opened;
                _websocket.Closed += Closed;

                _websocket.Open();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Errored?.Invoke(this, ex);
            }
        }

        public void Disconnect()
        {
            if (_websocket == null) return;

            try
            {
                _websocket.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Errored?.Invoke(this, ex);
            }
        }
    }
}
