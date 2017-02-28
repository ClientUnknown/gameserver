using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    interface ConnectionBase
    {
        bool authenticateClient();
        bool verifyConnected();
        void logConnectionData();
        void processIncoming();
    }
}
