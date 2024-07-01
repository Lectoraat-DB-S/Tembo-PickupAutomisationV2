using System.ComponentModel;
using System.Reflection;


namespace Tembo
{
    public class AmrController
    {
        private readonly TelnetController _connection;

        private bool _eStop;

        private const string Password = "adept\r\n";


        /// <summary>
        /// Constructor for amr controller start connection
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ip"></param>
        public AmrController(int port, string ip)
        {
            Console.WriteLine("AmrController");

            _connection = new TelnetController(ip, port);
            Console.WriteLine("Connection");

            // exception neer zetten 
            _connection.WaitForMessage("Enter password:\r\n");
            _connection.SendMessage(Password);
            Console.WriteLine(Password);
            _connection.WaitForMessageContains("End of commands\r\n");
        }

        /// <summary>
        /// Shut AMR down stop everything
        /// </summary>
        public void Emergency_Active()
        {
            // turn top motors off
            _eStop = true;
            _connection.SendMessage("OutputOff o1\r\n");
            _connection.SendMessage("OutputOff o3\r\n");
        }

        /// <summary>
        /// Start amr up again
        /// </summary>
        public bool Emergency_InActive()
        {
            Console.WriteLine("emergency inactive");
            _eStop = false;

            _connection.SendMessage(Macros.MotorsDown.GetDescription());
            bool done = false;
            while (!done && !_eStop)// motors omlaag om zeker te zijn van motor positie
            {
                //Console.WriteLine("while loop");
                string response = _connection.ReadMessage();
                Console.WriteLine("WhileLoop: " + response);
                if (response.Equals("EStop pressed\r\n"))
                {
                    _eStop = true;
                    Console.WriteLine("------------------------------------Estop While Starting up");
                    return false;
                }else if (response.Contains("Completed macro"))
                {
                    Console.WriteLine("Estop Macro Completed: " + Macros.MotorsDown.ToString());
                    done = true;
                }
            }
            Console.WriteLine("while loop done");
            return true;
        }

        /// <summary>
        /// Check if there is an emergency stop and wait for one
        /// </summary>
        /// <returns></returns>
        public bool CheckForEstop()
        {
            Console.WriteLine("Estop");
            _connection.WaitForMessageContains("EStop pressed");
            return true;
        }

        /// <summary>
        /// Check if the motors are enabled and work
        /// </summary>a
        /// <returns></returns>
        public bool CheckMotorsEnabled()
        {
            _connection.WaitForMessage("Motors enabled\r\n");
            return true;
        }

        /// <summary>
        /// Move amr the given distance at the given speed
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="speed"></param>

        private void MoveAmr(int dist, int speed)
        {
            _connection.SendMessage("doTask move " + dist + " " + speed + "\r\n"); //Move dist
            bool reached = false;
            while (!reached && !_eStop)
            {
                string response = _connection.ReadMessage(); // done
                if (response.Equals("Completed doing task move " + dist + " " + speed + "\r\n"))
                {
                    reached = true;
                }
                else if (response.Equals("Failed doing task move " + dist + " " + speed + "\r\n"))
                {
                    _eStop = true;
                }
            }
        }

        /// <summary>
        /// Make sound to notify operator that emergency stop is active
        /// </summary>
        public void SayEstop()
        {
            _connection.SendMessage("DoTask say A.M.R.stuck.in.Trayrequest\r\n");
        }

        /// <summary>
        /// Sent amr to one of the positions in the enum positions
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="res"></param>
        public void Send_to(AmrPositions pos, AmrResponses res)
        {
            Console.WriteLine(pos.GetDescription());
            _connection.SendMessage(pos.GetDescription());
            bool reached = false;
            Console.WriteLine("Estop amr: " + _eStop);
            while (!reached && !_eStop)
            {
                string response = _connection.ReadMessage(); // done
                if (response.Equals(res.GetDescription()))
                {
                    reached = true;
                }
                else if (response.Equals("Failed  going to goal\r\n"))
                {
                    _eStop = true;
                }
            }
        }

        /// <summary>
        /// Sent amr to one of the positions in the enum positions
        /// </summary>
        /// <param name="macro"></param>
        public void DoMacro(Macros macro)
        {
            _connection.SendMessage(macro.GetDescription());
            bool done = false;
            while (!done && !_eStop)
            {
                string response = _connection.ReadMessage();
                if (response.Contains("Completed macro"))
                {
                    Console.WriteLine("Macro: " + macro.ToString());
                    done = true;
                }
            }
        }

        /// <summary>
        /// Execute a trayrequest to get the tray
        /// </summary>
        /// <returns></returns>
        public bool TrayRequest()
        {
            if (_eStop) return true;
            DoMacro(Macros.MotorsUp); //MotorsUp macro

            if (_eStop) return true;
            MoveAmr(1350, 100);

            if (_eStop) return true;
            DoMacro(Macros.MotorsDown); //Motors down

            if (_eStop) return true;
            MoveAmr(680, 100);

            if (_eStop) return true;
            DoMacro(Macros.MotorsUp); //Motors up

            if (_eStop) return true;
            MoveAmr(800, 100);

            return false;
        }

    }

    /// <summary>
    /// Enumarator with all the amr positions
    /// </summary>
    public enum AmrPositions
    {
        [Description("GoTo WaitForTrayRequest\r\n")]
        WaitForTrayRequest,
        [Description("GoTo TestOpstelling\r\n")]
        TestOpstelling
    }

    /// <summary>
    /// Enumarator with all macros on the amr
    /// </summary>
    public enum Macros
    {
        [Description("ExecuteMacro MotorsUp\r\n")]
        MotorsUp,
        [Description("ExecuteMacro MotorsDown\r\n")]
        MotorsDown
    }

    /// <summary>
    /// Enum with al responses the amr can give
    /// </summary>
    public enum AmrResponses
    {
        [Description("Arrived at WaitForTrayRequest\r\n")]
        WaitForTrayRequest,
        [Description("Arrived at TestOpstelling\r\n")]
        TestOpstelling
    }

    /// <summary>
    /// Class to get the description from enums to make naming easier
    /// </summary>
    public static class Extensions
    {
        public static string GetDescription(this Enum e)
        {
            var attribute =
                e.GetType()
                    .GetTypeInfo()
                    .GetMember(e.ToString())
                    .FirstOrDefault(member => member.MemberType == MemberTypes.Field)
                    ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .SingleOrDefault()
                    as DescriptionAttribute;

            return attribute?.Description ?? e.ToString();
        }
    }
}
