using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client1
{

    class ClientCommunicate
    {
        private string serverIp = null;
        private int serverPortNumber = 0;
        Socket clientSocket;
        private ServerCommunicate serverObject = null;//To handle comunication to server on status by host
        private IPAddress oppositeClient = null;
        private int oppositeClientPort = 0;


        public ClientCommunicate() { }
        public ClientCommunicate(string ipAddress, int portNumber)
        {
            if (string.IsNullOrEmpty(ipAddress) && portNumber < 1024)
            {
                return;
            }

            serverIp = ipAddress.Substring(0, ipAddress.IndexOf(":"));//Dns.GetHostAddresses("localhost")[0];
            serverPortNumber = portNumber;
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public int connect()
        {
            Console.WriteLine("Establishing Connection to {0} : {1}", serverIp, serverPortNumber);

            try
            {
                TcpClient mysocket = new TcpClient();
                mysocket.Connect(serverIp, serverPortNumber);
                HandleCommunication(mysocket);
            }
            catch (Exception ex)
            {
                Console.Write("Could not connect to server, Please make sure server is running!\n");
                return 0;
            }


            Console.WriteLine("Connection established");
            return 1;
        }

        /// <summary>
        /// Handles information sent from Host Client to Client
        /// </summary>
        /// <param name="clientSocket"></param>
        private void HandleCommunication(TcpClient clientSocket)
        {

            string message = "";
            Console.WriteLine("Type the message for the player:");
            NetworkStream nwStream = clientSocket.GetStream();
            while (true)
            {
                //Console.WriteLine("sending:");
                message = Console.ReadLine();

                byte[] buffer = new byte[clientSocket.ReceiveBufferSize];
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(message);
                nwStream.Write(bytesToSend, 0, message.Length);

                message = "";
                buffer = null;

                buffer = new byte[clientSocket.ReceiveBufferSize];
                var mess = nwStream.Read(buffer, 0, clientSocket.ReceiveBufferSize);
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, mess);
                Console.WriteLine("Recieved:" + dataReceived + '\n');

                dataReceived = "";
            }
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Listen for Connection of Host Client
        /// </summary>
        internal void Listen()
        {
            TcpListener serverSocket = new TcpListener(System.Net.IPAddress.Any, 8882);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;
            bool temp = true;

            //serverSocket.Server.Bind(new IPEndPoint(IPAddress.Any, 8882));

            serverSocket.Start(5);
            while (temp)
            {
                if (serverSocket.Pending())
                {
                    //counter += 1;
                    clientSocket = serverSocket.AcceptTcpClient();

                    //Alert client has connected
                    Console.Write("\n" + "Client No:" + Convert.ToString(counter) + " "
                        + clientSocket.Client.RemoteEndPoint.ToString() + " connected!\n>> ");

                    CommunicationtoHost(clientSocket);
                    temp = false;
                }
            }
        }


        /// <summary>
        /// Handle Communication from Host client to Client
        /// </summary>
        /// <param name="clientSocket"></param>
        private void CommunicationtoHost(TcpClient clientSocket)
        {
            NetworkStream nwStream = clientSocket.GetStream();
            string message = "";
            while (true)
            {
                byte[] buffer = new byte[clientSocket.ReceiveBufferSize];

                buffer = new byte[clientSocket.ReceiveBufferSize];
                var mess = nwStream.Read(buffer, 0, clientSocket.ReceiveBufferSize);
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, mess);
                Console.WriteLine("Recieved:" + dataReceived + '\n');

                message = Console.ReadLine();
                nwStream = clientSocket.GetStream();

                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(message);//)item.Client.RemoteEndPoint.ToString() + " \n");
                nwStream.Write(bytesToSend, 0, message.Length);//item.Client.RemoteEndPoint.ToString().Length + 2);}
                message = "";



            }
        }


        public ServerCommunicate ServerObject
        {
            get
            {
                return serverObject;
            }

            set
            {
                serverObject = value;
            }
        }
    }
}
