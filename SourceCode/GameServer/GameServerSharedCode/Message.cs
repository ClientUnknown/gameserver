using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerSharedCode
{
    /**
* This class defines a Message object used by nodes in the star network.
* Messages from one node to another pass through the central server node
* on their way to other nodes in the network.
*/
    [Serializable]
    public class Message
    {
        /* Member Data */
        public string m_senderID; //identifier for node sending the message        
        public Type m_messageType; //identifier for message object (e.g., GazePosition, EEGAcknowledgement, etc.)

        //public string m_jsonMessage; //serialized message object, type indicated by m_messageType
        object m_messageData;//To store the message actual required ??

        /**
         * Default constructor.
         */
        public Message()
        {
            m_senderID = "NULL";
            
            //m_messageType = "NULL";
            //m_jsonMessage = "NULL";
        }

        /**
         * Instance constructor:
         *  Create an instance of the object by supplying all of the data members in the constructor.
         */
        public Message(string _senderID_, Type _messageType_, object _jsonMessage_)
        {
            m_senderID = _senderID_;            
            m_messageType = _messageType_;
            //m_jsonMessage = _jsonMessage_;
        }
        
        /**
         * Instance constructor:
         *  Create an instance of the object by deserializing a provided JSON string.
         */
        public Message(string jsonString)
        {
            
            //Message m = JsonConvert.DeserializeObject<Message>(jsonString);
            //this.m_senderID = m.m_senderID;
            
            
            //this.m_messageType = m.m_messageType;
            //this.m_jsonMessage = m.m_jsonMessage;
        }

 

   
        /**
         * This function returns the JSON encoding of this object.
         */
        public string ToJsonString()
        {
            //return JsonUtilities.SerializeObjectToJSON(this);
            return JsonConvert.SerializeObject(this);
        }
        
        /**
         * Accessors.
         */
        public string senderID
        {
            get { return m_senderID; }
        }
       
        public Type messageType
        {
            get { return m_messageType; }
        }
        
    }

}
