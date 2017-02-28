using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client1
{
    class ServerCommunicate
    {
        private Socket serverSocket;
        public ServerCommunicate(Socket ServerSocket)
        {
            serverSocket = ServerSocket;
        }

        public string ServerRecv()
        {
            while (true)
            {
                byte[] buffer = new byte[serverSocket.ReceiveBufferSize];
                Stream stream = new NetworkStream(serverSocket);
                var asyncReader = stream.BeginRead(buffer, 0, buffer.Length, null, null);
                int bytesRead = stream.EndRead(asyncReader);
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                return dataReceived;
            }
        }
        //
        public void ServerSend(string Message)
        {
            byte[] dataToSave = Encoding.ASCII.GetBytes(Message);
            int bytesToBeSent = dataToSave.Length;
            serverSocket.Send(dataToSave, bytesToBeSent, SocketFlags.None);
        }


    }
}
