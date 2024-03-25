using System.ComponentModel;
using System.Reflection;


namespace Tembo
{
    public class AmrController
    {
        TelnetController _connection;

        const string Password = "admin";

        /// <summary>
        /// Constructor for amr controller start connection
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ip"></param>
        public AmrController(int port, string ip)
        {
            _connection = new TelnetController(ip, port);
            // exception neer zetten
            _connection.WaitForMessage("Enter password");
            _connection.SendMessage(Password);
            _connection.WaitForMessage("End of commands");
        }

        /// <summary>
        /// Shut AMR down stop everything
        /// </summary>
        public void Emergency_Active()
        {
            // turn top motors off
            _connection.SendMessage("outputOff 01");
            _connection.SendMessage("outputOff 02");
            _connection.SendMessage("outputOff 03");
            _connection.SendMessage("outputOff 04");
        }

        /// <summary>
        /// Start amr up again
        /// </summary>
        public void Emergency_InActive()
        {
            //todo: set top motors to latest state
        }

        /// <summary>
        /// Sent amr to one of the positions in the enum positions
        /// </summary>
        /// <param name="pos"></param>
        public void Sent_to(AmrPositions pos)
        {
            _connection.SendMessage(pos.GetDescription());
        }
        /// <summary>
        /// Wait for the amr to arrive at its location
        /// </summary>
        /// <param name="res"></param>
        public void WaitForArrival(AmrResponses res)
        {
            _connection.WaitForMessage(res.GetDescription());
        }

        /// <summary>
        /// Check if there is an emergency stop and wait for one
        /// </summary>
        /// <returns></returns>
        public bool CheckforEstop()
        {
            _connection.WaitForMessage("EStop pressed");
            return true;
        }

        /// <summary>
        /// Check if the motors are enabled and work
        /// </summary>
        /// <returns></returns>
        public bool CheckMotorsEnabled()
        {
            _connection.WaitForMessage("Motors enabled");
            return true;
        }

    }

    /// <summary>
    /// Enumarator with all the amr positions
    /// </summary>
    public enum AmrPositions
    {
        [Description("GoTo Beginpositie_AMR")]
        Beginpos,
        [Description("patrolonce DemoRoute")]
        DemoRoute

    }

    /// <summary>
    /// Enum with al responses the amr can give
    /// </summary>
    public enum AmrResponses
    {
        [Description()]
        Beginpos
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
