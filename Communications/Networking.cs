// This is a networking object implemented by Andy Huo and Emmanuel Luna based off of a given API in CS 3500.  This is not
// meant for use outside of this class.
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Communications
{
    /// <summary>
    /// A Networking class which serves as a general-purpose use for communication over a network via TCPclients
    /// </summary>
    public class Networking
    {
        /// The Delegate methods used by the Networking Class
        public delegate void ReportMessageArrived(Networking channel, string message);
        public delegate void ReportDisconnect(Networking channel);
        public delegate void ReportConnectionEstablished(Networking channel);

        private ReportMessageArrived messageArrived;
        private ReportDisconnect messageDisconnect;
        private ReportConnectionEstablished connectionEstablished;
        private ILogger log;
        private TcpClient client = new TcpClient();
        private TcpListener listener;
        private char termination;
        private CancellationTokenSource StopWaitingClients;

        public string ID { get; set; }

        /// <summary>
        /// Networking constructor that will will give access to networking process in the GUI
        /// </summary>
        /// <param name="logger"> Logging object provided by Deoendency injection from the main program </param>
        /// <param name="onConnect"> Delegate callback method </param>
        /// <param name="onDisconnect"> Delegate callback method </param>
        /// <param name="onMesage"> Delegate callback method </param>
        /// <param name="terminationCharacter"> Character defined to seperate one message from another (\n) </param>
        public Networking(ILogger logger, ReportConnectionEstablished onConnect, ReportDisconnect onDisconnect,
            ReportMessageArrived onMessage, char terminationCharacter)
        {
            this.log = logger;
            this.connectionEstablished = onConnect;
            this.messageDisconnect = onDisconnect;
            this.messageArrived = onMessage;
            this.termination = terminationCharacter;
            this.client = new TcpClient();
        }

        /// <summary>
        /// Create (and store) a TCP client object connected to the given host/port
        /// 
        /// Handle the exceptional cases of where the host/port is not avaiable
        /// 
        /// If when connecting usuing the TcpClient constructor an expcetion occurs, 
        /// throw this exception out of the method
        /// 
        /// This method will be used by clients when connecting.  It will not be used by Server (which will use
        /// WaitForClients)
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void Connect(string host, int port)
        {
            try
            {
                if (!this.client.Connected)
                {
                    this.client = new TcpClient(host, port);
                    this.log.LogInformation($"{DateTime.Now} - {System.Environment.CurrentManagedThreadId} - Infor - Client has connected to {this.client.Client.RemoteEndPoint}\n");
                    this.connectionEstablished(this);
                }
            }
            catch (Exception ex) 
            {
                log.LogInformation($"{DateTime.Now} - {System.Environment.CurrentManagedThreadId} - Infor - Unsuccessful attempt to connect to {host} on port {port}\n");
                log.LogInformation(ex, $"{DateTime.Now} - {System.Environment.CurrentManagedThreadId} - Infor - Exception found: {ex.Message}\n");
                throw;
            }
        }

        /// <summary>
        /// If inifnite is true, infinitely await for data to come in over the tcl client.  (You will need
        /// to use GetStream and ReadSync).  If false, just wait for a single message and then return.
        /// 
        /// If a full message(s) is recieved, sent it/them on to the "client code" via stored handle message callback
        /// 
        /// Handle the exceptional case where the tcp client disconnects while the Networking object is waiting to read data.
        /// If this happens, call the report disconnect callback.
        /// </summary>
        /// <param name="infinite"></param>
        public async void AwaitMessagesAsync(bool infinite)
        {
            try
            {
                StringBuilder dataBacklog = new StringBuilder();
                byte[] buffer = new byte[4096];
                NetworkStream stream = client.GetStream();

                if (stream == null)
                {
                    return;
                }
                if (infinite)
                {
                    while (infinite)
                    {
                        int total = await stream.ReadAsync(buffer);

                        if (total == 0)
                        {
                            throw new Exception("disconnected");
                        }

                        string current_data = Encoding.UTF8.GetString(buffer, 0, total);

                        dataBacklog.Append(current_data);

                        log.LogInformation($"{DateTime.Now} Received {total} new bytes for a total of {dataBacklog.Length}.\n");

                        this.CheckForMessage(dataBacklog);


                    }
                }
                else
                {
                    int total = await stream.ReadAsync(buffer);

                    string current_data = Encoding.UTF8.GetString(buffer, 0, total);

                    dataBacklog.Append(current_data);

                    total = 0;

                    log.LogInformation($"{DateTime.Now} Received {total} new bytes for a total of {dataBacklog.Length}.\n");

                    this.CheckForMessage(dataBacklog);

                }
                
            }
            catch (Exception ex)
            {
                messageDisconnect(this);
            }


        }

        /// <summary>
        ///   Given a string (actually a string builder object)
        ///   check to see if it contains one or more messages as defined by
        ///   our protocol (the newline character '\n')
        ///   
        /// The method first converts the StringBuilder to a string by calling the ToString method. It then searches for the position of a termination character (stored in a variable called termination) within the string.
        /// If the termination character is found, the method extracts the message that precedes it by using the Substring method and removes it from the StringBuilder by calling the Remove method.The method then raises an event called messageArrived, passing the extracted message as an argument.
        /// If the termination character is not found, the method simply writes a message to the console saying that no message was found.
        /// At the end of the method, the console prints a message indicating the number of bytes remaining in the StringBuilder.
        /// </summary>
        /// <param name="data"> all characters encountered so far</param>
        private void CheckForMessage(StringBuilder data)
        {
            string allData = data.ToString();
            int terminator_position = allData.IndexOf(termination);
            bool foundOneMessage = false;
            string message ="";

            while (terminator_position >= 0)
            {
                foundOneMessage = true;

                message = allData.Substring(0, terminator_position + 1);
                data.Remove(0, terminator_position + 1);
                log.LogInformation($"{DateTime.Now} - {System.Environment.CurrentManagedThreadId} - Infor - Message found:\n" +
                    $"  {message}\n");

                allData = data.ToString();
                terminator_position = allData.IndexOf("\n");
            }

            if (!foundOneMessage)
            {
                log.LogInformation($"{DateTime.Now} - {System.Environment.CurrentManagedThreadId} - Infor - Message NOT found\n");
            }
            else
            {
                messageArrived(this, message.Substring(0, message.Length-1));
                log.LogInformation(
                    $"{DateTime.Now} - {System.Environment.CurrentManagedThreadId} - Infor - bytes unprocessed.\n");
            }
        }

        /// <summary>
        /// Used by servers
        /// 
        /// Continuously wait for clients to connect (uses a TcpListener object which will create new TcpClient objects)
        /// 
        /// Build a new networking object *using the TcpClient object from above) to handle them and then await new messages
        /// from that client in a new threat
        /// 
        /// (infinite = used when the thread is awaiting client message (see above.)
        /// </summary>
        /// <param name="port"></param>
        /// <param name="infinite"></param>
        public async void WaitForClients(int port, bool infinite)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            try
            {
                StopWaitingClients = new();

                while (true)
                {
                    TcpClient connection = await listener.AcceptTcpClientAsync(StopWaitingClients.Token);
                    log.LogInformation($"{DateTime.Now} - {System.Environment.CurrentManagedThreadId} - Infor ** New Connection ** Accepted From {connection.Client.RemoteEndPoint} to {connection.Client.LocalEndPoint}\n");
                    Networking clientConnection = new Networking(log, connectionEstablished, messageDisconnect, messageArrived, termination);
                    clientConnection.client = connection;
                    clientConnection.ID = $"{client.Client.LocalEndPoint}";
                    this.connectionEstablished(clientConnection);
                    clientConnection.AwaitMessagesAsync(true);

                }
            }
            catch(Exception e)
            {
                listener.Stop();
            }
            
        }

        /// <summary>
        /// Cancel the previous method (we will discuss cancellation tokens in class)
        /// </summary>
        public void StopWaitingForClients()
        {
            StopWaitingClients.Cancel();
        }

        /// <summary>
        /// Close the connection to the remote host using the stored tcp client object
        /// </summary>
        public void Disconnect()
        {
            this.client.Close();
        }
     

        /// <summary>
        /// Send the text across the network using the (already connected) tcp client
        /// 
        /// Reminder: don't allow termination (the \n character) characters in the text.
        /// 
        /// Use WriteAsync (must send byte arrays, so use: Encoding.UTF8.GetBytes)
        /// 
        /// Handle the exception circumstance where the remote client disconnects as we are writing the 
        /// message = i.e., call the report disconnect callback in an exception occurs.
        /// </summary>
        /// <param name="text"></param>
        public async void Send(string text)
        {
            string message = text + '\n';
            log.LogInformation($"{DateTime.Now} - {System.Environment.CurrentManagedThreadId} - Infor - Sending a message of size ({message.Length}) to connected\n");
            if (client.Connected)
            {
                try
                {

                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                    await client.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                    log.LogInformation($"{DateTime.Now} - {System.Environment.CurrentManagedThreadId} - Infor - Message Sent from:   {client.Client.LocalEndPoint} to {client.Client.RemoteEndPoint}\n");
                }
                catch (SocketException ex)
                {
                    log.LogInformation($"{DateTime.Now} - {System.Environment.CurrentManagedThreadId} - Infor - Client Disconnected: {client.Client.RemoteEndPoint} - {ex.Message} \n");
                }
            }          
        }

        /// <summary>
        /// The GetIp() method retrieves the IP address of the host on which the application is running. It first retrieves the name of 
        /// the host using the Dns.GetHostName() method and then uses the Dns.GetHostEntry(string hostName) method to get the IP address information 
        /// associated with that host. It then returns the second IP address in the list of IP addresses associated with the host, which is 
        /// obtained using the AddressList property of the IPHostEntry object. The IP address is returned as a string.
        /// 
        /// NOTE! This method is no longer used within our code, for reasons of security.
        /// </summary>
        /// <returns></returns>
        public string GetIp()
        {
            string host = Dns.GetHostName();
            IPHostEntry ip = Dns.GetHostEntry(host);
            string IP = ip.AddressList[1].ToString();
            return IP;
        }
    }
}