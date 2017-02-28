using MySql.Data.MySqlClient;
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
    class Connection : ConnectionBase
    {

        public TcpClient m_socket;//The socket for the connection
        private DateTime m_lastVerifyTime;
        private Encoding m_encoding;//The encoding used for 
        private int connectionState;//0 - inactive/closed,  1 - Waiting, 2 - Playing/Paired
        private NetworkStream oppositePlayerNetworkStream = null;
        private NetworkStream currentNetworkStream = null;
        private bool isPaired = false;
        private bool exitThread = false;

        private Thread m_thread;

        /// <summary>
        /// Parametarized constructor for Connection class
        /// </summary>
        /// <param name="socket">Socket on which the Client request is accepted</param>
        /// <param name="encoding">The encoding used for the communication</param>
        public Connection(TcpClient socket, Encoding encoding)
        {
            Socket = socket;
            m_encoding = encoding;
            currentNetworkStream = socket.GetStream();
            //processIncoming();
        }

        public void logConnectionData()
        {

        }

        public void processIncoming()
        {

            IPEndPoint endPoint = (IPEndPoint)m_socket.Client.RemoteEndPoint;
            // .. or LocalEndPoint - depending on which end you want to identify

            IPAddress ipAddress = endPoint.Address;

            // get the hostname
            //IPHostEntry hostEntry = Dns.GetHostEntry(ipAddress);
            //string hostName = hostEntry.HostName;

            // get the port
            int port = endPoint.Port;
            //Console.WriteLine(string.Format("Thread ID {0}"),);
            Console.WriteLine(string.Format("Ip Address of connected client is {0}", ipAddress));
            Console.WriteLine(string.Format("port number of connected client is {0}", port));

            while (!isPaired) ;

            //if (oppositePlayerNetworkStream==null)
            //{
            //    return;
            //}

            if (exitThread)
            {
                CallbackThread.Abort();
            }

            string dataReceived = null;
            while (true)
            {
                dataReceived = null;
                byte[] buffer = new byte[m_socket.ReceiveBufferSize];
                try
                {
                    int bytesRead = currentNetworkStream.Read(buffer, 0, m_socket.ReceiveBufferSize);
                    dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    
                    //Log the data to Database
                    //Console.WriteLine(string.Format("received: {0}", dataReceived));
                    //currentNetworkStream.Write(buffer, 0, bytesRead);
                    if (string.Compare(dataReceived, "exit") == 0)
                        break;
                }
                catch (ObjectDisposedException)
                {

                }
                catch (System.IO.IOException ex)
                {
                    Console.WriteLine("");
                    break;
                }
            }



            /*
            if(!authenticateClient())
            {
                //close socket do anything here
            }*/
        }



        static void dataLogQuery(string player1, string player2,string date, string game, double duration, string events)
        {

            
            // Console.WriteLine("Password encypted "+ hash);


            using (MySqlConnection conn = new MySqlConnection())
            {
                // try block
                try
                {

                    conn.ConnectionString = "server=mysql.cs.mtsu.edu;database=ss2ac;uid=ss2ac;pwd=summers1707";
                    conn.Open();
                    // Create the command
                    MySqlCommand comm = conn.CreateCommand();
                    comm.CommandText = "INSERT INTO Session_Log(Player1, Player2, Date, Game, Duration, Events) VALUES(@player1,@player2,@date,@game,@duration,@events)";
                    comm.Parameters.AddWithValue("@player1", player1);
                    comm.Parameters.AddWithValue("@player2", player2);
                    comm.Parameters.AddWithValue("@date", date);
                    comm.Parameters.AddWithValue("@game", game);
                    comm.Parameters.AddWithValue("@duration", duration);
                    comm.Parameters.AddWithValue("@events", events);

                    comm.ExecuteNonQuery();
                   // Console.WriteLine("Player Added ");

                }
                // catch block
                catch (MySqlException er)
                {

                    Console.WriteLine("There was an error reported by SQL Server, " + er.Message);
                }
            }

        }


        /// <summary>
        /// Method to validate client password and client exists
        /// </summary>
        /// <returns></returns>
        public bool authenticateClient()
        {
            if (!verifyConnected())
                return false;

            bool loggedInLoop = true;
            string dataFromClient = null;

            byte[] bytesFrom = new byte[10025];

            lock (m_socket)
            {
                Console.WriteLine("Before validate Credentials....");
                while (loggedInLoop)
                {
                    //NetworkStream logIndata = m_socket.GetStream();
                    int count = 0;

                    try
                    {
                        count = currentNetworkStream.Read(bytesFrom, 0, m_socket.Available); // what if data available is greater than the bytesFrom size of 10025
                        dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom, 0, count);
                    }
                    catch (System.IO.IOException)
                    {
                        //occurs when there's an error- Other end of the connection closes the connection
                        //TODO: Clean up of the connection from connection list and thread.. socket?

                        return false;// this would take care of clean up in the processincoming ??
                    }
                    catch (ObjectDisposedException)
                    {
                        //occurs when stream is closed
                        m_socket.Close();

                        return false;// this would take care of clean up in the processincoming ??
                    }

                    if (count > 0)
                    {
                        /*  Return codes to validate on client side
                           #0 - Means failure due to non existence of the Client Id in db
                           #1 - Means Successfully authenticated
                           #2 - Means failure due to password mismatch

                           Create enum data types for retrieving the message corresponding to the return code on client side.

                        */

                        /*Client sends clientId and password(delimited by ) and waits for reply from server
                        TODO:
                            first see if client id exists in the db, if not send 0. on client side display message corresponding to the return code

                            server pulls the password from database and validates.

                            Validation includes mdh5/decrypt and validate password if authenticated send 1 to client else send 2 to client and 
                            close socket and abort thread(need to abort? or the mechanism of looping through connections and validating connection is open will take care of thread? );
                        */


                    }

                    currentNetworkStream.Close();

                }
            }

            return true;
        }

        public bool verifyConnected()
        {
            //note: `Available` is checked before because it's faster,
            //`Available` is also checked after to prevent a race condition.
            bool connected = m_socket.Client.Available != 0 ||
                !m_socket.Client.Poll(1, SelectMode.SelectRead) ||
                m_socket.Client.Available != 0;
            m_lastVerifyTime = DateTime.UtcNow;
            return connected;
        }

        public TcpClient Socket
        {
            get
            {
                return m_socket;
            }
            set
            {
                m_socket = value;
            }
        }

        public DateTime LastVerifyTime
        {
            get
            {
                return m_lastVerifyTime;
            }
        }

        public Encoding Encoding
        {
            get
            {
                return m_encoding;
            }
            set
            {
                m_encoding = value;
            }
        }

        public int ConnectionState
        {
            get
            {
                return connectionState;
            }

            set
            {
                connectionState = value;
            }
        }

        public NetworkStream OppositePlayerNetworkStream
        {
            get
            {
                return oppositePlayerNetworkStream;
            }

            set
            {
                isPaired = true;
                oppositePlayerNetworkStream = value;
            }
        }

        public bool IsPaired
        {
            get
            {
                return isPaired;
            }

            set
            {
                isPaired = value;
            }
        }

        public bool ExitThread
        {
            get
            {
                return exitThread;
            }

            set
            {
                exitThread = value;
            }
        }
        public NetworkStream CurrentNetworkStream
        {
            get
            {
                return currentNetworkStream;
            }

            set
            {
                currentNetworkStream = value;
            }
        }


        public Thread CallbackThread
        {
            get
            {
                return m_thread;
            }
            set
            {
                //    if (!canStartNewThread())
                //    {
                //        throw new Exception("Cannot override TcpServerConnection Callback Thread. The old thread is still running.");
                //    }
                m_thread = value;
            }
        }


        //private bool canStartNewThread()
        //{
        //    if (m_thread == null)
        //    {
        //        return true;
        //    }
        //    return (m_thread.ThreadState & (ThreadState.Aborted | ThreadState.Stopped)) != 0 &&
        //           (m_thread.ThreadState & ThreadState.Unstarted) == 0;
        //}
    }
}
