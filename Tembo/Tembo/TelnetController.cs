using System.Net.Sockets;

namespace Tembo
{
    public class TelnetController
    {
        Stream _stream;

        /// <summary>
        /// Constructor for telnet controller to start connection 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="portNumber"></param>
        public TelnetController(string ipAddress, int portNumber)
        {
            try
            {
                var client = new TcpClient(ipAddress, portNumber);
                _stream = client.GetStream();
            }
            catch
            {
                Console.WriteLine("Failed to connect to " + ipAddress); 
                Environment.Exit(1);
            }
        }
        /// <summary>
        /// Send message via telnet
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public void SendMessage(string command)
        {
            Console.WriteLine("sending: " + command);
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(command);
            _stream.Write(data, 0, data.Length);
            Thread.Sleep(10);
        }
        /// <summary>
        /// read message via telnet
        /// </summary>
        /// <returns></returns>
        public string ReadMessage()
        {
            Byte[] responseData = new byte[256];
            Int32 numberOfBytesRead = _stream.Read(responseData, 0, responseData.Length);
            string response = System.Text.Encoding.ASCII.GetString(responseData, 0, numberOfBytesRead);
            return response;
        }
        /// <summary>
        /// Wait for a specific message
        /// </summary>
        /// <param name="expectedMessage"></param>
        /// <returns></returns>
        public bool WaitForMessage(string expectedMessage)
        {
            bool recieved = false;
            while (!recieved)
            {
                string message = ReadMessage();
                Console.WriteLine("message: " + message + " = " + message.Equals(expectedMessage) + "\r\nWanted: " + expectedMessage);
                if (message.Equals(expectedMessage))
                {
                    Console.WriteLine("Recieved");
                    recieved= true;
                }
            }
            return recieved;
        }

        public bool WaitForMessageContains(string expectedMessage)
        {
            Console.WriteLine("Contains");
            bool recieved = false;
            while (!recieved)
            {
                string message = ReadMessage();
                Console.WriteLine("message: " + message + " = " + message.Equals(expectedMessage) + "\r\nWanted: " + expectedMessage);
                if (message.Contains(expectedMessage))
                {
                    Console.WriteLine("Recieved");
                    recieved = true;
                }
            }
            return recieved;
        }
    }
}
