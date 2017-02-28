using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    class Program
    {

        static void Main(string[] args)
        {
            int portNumber=-1;
            bool isValidPort = false;
            GameServer gameServer = new GameServer();

            while(!isValidPort)
            {
                Console.WriteLine("Enter the port number to listen for clients:");
                
                //Read port number and validate it being a integer
                if (int.TryParse(Console.ReadLine(), out portNumber))
                {
                    if (portNumber > 1024)
                    {
                        gameServer.Port = portNumber;
                        isValidPort = true;
                    }
                    else
                        Console.WriteLine("Enter a port number greater than 1024");
                }
                else
                {
                    Console.WriteLine("Enter a valid port number");
                }
                
            }

            Console.Clear();
            //Opening the server for listening

            gameServer.Open();

            return;// Remove ? after working on changes. Viswanath

            //MATT: Remove the lines above and try to do anything if you want to do any changes in your code
            // I am trying to build a structure way of doing what you are trying to do.
            
            Console.WriteLine("Server Is Starting");
            Thread handleClientsThread = new Thread(new ThreadStart(handleClientsThreadMethod));
            //handleClientsThread.IsBackground = true;
            handleClientsThread.Start();
        }

        static void handleClientsThreadMethod()
        {
            Dictionary<string, TcpClient> clientlist = new Dictionary<string, TcpClient>();
            TcpListener serverSocket = new TcpListener(System.Net.IPAddress.Any, 8881);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            //Console.WriteLine("Server Running On port {0}", 8881 );
            serverSocket.Start();
            handleClient myclients = new handleClient(clientSocket);

            counter = 0;
            while (true)
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();

                //Alert client has connected
                Console.Write("\n" + "Client No:" + Convert.ToString(counter) + " " 
                    + clientSocket.Client.RemoteEndPoint.ToString() + " connected!\n>> ");

                //Add Client to list of clients
                myclients.AddClient(clientSocket);
                //Show the Amount of connected users
                Console.WriteLine("Number of Connections: " + myclients.NumberOfConnections());

                //Send Client a list of all connected clients
                myclients.SendConnectedClientList(clientSocket);


                //Connect Clients
                if(myclients.NumberOfConnections() > 1)
                    myclients.PairClients(clientSocket, myclients.GetClient());


                //Erases list of all disconnected clients
                myclients.CleanConnection();
            }

        }
    }

    [Serializable()]
    public class handleClient
    {
        //IDictionary<string, TcpClient> clientTable = new Dictionary<string, TcpClient>();
        List<TcpClient> mylist = new List<TcpClient>();
        TcpClient thisclient;
        public handleClient(TcpClient currclient)
        {
            thisclient = currclient;
        }

        /// <summary>
        /// Add a connected client to the list
        /// </summary>
        /// <param name="thisclient"></param>
        public void AddClient(TcpClient thisclient)
        {
            mylist.Add(thisclient);
        }

        /// <summary>
        /// Removes A specific client from the connected clients list
        /// </summary>
        /// <param name="thisclient"></param>
        public void RemoveClient(TcpClient thisclient)
        {
            mylist.Remove(thisclient);
            thisclient.Client.Close();
        }

        /// <summary>
        /// Prints the list of clients
        /// </summary>
        public void PrintClients()
        {
            foreach (var kvp in mylist)
            {
                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                Console.WriteLine("Connection: {0}", kvp);
            }
        }

        /// <summary>
        /// Send Newly connected socket a list of already connected sockets
        /// </summary>
        /// <param name="clientSocket"></param>
        public void SendConnectedClientList(TcpClient clientSocket)
        {
            string singleclient = "There are no other users at this time \n";
            NetworkStream nwStream = clientSocket.GetStream();
            byte[] buffer = new byte[clientSocket.ReceiveBufferSize];

            if (mylist.Count >= 2)
            {
                foreach (var item in mylist)
                {
                    if (clientSocket.Client.RemoteEndPoint.ToString() != item.Client.RemoteEndPoint.ToString())
                    {
                        if (item.Client.Connected)
                        {
                            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(item.Client.RemoteEndPoint.ToString() + " \n");
                            nwStream.Write(bytesToSend, 0, item.Client.RemoteEndPoint.ToString().Length + 2);
                        }

                    }
                }
                

            }
            else
            {
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(singleclient);
                nwStream.Write(bytesToSend, 0, singleclient.Length);
            }

            //clientSocket.Client.Close();
        }

        /// <summary>
        /// Removes all Sockets from list that are no longer connected
        /// </summary>
        public void CleanConnection()
        {
            List<TcpClient> templist = new List<TcpClient>();
            foreach (var item in mylist)
            {
                if (!item.Connected)
                {
                    templist.Add(item);
                }
            }
            foreach (var item in templist)
            {
                Console.WriteLine(item.Client.ToString() + " Closed\n");
                mylist.Remove(item);
            }
        }

        /// <summary>
        /// Returns the number of connected clients
        /// </summary>
        /// <returns></returns>
        public int NumberOfConnections()
        {
            return mylist.Count();
        }


        /// <summary>
        /// Connects two clients together
        /// </summary>
        /// TODO: IMPLEMENT A WAY FOR CLIENTS TO CONNECT
        public void PairClients(TcpClient client1, TcpClient client2)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a random client from list
        /// </summary>
        /// <returns></returns>
        public TcpClient GetClient()
        {
            foreach(var item in mylist)
            {
                if(item != thisclient)
                {
                    return item;
                }
            }

            throw new Exception("ERROR! No two clients can be paired.");
        }

        //internal void startClient(TcpClient clientSocket, string v, ref object clientTable)
        //{

        //}
    }
}
