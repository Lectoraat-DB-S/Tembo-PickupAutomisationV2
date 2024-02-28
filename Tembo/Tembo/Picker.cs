using System.Drawing.Printing;
using Tembo;
using TwinCAT.Ads;

public class Picker
{
    private AmsAddress AMSNETID = new("10.100.1.10.1.1");
    private AdsClient PLCconnection = new();
    private string AMR_IP = "10.38.4.171";
    private int AMR_PORT = 7171;

    TelnetController AMR_Connection;

    private bool AMR_ESTOP = false;
    private bool AMR_READY = false;


    public Picker()
    {
        connect();
        events_PLC();
    }

    private void connect()
    {
        // setup PLC connection
        PLCconnection.Connect(AMSNETID);
        Console.WriteLine("Connected: " + PLCconnection.IsConnected.ToString());
        Console.WriteLine("Local Address: " + PLCconnection.Address);
        Console.WriteLine("Symbols: " + PLCconnection.ReadSymbol);

        // setup AMR connection 
        AMR_Connection = new TelnetController(AMR_IP, AMR_PORT);
        AMR_Connection.WaitForMessage("EnterPassword:\r\n");
        AMR_Connection.SendMessage("Admin");
        AMR_Connection.WaitForMessage("End of commands\r\n");
        idle();
    }

    private void idle()
    {
        AMR_Connection.SendMessage("GoTo Beginpositie_AMR\r\n");
        AMR_Connection.WaitForMessage("Arrived at BeginPositie_AMR");
        AMR_READY= true;

        // misschien nog wat doen voor vision camera
    }

    private void trayRequest()
    {
        AMR_Connection.SendMessage("patrolonce DemoRoute");
    }

    private void emergency()
    {
        AMR_ESTOP= true;
        AMR_Connection.SendMessage("outputOff 01");
        AMR_Connection.SendMessage("outputOff 02");
        AMR_Connection.SendMessage("outputOff 03");
        AMR_Connection.SendMessage("outputOff 04");
        // zet alles stop
    }



    private void events_PLC()
    {
        // alle knoppen kunnen misschien met events worden behandeld zou beter werken
    }
}