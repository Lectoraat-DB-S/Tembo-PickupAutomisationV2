using System.Net.Sockets;

namespace Tembo
{
    public class TelnetController
    {
        TcpClient client;
        Stream stream;
        public TelnetController(string ip_address, int port_number)
        {
            try
            {
                client = new TcpClient(ip_address, port_number);
                stream = client.GetStream();
            }
            catch
            {
                Console.WriteLine("Failed to connect to " + ip_address);
                System.Environment.Exit(1);
            }
        }

        public string SendMessage(string command)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(command);
            stream.Write(data, 0, data.Length);
            Thread.Sleep(10);
            string response = ReadMessage();

            return response;
        }

        public string ReadMessage()
        {
            Byte[] responseData = new byte[256];
            Int32 numberOfBytesRead = stream.Read(responseData, 0, responseData.Length);
            string response = System.Text.Encoding.ASCII.GetString(responseData, 0, numberOfBytesRead);
            return response;
        }

        public bool WaitForMessage(string expectedMessage)
        {
            bool recieved = false;
            while (!recieved)
            {
                if (expectedMessage.Equals(ReadMessage()))
                {
                    recieved= true;
                }
            }
            return recieved;
        }
    }
}
