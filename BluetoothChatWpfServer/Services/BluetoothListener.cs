using System;
using System.IO;
using System.Net.Sockets; // for NetworkStream
using System.Collections.Generic; // for List
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using InTheHand.Net.Sockets;

namespace BluetoothChatWpfServer.Services
{
    /// <summary>
    /// Define the receiver Bluetooth service.
    /// </summary>
    public class BtChatListener : ObservableObject, IBtChatListener
    {
        private bool _ExitLoop = true;
        private BluetoothListener _Listener;
        private Guid _serviceClassId;
 
        //public delegate void dOnMessageBook(BluetoothClient xSender, ProtoBufExample.Header xHeader, ProtoBufExample.Book xBook);
        //public event dOnMessageBook OnMessageBook;
        public event EventHandler<BtChatBookEventArgs> MessageBookReceived;
 
        //public delegate void dOnMessageFable(BluetoothClient xSender, ProtoBufExample.Header xHeader, ProtoBufExample.Fable xFable);
        //public event dOnMessageFable OnMessageFable;
        public event EventHandler<BtChatFableEventArgs> MessageFableReceived;

        public event EventHandler<BtChatConnectedEventArgs> ClientConnected;

        public event EventHandler<BtChatConnectedEventArgs> ClientDisonnected;

        private List<BluetoothClient> _Clients = new List<BluetoothClient>();
 
        public int Port { get; private set; }
        public string ServiceClassId { get { return _serviceClassId.ToString(); } private set { _serviceClassId = new Guid(value); } }
        public string ThreadName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BluetoothListener" /> class.
        /// </summary>
        public BtChatListener()
        {
            _serviceClassId = new Guid("34b1cf4d-1069-4ad6-89b6-e161d79be4d8");
            ThreadName = "BtChatListener";
        } //

        /*public BtChatListener(string serviceClassId, string xThreadName)
        {
            //"34b1cf4d-1069-4ad6-89b6-e161d79be4d8");
            _serviceClassId = new Guid(serviceClassId);
            ThreadName = xThreadName;
        } //*/
 
        public bool Start() {
            if (!_ExitLoop) {
                Console.WriteLine("Listener running already");
                return false;
            }
            _ExitLoop = false;
 
            try {
                _Listener = new BluetoothListener(_serviceClassId)
                {
                    ServiceName = "MyService"
                };
                _Listener.Start();
 
                Thread lThread = new Thread(new ThreadStart(LoopWaitingForClientsToConnect));
                lThread.IsBackground = true;
                lThread.Name = ThreadName + "WaitingForClients";
                lThread.Start();
 
                return true;
            }
            catch (Exception ex) { 
                Console.WriteLine(ex.Message);
            }
            return false;
        } //
 
        public void Stop() {
            _ExitLoop = true;
            lock (_Clients) {
            foreach (BluetoothClient lClient in _Clients) lClient.Close();
            _Clients.Clear();
            }
        } //        
 
        private void LoopWaitingForClientsToConnect() {
            try {
                while (!_ExitLoop) {
                    Console.WriteLine("waiting for a client");
                    BluetoothClient lClient = _Listener.AcceptBluetoothClient();
                    string lClientIpAddress = lClient.Client.LocalEndPoint.ToString();
                    Console.WriteLine("new client connecting: " + lClientIpAddress);
                    if (_ExitLoop) break;
                    lock (_Clients) _Clients.Add(lClient);
 
                    Thread lThread = new Thread(new ParameterizedThreadStart(LoopRead));
                    lThread.IsBackground = true;
                    lThread.Name = ThreadName + "CommunicatingWithClient";
                    lThread.Start(lClient);
                    OnClientConnected(new BtChatConnectedEventArgs(lClientIpAddress));
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            finally {
                _ExitLoop = true;
                if (_Listener != null) _Listener.Stop();
            }
        } // 
 
        private void LoopRead(object xClient) {
            BluetoothClient lClient = xClient as BluetoothClient;
            NetworkStream lNetworkStream = lClient.GetStream();
 
            while (!_ExitLoop) {
            try {
                ProtoBufExample.Header lHeader = ProtoBuf.Serializer.DeserializeWithLengthPrefix<ProtoBufExample.Header>(lNetworkStream, ProtoBuf.PrefixStyle.Fixed32);
                if (lHeader == null) break; // happens during shutdown process
                switch (lHeader.objectType) {
 
                    case ProtoBufExample.eType.eBook:
                        ProtoBufExample.Book lBook = ProtoBuf.Serializer.DeserializeWithLengthPrefix<ProtoBufExample.Book>(lNetworkStream, ProtoBuf.PrefixStyle.Fixed32);
                        if (lBook == null) break;
                        lHeader.data = lBook; // not necessary, but nicer                            

                        OnMessageBookReceived(new BtChatBookEventArgs(lHeader, lBook));
                        break;
 
                    case ProtoBufExample.eType.eFable:
                        ProtoBufExample.Fable lFable = ProtoBuf.Serializer.DeserializeWithLengthPrefix<ProtoBufExample.Fable>(lNetworkStream, ProtoBuf.PrefixStyle.Fixed32);
                        if (lFable == null) break;
                        lHeader.data = lFable; // not necessary, but nicer                            

                        OnMessageFableReceived(new BtChatFableEventArgs(lHeader, lFable));
                        break;
 
                    default:
                        Console.WriteLine("Mayday, mayday, we are in big trouble.");
                        break;
                }
            }
            catch (System.IO.IOException) {
                if (_ExitLoop) Console.WriteLine("user requested client shutdown");
                else Console.WriteLine("disconnected");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
            Console.WriteLine("server: listener is shutting down");
        } //

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnMessageBookReceived(BtChatBookEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<BtChatBookEventArgs> handler = MessageBookReceived;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                //e.Message += String.Format(" at {0}", DateTime.Now.ToString());

                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnMessageFableReceived(BtChatFableEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<BtChatFableEventArgs> handler = MessageFableReceived;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                //e.Message += String.Format(" at {0}", DateTime.Now.ToString());

                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnClientConnected(BtChatConnectedEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<BtChatConnectedEventArgs> handler = ClientConnected;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                //e.Message += String.Format(" at {0}", DateTime.Now.ToString());

                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnClientDisonnected(BtChatConnectedEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<BtChatConnectedEventArgs> handler = ClientDisonnected;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                //e.Message += String.Format(" at {0}", DateTime.Now.ToString());

                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
 
        public void SendBook(int iClient) {
            if (iClient < 0 || _Clients.Count < iClient) return;
            BluetoothClient xClient = _Clients[iClient];
            if (xClient == null) return;
            ProtoBufExample.Header xHeader;
            ProtoBufExample.Book lBook = ProtoBufExample.GetData();
            xHeader = new ProtoBufExample.Header(lBook, ProtoBufExample.eType.eBook);

            Send(xClient, xHeader);
        }

        public void SendFable(int iClient) {
            if (iClient < 0 || _Clients.Count < iClient) return;
            BluetoothClient xClient = _Clients[iClient];
            if (xClient == null) return;
            ProtoBufExample.Header xHeader;
            ProtoBufExample.Book lBook = ProtoBufExample.GetData();
            xHeader = new ProtoBufExample.Header(lBook.stories[1], ProtoBufExample.eType.eFable);

            Send(xClient, xHeader);
        }

        private void Send(BluetoothClient xClient, ProtoBufExample.Header xHeader)
        {
            if (xClient == null) return;
            if (xHeader == null) return;

            lock (xClient)
            {
                try
                {
                    NetworkStream lNetworkStream = xClient.GetStream();

                    // send header (most likely a simple feedback)
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<ProtoBufExample.Header>(lNetworkStream, xHeader, ProtoBuf.PrefixStyle.Fixed32);

                    // send errors
                    if (xHeader.objectType != ProtoBufExample.eType.eError) return;
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<ProtoBufExample.ErrorMessage>(lNetworkStream, (ProtoBufExample.ErrorMessage)xHeader.data, ProtoBuf.PrefixStyle.Fixed32);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        /*
        public event Action<string> RecievedMessageEvent;

        private void OnRecievedMessage(string message)
        {
            if (RecievedMessageEvent != null) RecievedMessageEvent.Invoke(message);
        }

        public event Action<int> ConnectedEvent;

        private void OnConnected()
        {
            if (ConnectedEvent != null) ConnectedEvent.Invoke(0);
        }

        public event Action<int> DisconnectedEvent;

        private void OnDisconnected()
        {
            if (DisconnectedEvent != null) DisconnectedEvent.Invoke(0);
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        /// <summary>
        /// Listeners the accept bluetooth client.
        /// </summary>
        /// <param name="token">
        /// The token.
        /// </param>
        private void Listener(CancellationTokenSource token)
        {
            try
            {
                while (true)
                {
                    using (var client = _listener.AcceptBluetoothClient())
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                        OnConnected();
                        using (var stream = client.GetStream()) //using needed to Dispose object after clause.
                        {
                            do
                            {
                                // Read message length
                                int bytesRead = 0;
                                bytesRead = stream.Read(_buffer, 0, sizeof(UInt32));
                                if (bytesRead < 1) break;
                                int strLen = (int)BitConverter.ToUInt32(_buffer, 0);
                                byte[] strBuf = new byte[strLen];
                                bytesRead = stream.Read(strBuf, 0, sizeof(byte) * strLen);
                                if (bytesRead < 1) break;
                                string message = GetString(strBuf);
                                OnRecievedMessage(message);
                                //_responseAction("read");
                            }
                            while (true);
                        }
                        OnDisconnected();
                    }
                }
            }
            catch (Exception exception)
            {
               // todo handle the exception
            }
        }*/

    }
}
