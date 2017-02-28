using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace Client1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Authenticating client
            int retCode = callToLogin();
            int serverPortNumber = 0;

            //Login validation for client
            if (retCode == 1)
            {

                string textToSend = DateTime.Now.ToString();
                IPAddress serverIpAddress;

                Socket Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //Read the ip address of the server to connect
                Console.WriteLine("Enter the IP address of server or press enter to use localhost\n");
                IPAddress.TryParse(Console.ReadLine(), out serverIpAddress);

                //Read the port number of the server to connect
                Console.WriteLine("Enter the port number of server:");
                int.TryParse(Console.ReadLine(), out serverPortNumber);

                //Establishing connection to server
                Console.WriteLine("Establishing Connection to {0}:{1}", serverIpAddress, serverPortNumber);

                //TRY Connecting To Server
                try
                {
                    if (serverIpAddress == null)
                    {
                        //connect to server
                        Server.Connect(Dns.GetHostAddresses("localhost"), serverPortNumber);
                        if (!Server.Connected)
                            return;
                    }
                    else
                    {
                        Server.Connect(serverIpAddress, serverPortNumber);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not connect to server, please make sure the server is running!\n");
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("Connection established to server..");
                Stream stream = new NetworkStream(Server);
                ServerCommunicate serverchat = new ServerCommunicate(Server);

                //Try Connect to Client
                var serverMSG = serverchat.ServerRecv();
                
                //receive message from server to decide on other player. and also if being a client or host in the pairing process
                while (serverMSG != "Host" && serverMSG != "Client")
                {
                    Console.WriteLine(serverMSG);
                    serverMSG = serverchat.ServerRecv();
                }

                if (serverMSG == "Host")
                {
                    Console.WriteLine("You are being connected to a player");
                    ClientCommunicate clientchat = new ClientCommunicate();

                    //Wait to listen for the request from opposite playes
                    clientchat.Listen();
                }
                else if (serverMSG == "Client")
                {
                    Console.WriteLine("You are the host");
                    
                    //receive the other player IP and port
                    serverMSG = serverchat.ServerRecv();
                    
                    //start communicating with other player
                    ClientCommunicate clientchat = new ClientCommunicate(serverMSG, 8882);
                    clientchat.ServerObject = serverchat;
                    
                    //send message to server on pairing confirmation
                    serverchat.ServerSend("Game started between: Players");
                    
                    //connect to opposite player(host which is listening)
                    clientchat.connect();
                    Console.WriteLine("You are connected to a player");

                }

            }
        }

        static int authetication(string userid, string password)
        {
            using (MySqlConnection conn = new MySqlConnection())
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(password);
                data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
                String hashpwd = System.Text.Encoding.ASCII.GetString(data);

                try
                {
                  
                    conn.ConnectionString = "server=mysql.cs.mtsu.edu;database=ss2ac;uid=ss2ac;pwd=summers1707";
                    conn.Open();
                  
                    MySqlCommand command = new MySqlCommand("select * from Player_Info where Player_Id like @username and Password = @password; ", conn);
                    command.Parameters.AddWithValue("@username", userid);
                    command.Parameters.AddWithValue("@password", hashpwd);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (userid == "admin")
                            {
                                runQuery();
                                return 0;
                            }
                            else
                            {
                                Console.WriteLine("Login Succesful! ");
                                return 1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Incorrect Username or Password ! ");
                            Console.WriteLine("Login Unsuccesful! ");
                            Console.ReadKey();
                            return 0;
                        }
                    }



                    // try block


                }
                // catch block
                catch (MySqlException er)
                {
                    Console.WriteLine("There was an error reported by SQL Server, " + er.Message);
                }

            }
            return 0;

        }

        static int callToLogin()
        {

            Console.Write("Please enter username and password to Login \n");
            Console.WriteLine("User_Name :");
            string username = Console.ReadLine();
            Console.WriteLine("Password:");
            //string password = Console.ReadLine();
            string password = null;
            while (true)
            {
                var key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    break;
                password += key.KeyChar;
            }
            Console.WriteLine();
            username = username.Trim();
            password = password.Trim();

            int k = authetication(username, password);
            return k;

        }
        static void printQuery()
        {
            Console.WriteLine("logs: To print all logs ");
            Console.WriteLine("logs-MM/dd/yyyy: To print log for a particular date");
            Console.WriteLine("Users-Connected: To print list of users who are connected to server");
            Console.WriteLine("Player: To print all the players list");
            Console.WriteLine("Add-Player: To add new players");
            Console.WriteLine("Remove-Player: To remove an existing player");


        }


        static void runQuery()
        {
            Console.Write("Hello Admin \n");
            Console.Write("Use 'query' to know your queries \n");

            string signal = "";      // initialize to neutral
            while (signal != "Exit")      // X indicates stop
            {

                signal = Console.ReadLine();
                signal = signal.Trim();


                if (string.Equals(signal, "query", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("Your custom queries are as given\n");
                    printQuery();
                }



                else if (string.Equals(signal, "logs", StringComparison.CurrentCultureIgnoreCase))
                {
                    logsQuery();
                }

                else if (signal.IndexOf("logs-", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    logsDateQuery(signal);
                }


                else if (string.Equals(signal, "Users-Connected", StringComparison.CurrentCultureIgnoreCase))
                {

                    usersConnectedQuery();
                }


                else if (string.Equals(signal, "Player", StringComparison.CurrentCultureIgnoreCase))

                {
                    playersQuery();


                }


                else if (string.Equals(signal, "Add-Player", StringComparison.CurrentCultureIgnoreCase))
                {

                    addPlayerQuery();
                }

                else if (string.Equals(signal, "Remove-Player", StringComparison.CurrentCultureIgnoreCase))
                {

                    removePlayer();
                }

                else if (string.Equals(signal, "Exit", StringComparison.CurrentCultureIgnoreCase))
                {

                    Console.WriteLine("Disconnecting...");
                    return;
                }
                else
                {

                    Console.WriteLine("Incorrect Query");

                }

            }

        }


        static void logsQuery()
        {
            using (MySqlConnection conn = new MySqlConnection())
            {
                try
                {
                    conn.ConnectionString = "server=mysql.cs.mtsu.edu;database=ss2ac;uid=ss2ac;pwd=summers1707";
                    conn.Open();

                    MySqlCommand command = new MySqlCommand("SELECT * FROM Session_Log", conn);


                    // try block

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        Console.WriteLine("Player1\t  Player2\t  Game\t  Duartion\t  Events");
                        while (reader.Read())
                        {
                            Console.WriteLine(String.Format("{0} \t  | {1} \t  | {2} \t  | {3} \t  | {4} \t ",
                                reader[0], reader[1], reader[2], reader[3], reader[4]));
                        }
                    }

                }
                // catch block
                catch (MySqlException er)
                {
                    Console.WriteLine("There was an error reported by SQL Server, " + er.Message);
                }
            }

        }

        static void logsDateQuery(string signal)
        {
            string givenDate = signal.Substring(5);

            givenDate.Trim();
            string currentDATE = givenDate;
            DateTime dt;

            bool isValid = DateTime.TryParseExact(
                givenDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dt);

            //Console.WriteLine(givenDate);

            if (isValid)
            {
                // try block
                try
                {
                    using (MySqlConnection conn = new MySqlConnection())
                    {

                        conn.ConnectionString = "server=mysql.cs.mtsu.edu;database=ss2ac;uid=ss2ac;pwd=summers1707";
                        conn.Open();

                        MySqlCommand command = new MySqlCommand("SELECT * FROM Session_Log WHERE Date ='@param1'", conn);
                        command.Parameters.AddWithValue("@param1", givenDate);

                        //MySqlCommand command = new MySqlCommand("SELECT * FROM Session_Log WHERE Player1='@param1'", conn);
                        //command.Parameters.AddWithValue("@param1", "vw2pd");


                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            Console.WriteLine("Player1\t  Player2\t Date\t  Game\t  Duartion\t  Events\t");
                            while (reader.Read())
                            {
                                Console.WriteLine(String.Format("{0} \t  | {1} \t  | {2} \t  | {3} \t  | {4} \t  | {5} \t ",
                                    reader[0], reader[1], reader[2], reader[3], reader[4]));
                            }
                        }





                    }
                }
                // catch block
                catch (MySqlException er)
                {
                    Console.WriteLine("There was an error reported by SQL Server, " + er.Message);
                }

            }


            else
            {
                Console.WriteLine("Incorrect Date format.Use yyyy-MM-dd");
            }

        }

        static void usersConnectedQuery()
        {
            using (MySqlConnection conn = new MySqlConnection())
            {
                // try block
                try
                {
                    conn.ConnectionString = "server=mysql.cs.mtsu.edu;database=ss2ac;uid=ss2ac;pwd=summers1707";
                    conn.Open();

                    MySqlCommand command = new MySqlCommand("SELECT * FROM SysSpec", conn);


                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        Console.WriteLine("UserConnected\t  IP_Address\t  ");
                        while (reader.Read())
                        {
                            Console.WriteLine(String.Format("{0} \t  | {1} \t ",
                                reader[0], reader[1]));
                        }
                    }





                }
                // catch block
                catch (MySqlException er)
                {
                    Console.WriteLine("There was an error reported by SQL Server, " + er.Message);
                }
            }
        }


        static void playersQuery()
        {

            using (MySqlConnection conn = new MySqlConnection())
            {
                // try block
                try
                {
                    // Create the connectionString
                    // Trusted_Connection is used to denote the connection uses Windows Authentication
                    conn.ConnectionString = "server=mysql.cs.mtsu.edu;database=ss2ac;uid=ss2ac;pwd=summers1707";
                    conn.Open();
                    // Create the command
                    MySqlCommand command = new MySqlCommand("SELECT * FROM Player_Info", conn);





                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        Console.WriteLine("Player_Id\t  Name\t  Password(Encrypted)\t");
                        while (reader.Read())
                        {
                            Console.WriteLine(String.Format("{0} \t  | {1} \t  | {2} \t ",
                                reader[0], reader[1], reader[2]));
                        }
                    }

                }
                // catch block
                catch (MySqlException er)
                {

                    Console.WriteLine("There was an error reported by SQL Server, " + er.Message);
                }
            }

        }


        static void addPlayerQuery()
        {
            Console.WriteLine("Adding Player ");
            Console.WriteLine("Player Name :");
            string name = Console.ReadLine();
            Console.WriteLine("Player_Id:");
            string playerid = Console.ReadLine();
            Console.WriteLine("Password:");
            string password = Console.ReadLine();
            name = name.Trim();
            playerid = playerid.Trim();
            password = password.Trim();

            //Console.WriteLine("Password " + password);

            //Encrypting password
            byte[] data = System.Text.Encoding.ASCII.GetBytes(password);
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            String hash = System.Text.Encoding.ASCII.GetString(data);


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
                    comm.CommandText = "INSERT INTO Player_Info(Player_Id,Name,Password) VALUES(@playerid, @name,@password)";
                    comm.Parameters.AddWithValue("@playerid", playerid);
                    comm.Parameters.AddWithValue("@name", name);
                    comm.Parameters.AddWithValue("@password", hash);

                    comm.ExecuteNonQuery();
                    Console.WriteLine("Player Added ");

                }
                // catch block
                catch (MySqlException er)
                {

                    Console.WriteLine("There was an error reported by SQL Server, " + er.Message);
                }
            }

        }

        static void removePlayer()
        {
            Console.WriteLine("Removing Player ");
            Console.WriteLine("Player_Id:");
            string playerid = Console.ReadLine();

            playerid = playerid.Trim();




            using (MySqlConnection conn = new MySqlConnection())
            {
                // try block
                try
                {
                    conn.ConnectionString = "server=mysql.cs.mtsu.edu;database=ss2ac;uid=ss2ac;pwd=summers1707";
                    conn.Open();
                    // Create the command
                    MySqlCommand comm = conn.CreateCommand();
                    comm.CommandText = "Delete From Player_Info Where Player_Id='" + playerid + "';";


                    comm.ExecuteNonQuery();
                    Console.WriteLine("Player Removed ");
                }
                // catch block
                catch (MySqlException er)
                {

                    Console.WriteLine("There was an error reported by SQL Server, " + er.Message);
                }
            }


        }

        

        private static void EndReading(IAsyncResult ar)
        {

            throw new NotImplementedException();
        }
    }
}
