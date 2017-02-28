using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    class GameServer
    {
        private List<Connection> connections;
        private Dictionary<Connection, Connection> pairedConnections = new Dictionary<Connection, Connection>();

        private TcpListener listener;//for listening for requests from the client/game
        private Thread listenThread;//Thread for running listener for the client/game
        private int m_port;//port on which Server listens for requests from client
        private bool m_isOpen;//to check if server is open
        private int activeThreads;//Active threads count
        private Encoding m_encoding;//The encoding used for communication over tcp connection
        private int m_idleTime;//Time config to make server sleep

        public GameServer()
        {
            Initialise();
        }

        /// <summary>
        /// Method to initialize the server configuration information
        /// </summary>
        private void Initialise()
        {
            activeThreads = 0;
            m_idleTime = 50;
            m_port = -1;
            m_isOpen = false;
            m_encoding = Encoding.ASCII;

            connections = new List<Connection>();
            listener = null;
            listenThread = null;
        }

        /// <summary>
        /// Method to open/start the server
        /// </summary>
        public void Open()
        {
            lock (this)
            {
                if (m_isOpen)
                {
                    //already open, no work to do
                    return;
                }
                if (m_port < 0)
                {
                    throw new Exception("Invalid port");
                }

                try
                {
                    listener.Start(5);
                    //syncListener.Start(5);
                }
                catch (Exception)
                {
                    listener.Stop();
                    listener = new TcpListener(IPAddress.Any, m_port);
                    listener.Start(5);//5 is the limit for queue to hold the number of pending connections while listening

                }

                m_isOpen = true;

                listenThread = new Thread(new ThreadStart(runListener));
                listenThread.Start();

                /*connCheck = new Thread(new ThreadStart(checkConnOpen));
                connCheck.Start();

                syncThread = new Thread(new ThreadStart(syncListner));
                syncThread.Start();*/

                //sendThread = new Thread(new ThreadStart(runSender));
                //sendThread.Start();
            }
        }


        public int Port
        {
            get
            {
                return m_port;
            }
            set
            {
                if (value < 0)
                {
                    return;
                }

                if (m_port == value)
                {
                    return;
                }

                if (m_isOpen)
                {
                    throw new Exception("Invalid attempt to change port while still open.\nPlease close port before changing.");
                }

                m_port = value;
                if (listener == null)
                {
                    //this should only be called the first time.
                    listener = new TcpListener(IPAddress.Any, m_port);
                }
                else
                {
                    listener.Server.Bind(new IPEndPoint(IPAddress.Any, m_port));
                }
                //if (syncListener == null)
                //{
                //    syncListener = new TcpListener(IPAddress.Any, m_portSync);
                //}
                //else
                //{
                //    syncListener.Server.Bind(new IPEndPoint(IPAddress.Any, m_portSync));
                //}
            }
        }

        private void runListener()
        {
            Console.WriteLine("Started to listen on the thread ");
            //Console.ReadKey();
            //Thread.CurrentThread.Abort();

            while (m_isOpen)
            {
                try
                {

                    if (listener.Pending())
                    {
                        TcpClient socket = listener.AcceptTcpClient();
                        Connection conn = new Connection(socket, m_encoding);


                        //Thread newConn = new Thread(() => newConnection(socket));


                        conn.CallbackThread = new Thread(new ThreadStart(conn.processIncoming));
                        conn.CallbackThread.Start();
                        conn.CallbackThread.IsBackground = true;
                        lock (connections)
                        {
                            connections.Add(conn);
                        }

                        //newConn.Start();
                        if ((connections.Count % 2 == 0) && connections.Count > 0)
                        {
                            int countConnections = connections.Count;

                            string Message = "Host";
                            byte[] dataToSave = Encoding.ASCII.GetBytes(Message);
                            int bytesToBeSent = dataToSave.Length;
                            connections[countConnections - 2].m_socket.GetStream().Write(dataToSave, 0, bytesToBeSent);

                            Message = "";

                            Message = "Client";
                            dataToSave = Encoding.ASCII.GetBytes(Message);
                            bytesToBeSent = dataToSave.Length;
                            connections[countConnections - 1].m_socket.GetStream().Write(dataToSave, 0, bytesToBeSent);

                            Message = connections[countConnections - 2].m_socket.Client.RemoteEndPoint.ToString();
                            dataToSave = Encoding.ASCII.GetBytes(Message);
                            bytesToBeSent = dataToSave.Length;
                            connections[countConnections - 1].m_socket.GetStream().Write(dataToSave, 0, bytesToBeSent);

                            pairedConnections.Add(connections[countConnections - 2], connections[countConnections - 1]);

                            lock (connections)
                            {
                                connections[countConnections - 2].IsPaired = true;
                                connections[countConnections - 1].IsPaired = true;
                                connections[countConnections - 1].ExitThread = true;

                                connections.Remove(connections[countConnections - 1]);
                                connections.Remove(connections[countConnections - 2]);
                            }

                            //break;
                            //connections[0].OppositePlayerNetworkStream = connections[1].CurrentNetworkStream;
                            //connections[1].OppositePlayerNetworkStream = connections[0].CurrentNetworkStream;
                        }


                    }
                    else
                    {
                        System.Threading.Thread.Sleep(m_idleTime);
                    }
                }
                catch (ThreadInterruptedException) { } //thread is interrupted when we quit
                catch (Exception e)
                {
                    //if (m_isOpen && OnError != null)
                    //{
                    //    OnError(this, e);
                    //}
                }
            }
        }

        private void newConnection(TcpClient socket)
        {
            Connection conn = new Connection(socket, m_encoding);
            lock (connections)
            {
                connections.Add(conn);
            }

            ClientAlert(socket);

        }

        private void ClientAlert(TcpClient socket)
        {
            string singleclient = "There are no other users at this time, Waiting for other users \n";
            string multipleclients = "There are " + (connections.Count - 1).ToString() + " other user(s) connected to the server";
            NetworkStream nwStream = socket.GetStream();
            byte[] posToSend = ASCIIEncoding.ASCII.GetBytes(singleclient);
            byte[] negToSend = ASCIIEncoding.ASCII.GetBytes(multipleclients);

            if (connections.Count <= 1)
                nwStream.Write(posToSend, 0, singleclient.Length);
            else
                nwStream.Write(negToSend, 0, multipleclients.Length);
        }

        public void PairClients()
        {
            //Console.WriteLine("Searching for another user to Pair with!");
        }


    }
}
