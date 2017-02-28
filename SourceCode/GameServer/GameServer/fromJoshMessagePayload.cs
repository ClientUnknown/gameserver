/**
 * File: PayloadLib.cs
 * Author: Joshua Wade
 * Date: 7-16-2014

 ******************Extra Comments from Josh on Slack :**********************************
 This file shows an example of using the Newtonsoft JSON library to serialize and deserialize objects in C#.

[9:38]  
In this implementation, a "Payload" object is sent between the server and client. A Payload contains a serialized list of "Message" objects, which can be any type.

[9:38]  
So the basic idea is that Payloads are sent between different endpoints and the contents of the Payload can be literally any kind of object.

[9:39]  

         This function returns the JSON encoding of this object.
        
public string ToJsonString()
{
    return JsonConvert.SerializeObject(this);
}
***************************************************************************************
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace RaslNetworkLibrary
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
        public List<string> m_intendedRecipients; //list of identifiers for intended recipient nodes
        public string m_messageType; //identifier for message object (e.g., GazePosition, EEGAcknowledgement, etc.)
        public string m_jsonMessage; //serialized message object, type indicated by m_messageType

        /**
         * Default constructor.
         */
        public Message()
        {
            m_senderID = "NULL";
            m_intendedRecipients = new List<string>();
            m_messageType = "NULL";
            m_jsonMessage = "NULL";
        }

        /**
         * Instance constructor:
         *  Create an instance of the object by supplying all of the data members in the constructor.
         */
        public Message(string _senderID_, string _intendedRecipients_, string _messageType_, string _jsonMessage_)
        {
            m_senderID = _senderID_;
            m_intendedRecipients = _intendedRecipients_.Split(',').ToList<string>();
            m_messageType = _messageType_;
            m_jsonMessage = _jsonMessage_;
        }

        /**
         * Instance constructor:
         *  Create an instance of the object by deserializing a provided JSON string.
         */
        public Message(string jsonString)
        {
            Message m = JsonConvert.DeserializeObject<Message>(jsonString);
            this.m_senderID = m.m_senderID;
            this.m_intendedRecipients = new List<string>();
            foreach (string recipient in m.m_intendedRecipients)
                this.m_intendedRecipients.Add(recipient);
            this.m_messageType = m.m_messageType;
            this.m_jsonMessage = m.m_jsonMessage;
        }

        /**
         * Copy constructor.
         */
        public Message(Message rhs)
        {
            this.m_senderID = rhs.m_senderID;
            this.m_intendedRecipients = new List<string>();
            foreach (string recipient in rhs.m_intendedRecipients)
                this.m_intendedRecipients.Add(recipient);
            this.m_messageType = rhs.m_messageType;
            this.m_jsonMessage = rhs.m_jsonMessage;
        }

        /**
         * This function returns true if this Message is for the specified recipient.
         */
        public bool HasRecipient(string recipID)
        {
            return m_intendedRecipients.Contains(recipID);
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
         * This function returns a nicely formatted string representation of the object.
         */
        public override string ToString()
        {
            string s = "[senderID=\"" + m_senderID + "\", intendedRecipients=[";
            if (m_intendedRecipients.Count > 0)
            {
                foreach (string recipient in m_intendedRecipients)
                    s += "\"#\",".Replace("#", recipient);
                s = s.Substring(0, s.Length - 1); //remove extraneous comma
            }
            s += "], messageType=\"" + m_messageType + "\", ";
            s += "jsonMessage=\"" + m_jsonMessage + "\"]";
            return s;
        }

        /**
         * Accessors.
         */
        public string senderID
        {
            get { return m_senderID; }
        }
        public string GetRecipientAtIndex(int index)
        {
            if (index < 0 || index > (m_intendedRecipients.Count - 1))
                return "NULL";
            else
                return m_intendedRecipients[index];
        }
        public string messageType
        {
            get { return m_messageType; }
        }
        public string jsonMessage
        {
            get { return m_jsonMessage; }
        }
    }

    /**
     * This class defines a Payload object. A Payload contains a serialized list of
     * Message objects, allowing nodes in the network to receive multiple Messages
     * in a single receive statement.
     */
    [Serializable]
    public class Payload
    {
        /* Member Data */
        public List<Message> m_messages; //list of Message objects--can be empty

        /**
         * Default constructor.
         */
        public Payload()
        {
            m_messages = new List<Message>();
        }

        /**
         * Instance constructor:
         *  Create an instance of the object by deserializing a provided JSON string.
         */
        public Payload(string jsonString)
        {
            Payload p = JsonConvert.DeserializeObject<Payload>(jsonString);
            this.m_messages = new List<Message>();
            foreach (Message m in p.m_messages)
                this.m_messages.Add(new Message(m));
        }

        /**
         * Copy constructor.
         */
        public Payload(Payload rhs)
        {
            this.m_messages = new List<Message>();
            foreach (Message m in rhs.m_messages)
                this.m_messages.Add(new Message(m));
        }

        /**
         * This function adds a Message to this Payload.
         */
        public void AddMessage(Message m)
        {
            m_messages.Add(m);
        }

        /**
         * This function removes a Message from this Payload by its Sender ID.
         */
        public void RemoveMessageFromSender(string senderIdToRemove)
        {
            //locate the index of the message that should be removed
            int indexToRemove = -1;
            for (int i = 0; i < m_messages.Count; i++)
                if (m_messages[i].senderID == senderIdToRemove)
                    indexToRemove = i;

            //remove if found
            if (indexToRemove > -1)
                m_messages.RemoveAt(indexToRemove);
        }

        /**
         * This function clears the Payload of all Messages
         */
        public void Empty()
        {
            m_messages.Clear();
        }

        /**
         * This function returns the number of Message objects contained in this Payload.
         */
        public int MessageCount()
        {
            return m_messages.Count;
        }

        /**
         * This function allows the user to determine if this Payload contains a message from a
         * specified node given a node identifier.
         */
        public bool ContainsMessageFromSender(string senderID)
        {
            //iterate over each Message to determine if the Payload contains senderID
            foreach (Message m in m_messages)
                if (m.senderID == senderID)
                    return true;
            //otherwise
            return false;
        }

        /**
         * This function returns the Message contained at the specified index if it exists.
         */
        public Message GetByIndex(int index)
        {
            //if index is out of range, return null
            if (index < 0 || index > (MessageCount() - 1))
                return null;
            else
                return m_messages[index];
        }

        /**
         * This function returns the Message from the sender with the specified id if it exists.
         */
        public Message GetBySenderID(string senderID)
        {
            foreach (Message m in m_messages)
                if (m.senderID == senderID)
                    return m;
            return null;
        }

        /**
         * This function returns the JSON encoding of this object.
         */
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /**
         * This function returns a nicely formatted string representation of the object.
         */
        public override string ToString()
        {
            string s = "Payload=[";
            if (m_messages.Count > 0)
            {
                foreach (Message m in m_messages)
                    s += m.ToString() + ",";
                s = s.Substring(0, s.Length - 1); //remove extraneous comma
            }
            s += "]";
            return s;
        }
    }

}