using System.ComponentModel;
using System.Reflection;


namespace Tembo
{
    public class AmrController
    {
        private TelnetController _connection;

        private readonly string _move = "doTask move ";
        private readonly string _moveDone = "Completed doing task move ";

        const string Password = "adept\r\n";

        public string name;

        /// <summary>
        /// Constructor for amr controller start connection
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ip"></param>
        public AmrController(int port, string ip, string name)
        {
            Console.WriteLine("AmrController");

            _connection = new TelnetController(ip, port);
            Console.WriteLine("Connection");

            // exception neer zetten 
            _connection.WaitForMessage("Enter password:\r\n", name);
            _connection.SendMessage(Password);
            Console.WriteLine(Password);
            _connection.WaitForMessageContains("End of commands\r\n");
            this.name = name;
        }

        /// <summary>
        /// Shut AMR down stop everything
        /// </summary>
        public void Emergency_Active()
        {
            // turn top motors off
            _connection.SendMessage("outputOff 01");
            _connection.SendMessage("outputOff 03");
        }

        /// <summary>
        /// Start amr up again
        /// </summary>
        public void Emergency_InActive()
        {
            _connection.SendMessage(""); // Move een stuk om veilig te staan
            // motors omlaag
            // zet terug op start positie
            //todo: set top motors to latest state
        }

        /// <summary>
        /// Sent amr to one of the positions in the enum positions
        /// </summary>
        /// <param name="pos"></param>
        public void Sent_to(AmrPositions pos)
        {
            Console.WriteLine(pos.GetDescription());
            _connection.SendMessage(pos.GetDescription());
        }
        /// <summary>
        /// Wait for the amr to arrive at its location
        /// </summary>
        /// <param name="res"></param>
        public void WaitForArrival(AmrResponses res)
        {
            Console.WriteLine(name + ": Checking");
            _connection.WaitForMessage(res.GetDescription(), name);
        }

        /// <summary>
        /// Check if there is an emergency stop and wait for one
        /// </summary>
        /// <returns></returns>
        public bool CheckForEstop()
        {
            Console.WriteLine(name + ": Estop");
            _connection.WaitForMessage("EStop pressed\r\n", name);
            return true;
        }

        /// <summary>
        /// Check if the motors are enabled and work
        /// </summary>a
        /// <returns></returns>
        public bool CheckMotorsEnabled()
        {
            _connection.WaitForMessage("Motors enabled", name);
            return true;
        }

        public void TrayRequest()
        {
            _connection.SendMessage(macros.MotorsUp.GetDescription()); //MotorsUp macro
            _connection.WaitForMessageContains("Completed macro MotorsUp\r\n"); // done

            _connection.SendMessage(AmrPositions.Beginpos.GetDescription()); //Go To Wait For tray
            _connection.WaitForMessage(AmrResponses.Beginpos.GetDescription(), name); // done

            _connection.SendMessage(_move + 1300 + "\r\n"); //Move 1300
            _connection.WaitForMessageContains(_moveDone + 1300 + "\r\n"); // done

            _connection.SendMessage(macros.MotorsDown.GetDescription()); //Motors down
            _connection.WaitForMessageContains("Completed macro MotorsDown\r\n"); // done

            _connection.SendMessage(_move + 680 + "\r\n"); //Move 690
            _connection.WaitForMessageContains(_moveDone + 680 + "\r\n"); // done

            _connection.SendMessage(macros.MotorsUp.GetDescription()); //Motors up
            _connection.WaitForMessageContains("Completed macro MotorsUp\r\n"); // done

            _connection.SendMessage(_move + 800 + "\r\n"); //Move 800
            _connection.WaitForMessageContains(_moveDone + 800 + "\r\n"); // done

            _connection.SendMessage(AmrPositions.R2D2.GetDescription()); //Go to R2D2
            _connection.WaitForMessage(AmrResponses.R2D2.GetDescription(), name); // done

        }

    }

    /// <summary>
    /// Enumarator with all the amr positions
    /// </summary>
    public enum AmrPositions
    {
        [Description("GoTo WaitForTrayRequest\r\n")]
        Beginpos,
        [Description("GoTo R2-D2\r\n")]
        R2D2,
        [Description("patrolonce TemboTest\r\n")]
        TemboTest

    }

    /// <summary>
    /// Enumarator with all macros on the amr
    /// </summary>
    public enum macros
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
        Beginpos,
        [Description("Finished patrolling route TemboTest\r\n")]
        Patrol,
        [Description("Arrived at R2-D2\r\n")]
        R2D2
    }

    /// <summary>
    /// Class to get the description from enums to make naming easyer
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
