using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using WebSocket4Net;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace OBSWebsocketSerial.Models
{
    public class ObsWebsocket
    {
        private WebSocket _websocket;

        public delegate void MessageEventHandler(object sender, string data);
        public delegate void ErrorEventHandler(object sender, Exception ex);

        public event EventHandler Opened;
        public event EventHandler Closed;
        public event EventHandler Identified;
        public event MessageEventHandler MessageReceived;
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
                _websocket.MessageReceived += websocket_MessageReceived;

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

        public void Send(int op, JObject d)
        {
            if (op == 6)
            {
                Guid guid = Guid.NewGuid();
                string requestID = guid.ToString();
                d.Add("requestId", requestID);
            }

            JObject requestJson = new()
                {
                    { "op", op },
                    { "d", d }
                };

            Debug.WriteLine(requestJson.ToString());
            _websocket.Send(requestJson.ToString());
        }

        public void SendRequest(string requestType, JObject requestData)
        {
            JObject requestJson = new()
            {
                { "requestType", requestType },
                //{ "requestId", "f819dcf0-89cc-11eb-8f0e-382c4ac93b9c" },
                { "requestData", requestData }
            };

            Send(6, requestJson);
        }

        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            JObject receivedJson = JObject.Parse(e.Message);
            Debug.WriteLine(receivedJson.ToString());

            JObject requestJson;

            // Hello (OpCode 0) 受信時
            if (receivedJson["op"].ToString() == "0")
            {
                // OBSでパスワードが設定されている場合
                if (receivedJson["d"]["authentication"] != null)
                {
                    Debug.WriteLine("password required");

                    Disconnect();
                    return;
                }

                requestJson = new()
                {
                    { "rpcVersion", 1 }
                };

                Send(1, requestJson);
            }

            // Identified (OpCode 2) 受信時
            if (receivedJson["op"].ToString() == "2")
            {
                Identified?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
