using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerInterfaces
{
    interface Connection
    {
        /// <summary>
        /// Method to return the port number of the master player(will consider the a lists of players
        /// </summary>
        /// <returns></returns>
        bool connected();

        /// <summary>
        /// Method to check if the connection is still alive(listenning for data log?)
        /// </summary>
        /// <returns></returns>
        bool verifyConnected();

        /// <summary>
        /// Method to listen for data from master player
        /// </summary>
        void DatalogListener();

        /// <summary>
        /// method to close the connection manually
        /// </summary>        
        void forceDisconnect();

        /// <summary>
        /// Method to send the IPAddress to the clients that are being paired
        /// </summary>
        void ExchangeIPAndSocketClients();
    }
}
