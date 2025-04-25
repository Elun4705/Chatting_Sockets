// This is the built-in Maui Content page modified for use in our ChatClient class
using Communications;
using Microsoft.Extensions.Logging;
using Windows.System;

namespace ChatClient
{
    public partial class MainPage : ContentPage
    {

        /// The Delegate methods used by the Networking Class
        public delegate void ReportMessageArrived(Networking channel, string message);
        public delegate void ReportDisconnect(Networking channel);
        public delegate void ReportConnectionEstablished(Networking channel);

        private readonly ILogger<MainPage> _logger;
        Networking clientNetwork = null;
        bool connected = false;

        /// <summary>
        /// This is a constructor method, which uses the _logger to log an informational message using the _logger object.
        /// </summary>
        /// <param name="logger"></param>
        public MainPage(ILogger<MainPage> logger)
        {
            _logger = logger;
            InitializeComponent();
            _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - Chat Client Constructor \n");
        }

        /// <summary>
        /// An event handler which sends a user-written message initializes the
        /// components of the MainPage class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void SendUserMessage(object sender, EventArgs args)
        {
            if (connected) 
                {
                if(UserMessage.Text != null && UserMessage.Text != "")
                {
                    _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - A message has been sent! \n");

                    clientNetwork.Send($"{UserMessage.Text}");
                    UserMessage.Text = "";
                }
                else
                {
                    ChatHistory.Text += "\nNo spamming in the chat, please!";
                }

            }
            else
            {
                ChatHistory.Text += "\nConnect to a server first!";
            }
        }

        /// <summary>
        /// An event handler which asks the server to return a list of participants. Checks if the
        /// client network is connect to a server, if it is it request a command, if not it proceeds to
        /// ask for the client ot connect to a server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void GetAllParticipants(object sender, EventArgs args)
        {
            if(clientNetwork != null)
            {
                _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - Requesting participant list! \n");

                clientNetwork.Send("Command Participants");
            }
            else
            {
                ChatHistory.Text += "\nConnect to a server first!";
            }
        }

        /// <summary>
        /// An event handler which allows a user to connect to the server
        /// If the user enter a username, it's then allowed to communicate to a server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ClientConnectToServer(object sender, EventArgs args)
        {
            if (UserName.Text != "" && UserName.Text != null)
            {
                _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - Attempting to connect to server... \n");
                try
                {
                    clientNetwork = new Networking(_logger, onConnection, onDisconnect, onMessage, '\n');
                    clientNetwork.ID = UserName.Text;
                    clientNetwork.Connect("localhost", 11000);
                    clientNetwork.AwaitMessagesAsync(true);
                }
                catch (Exception e)
                {
                    clientNetwork= null;
                    ChatHistory.Text += "\nServer not found.";
                }
                
            }
            else
            {
                ChatHistory.Text += "\nEnter a username first!";
            }
        }

        /// <summary>
        /// A delegate method which activates any time a connection has been made between the server and the client
        /// </summary>
        /// <param name="channel"></param>
        private void onConnection(Networking channel)
        {
            _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - User has connected! \n");

            ConnectToServer.IsVisible = false;
            ConnectionStatus.IsVisible = true;
            connected = true;
            clientNetwork.Send($"Command Name {clientNetwork.ID}");
        }

        /// <summary>
        /// A delegate method which activates any time there's a disconnection between a client and server
        /// </summary>
        /// <param name="channel"></param>
        private void onDisconnect(Networking channel)
        {
            _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - User has Disconnected! \n");

            channel.Disconnect();
            ConnectToServer.IsVisible = true;
            ConnectionStatus.IsVisible = false;
            connected = false;
        }

        /// <summary>
        /// A delegate method which activates any time a message arrives
        /// Adds the message to the chat history in a new line
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        private void onMessage(Networking channel, string message)
        {
            // checks if the incoming message starts with the string "command participants" (case-insensitive).
            // If it does, the method executes the code inside the following braces.
            if (message.ToLower().StartsWith("command participants"))
            {
                _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - Command participants detected! \n");

                // initializes a string called clientList with the value "Command Participants". Which
                // iterates through each Networking object in the clients dictionary.
                string[] participants = message.Split(',');
                for (int i = 1; i < participants.Length; i++) {
                    ParticipantList.Text += $" {participants[i]} \n";
                }
            }
            else
            {
                Dispatcher.Dispatch(() => { this.ChatHistory.Text += "\n" + message; });

            }
        }
    }
}