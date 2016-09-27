using System;
using System.ComponentModel;
using InTheHand.Net.Sockets; // for BluetoothClient

namespace BluetoothChatWpfServer.Services
{
   public class BtChatBookEventArgs : EventArgs
    {
        public ProtoBufExample.Header _xHeader { get; private set; }
        public ProtoBufExample.Book _xBook { get; private set; }
        public BtChatBookEventArgs(ProtoBufExample.Header xHeader, ProtoBufExample.Book xBook)
        {
            _xHeader = xHeader;
            _xBook = xBook;
        }
    }

    public class BtChatFableEventArgs : EventArgs
    {
        public ProtoBufExample.Header _xHeader { get; private set; }
        public ProtoBufExample.Fable _xFable { get; private set; }
        public BtChatFableEventArgs(ProtoBufExample.Header xHeader, ProtoBufExample.Fable xFable)
        {
            _xHeader = xHeader;
            _xFable = xFable;
        }
    }

    public class BtChatConnectedEventArgs : EventArgs
    {
        public string _xClientIpAddress { get; private set; }
        public BtChatConnectedEventArgs(string xClientIpAddress)
        {
            _xClientIpAddress = xClientIpAddress;
        }
    }

    /// <summary>
    /// Define the receiver Bluetooth service interface.
    /// </summary>
    public interface IBtChatListener
    {
        //delegate void dOnMessageBook(BluetoothClient xSender, ProtoBufExample.Header xHeader, ProtoBufExample.Book xBook);
        //event dOnMessageBook OnMessageBook;
        event EventHandler<BtChatBookEventArgs> MessageBookReceived;

        //delegate void dOnMessageFable(BluetoothClient xSender, ProtoBufExample.Header xHeader, ProtoBufExample.Fable xFable);
        //event dOnMessageFable OnMessageFable;
        event EventHandler<BtChatFableEventArgs> MessageFableReceived;

        event EventHandler<BtChatConnectedEventArgs> ClientConnected;

        event EventHandler<BtChatConnectedEventArgs> ClientDisonnected;

        /// <summary>
        /// Starts the listening from Senders.
        /// </summary>
        /// <param name="reportAction">
        /// The report Action.
        /// </param>
        bool Start(); //Action<string> reportAction);

         /// <summary>
        /// Stops the listening from Senders.
        /// </summary>
        void Stop();

        void SendBook(int iClient);

        void SendFable(int iClient);

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        event PropertyChangedEventHandler PropertyChanged;
    }
}