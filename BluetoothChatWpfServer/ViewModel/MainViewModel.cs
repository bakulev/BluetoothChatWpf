using System; // for Action<string>
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using BluetoothChatWpfServer.Services;

namespace BluetoothChatWpfServer.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IBtChatListener _listenerService;

        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        #region TextBoxes

        #region StartListeningButtonText

        private string _startListeningButtonText = "Start listening";

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string StartListeningButtonText
        {
            get
            {
                return _startListeningButtonText;
            }
            set
            {
                Set(ref _startListeningButtonText, value);
            }
        }

        #endregion

        #region ListeningText

        private string _listeningText = "Start Listening";

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ListeningText
        {
            get
            {
                return _listeningText;
            }
            set
            {
                Set(ref _listeningText, value);
            }
        }

        #endregion

        #region MessageText

        private string _messageText = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string MessageText
        {
            get
            {
                return _messageText;
            }
            set
            {
                Set(ref _messageText, value);
            }
        }

        #endregion

        #region ChatLogText

        private string _chatLogText = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ChatLogText
        {
            get
            {
                return _chatLogText;
            }
            set
            {
                Set(ref _chatLogText, value);
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IBtChatListener listenerService)
        {
            /*_dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    WelcomeTitle = item.Title;
                });*/
            _listenerService = listenerService;
            _listenerService.PropertyChanged += ReceiverService_PropertyChanged;
            _listenerService.MessageBookReceived += SetBook;
            _listenerService.MessageFableReceived += SetFable;

            #region Commands
            this.StartListeningCommand = new RelayCommand(this.StartListening, CanStartListening);
            this.SendMessageCommand = new RelayCommand(this.SendMessage, CanSendMessage);
            #endregion

        }

        #region Commands

        #region StartListeningCommand

        public RelayCommand StartListeningCommand { get; private set; }

        public bool CanStartListening()
        {
            return true;
        }

        public void StartListening()
        {
            if (false) //_listenerService.WasStarted)
            {
                System.Console.WriteLine("StopListening");
                _listenerService.Stop();
                ListeningText = "Listening stoped.";
                StartListeningButtonText = "Start listening";
            }
            else
            {
                System.Console.WriteLine("StartListening");
                _listenerService.Start(); //SetData);
                ListeningText = "Listening started.";
                StartListeningButtonText = "Stop listening";
            }
        }

        /// <summary>
        /// The set data received.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        private void SetBook(object sender, BtChatBookEventArgs e)
        {
            var listener = sender as IBtChatListener;
            ChatLogText += e._xBook.ToString();
            ChatLogText += Environment.NewLine;
            //_listenerService.Send(data);
        }

        private void SetFable(object sender, BtChatFableEventArgs e)
        {
            var listener = sender as IBtChatListener;
            ChatLogText += e._xFable.ToString();
            ChatLogText += Environment.NewLine;
            //_listenerService.Send(data);
        }

        #endregion

        #region SendMessageCommand

        public RelayCommand SendMessageCommand { get; private set; }

        public bool CanSendMessage()
        {
            return true; // _listenerService.WasStarted;
        }

        public void SendMessage()
        {
            System.Console.WriteLine("SendMessage");
            MessageText = "Message Sended.";
            ChatLogText = "Log added.";
            //_listenerService.Send(xClient, new Header());
        }

        #endregion

        #endregion

        /// <summary>
        /// Handles the PropertyChanged event of the ReceiverBluetoothService control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ReceiverService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WasStarted")
            {
                RaisePropertyChanged();
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}