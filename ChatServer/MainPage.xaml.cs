// This is the built-in MAUI Content page class modified for use in our ChatServer project
using Communications;
using Microsoft.Extensions.Logging;
using System.Net;


namespace ChatServer
{
    public partial class MainPage : ContentPage
    {
        bool serverOpen = false;
        private readonly ILogger<MainPage> _logger;
        Networking serverNetwork;
        private string IP;
        Dictionary <Networking, string> clients;

        /// The Delegate methods used by the Networking Class
        public delegate void ReportMessageArrived(Networking channel, string message);
        public delegate void ReportDisconnect(Networking channel);
        public delegate void ReportConnectionEstablished(Networking channel);

        /// <summary>
        /// The follwoing is a constructor which gets the name of the local host using the 
        /// Dns.GetHostName() method and assigns it to a variable named host. Then gets the IP address 
        /// of the local host using the Dns.GetHostEntry() method, which takes the host name as an argument. 
        /// The result is assigned to a variable named ip. And then get the ip in to an address list in IP, and makes
        /// a new networking object
        /// </summary>
        /// <param name="logger"></param>
        public MainPage(ILogger<MainPage> logger)
        {
            string host = Dns.GetHostName();
            IPHostEntry ip = Dns.GetHostEntry(host);
            IP = ip.AddressList[1].ToString();

            clients = new Dictionary<Networking, string>();
            _logger = logger;
            InitializeComponent();
            ServerIP.Text = IP;
            _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - The server page has been opened! \n");
            serverNetwork = new Networking(_logger, onConnection, onDisconnect, onMessage, '\n');
        }

        /// <summary>
        /// An event handler for controlling the status of the server (whether it's on or off)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ChangeServerStatus(object sender, EventArgs args)
        {
            if (serverOpen == false)
            {
                ServerControl.Text = "Shutdown Server";
                serverOpen = true;
                _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - Server has just been Opened! \n");
                serverNetwork.WaitForClients(11000, true);
            }
            else
            {
                ServerControl.Text = "Start Server";
                serverOpen = false;
                _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - Server has just been Closed! \n");
                serverNetwork.StopWaitingForClients();
                foreach (Networking client in clients.Keys)
                {
                    Disconnect(client);
                }
            }
        }

        /// <summary>
        /// A delegate method which activates any time a connection has been made between the server and the client
        /// The following creats a lock on the clients object, which is a Dictionary object that associates instances
        /// of the Networking class with strings (as initialized in the constructor method). The purpose of the lock 
        /// is to prevent multiple threads from accessing the clients object concurrently, which could result in race 
        /// conditions and other thread safety issues. adds a new key-value pair to the clients dictionary, where the
        /// key is an instance of the Networking class (represented by the channel variable) and the value is the 
        /// ID of the channel (represented by the channel.ID property). Thus adding adds a new client to the chat server.
        /// Finally updates the list of participants on the chat server to include the new client that has joined.
        /// </summary>
        /// <param name="channel"></param>
        private void onConnection(Networking channel)
        {
            lock (clients)
            {
                clients.Add(channel, channel.ID);
            }
            ChatHistory.Text += $"\nNew User just connected!";
            _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - New user has just connected! \n");
        }

        /// <summary>
        /// A delegate method which activates any time there's a disconnection between a client and server
        /// The following creats a lock on the clients object, which is a Dictionary object that associates instances
        /// of the Networking class with strings (as initialized in the constructor method). The purpose of the lock 
        /// is to prevent multiple threads from accessing the clients object concurrently. In which afterward the channel
        /// is then disconnected and the server is closed.
        /// </summary>
        /// <param name="channel"></param>
        private void onDisconnect(Networking channel)
        {
            lock (clients)
            {
                clients.Remove(channel);
            }
            channel.Disconnect();
            _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - A user has disconnected! \n");
        }

        private void Disconnect(Networking channel)
        {
            string user = clients[channel];
            lock (clients)
            {
                clients.Remove(channel);
            }
            channel.Disconnect();
            ChatHistory.Text += ("\n" + $"{user} has disconnected!");
            ParticipantListServer.Text = "";
            refillParticipants();
        }

        /// <summary>
        /// A helper method meant to reconfigure the list of participants in the event that a user disconnects
        /// </summary>
        private void refillParticipants()
        {
            foreach (Networking client in clients.Keys)
            {
                ParticipantListServer.Text += $"{clients[client]} \n";
            }
        }

        /// <summary>
        /// A delegate method which activates any time a message arrives
        /// An event handler for incoming messages on the Networking channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        private void onMessage(Networking channel, string message)
        {
            // initializes an empty list of Networking objects called toSendTo.
            List<Networking> toSendTo = new();

            // checks if the incoming message starts with the string "command name"
            // (case-insensitive). If it does, the method executes the code inside the following braces.
            if (message.ToLower().StartsWith("command name"))
            {
                _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - Command Name detected! \n");

                // extracts the name from the message string by taking a substring of the message starting at
                // index 13 and with a length of message.Length - 13. This assumes that the message is
                // formatted as "command name [new name]".
                string newName = message.Substring(13,message.Length-13);
                Dispatcher.Dispatch(() => { ChatHistory.Text += $"\n{channel.ID} - {message}"; });
                if (newName != null)
                {
                    clients[channel] = newName;
                    channel.ID = newName;
                    ParticipantListServer.Text += $"{newName} \n";
                }
            }
            else
            {
                // checks if the incoming message starts with the string "command participants" (case-insensitive).
                // If it does, the method executes the code inside the following braces.
                if (message.ToLower().StartsWith("command participants"))
                {
                    _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - Command Participants detected! \n");

                    // initializes a string called clientList with the value "Command Participants". Which
                    // iterates through each Networking object in the clients dictionary.
                    string clientList = "Command Participants";
                    Dispatcher.Dispatch(() => { ChatHistory.Text += $"\n{channel.ID} - {message}"; }) ;
                    foreach (Networking client in clients.Keys)
                    {
                        // appends a comma-separated list of client IDs to the clientList string
                        clientList += $",{clients[client]}";
                    }
                    channel.Send(clientList.ToString());
                }
                else
                {
                    _logger.LogInformation($"{DateTime.Now} - {Environment.CurrentManagedThreadId} - Infor - Message recieved! \n");

                    // updates the chat history by appending the message string with a leading newline character.
                    string display = $"{channel.ID} - {message}";
                    Dispatcher.Dispatch(() => { ChatHistory.Text += "\n" + display; });
                    //
                    // Cannot have clients adding while we send messages, so make a copy of the
                    // current list of clients.
                    //

                    // acquires a lock on the clients object, which is a Dictionary object that associates instances of the Networking class with strings
                    lock (clients)
                    {
                        // adds the current client object to the toSendTo list.
                        foreach (Networking client in clients.Keys)
                        {
                            toSendTo.Add(client);
                        }
                    }

                    foreach (var client in toSendTo)
                    {
                        // sends the display string to each Networking object in the toSendTo list.
                        client.Send(display);
                    }
                }
            }   
        }
    }
}