//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace GameServerSharedCode
//{
//    /**
//  * This class defines a Payload object. A Payload contains a serialized list of
//  * Message objects, allowing nodes in the network to receive multiple Messages
//  * in a single receive statement.
//  */
//    [Serializable]
//    public class Payload
//    {
//        /* Member Data */
//        public List<Message> m_messages; //list of Message objects--can be empty

//        /**
//         * Default constructor.
//         */
//        public Payload()
//        {
//            m_messages = new List<Message>();
//        }

//        /**
//         * Instance constructor:
//         *  Create an instance of the object by deserializing a provided JSON string.
//         */
//        public Payload(string jsonString)
//        {
//            Payload p = JsonConvert.DeserializeObject<Payload>(jsonString);
//            this.m_messages = new List<Message>();
//            foreach (Message m in p.m_messages)
//                this.m_messages.Add(new Message(m));
//        }

//        /**
//         * Copy constructor.
//         */
//        public Payload(Payload rhs)
//        {
//            this.m_messages = new List<Message>();
//            foreach (Message m in rhs.m_messages)
//                this.m_messages.Add(new Message(m));
//        }

//        /**
//         * This function adds a Message to this Payload.
//         */
//        public void AddMessage(Message m)
//        {
//            m_messages.Add(m);
//        }

//        /**
//         * This function removes a Message from this Payload by its Sender ID.
//         */
//        public void RemoveMessageFromSender(string senderIdToRemove)
//        {
//            //locate the index of the message that should be removed
//            int indexToRemove = -1;
//            for (int i = 0; i < m_messages.Count; i++)
//                if (m_messages[i].senderID == senderIdToRemove)
//                    indexToRemove = i;

//            //remove if found
//            if (indexToRemove > -1)
//                m_messages.RemoveAt(indexToRemove);
//        }

//        /**
//         * This function clears the Payload of all Messages
//         */
//        public void Empty()
//        {
//            m_messages.Clear();
//        }

//        /**
//         * This function returns the number of Message objects contained in this Payload.
//         */
//        public int MessageCount()
//        {
//            return m_messages.Count;
//        }

//        /**
//         * This function allows the user to determine if this Payload contains a message from a
//         * specified node given a node identifier.
//         */
//        public bool ContainsMessageFromSender(string senderID)
//        {
//            //iterate over each Message to determine if the Payload contains senderID
//            foreach (Message m in m_messages)
//                if (m.senderID == senderID)
//                    return true;
//            //otherwise
//            return false;
//        }

//        /**
//         * This function returns the Message contained at the specified index if it exists.
//         */
//        public Message GetByIndex(int index)
//        {
//            //if index is out of range, return null
//            if (index < 0 || index > (MessageCount() - 1))
//                return null;
//            else
//                return m_messages[index];
//        }

//        /**
//         * This function returns the Message from the sender with the specified id if it exists.
//         */
//        public Message GetBySenderID(string senderID)
//        {
//            foreach (Message m in m_messages)
//                if (m.senderID == senderID)
//                    return m;
//            return null;
//        }

//        /**
//         * This function returns the JSON encoding of this object.
//         */
//        public string ToJsonString()
//        {
//            return JsonConvert.SerializeObject(this);
//        }

//        /**
//         * This function returns a nicely formatted string representation of the object.
//         */
//        public override string ToString()
//        {
//            string s = "Payload=[";
//            if (m_messages.Count > 0)
//            {
//                foreach (Message m in m_messages)
//                    s += m.ToString() + ",";
//                s = s.Substring(0, s.Length - 1); //remove extraneous comma
//            }
//            s += "]";
//            return s;
//        }
//    }
//}
