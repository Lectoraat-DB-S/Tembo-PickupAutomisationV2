using System.ComponentModel;
using TwinCAT.Ads;
using TwinCAT.TypeSystem;

namespace Tembo
{
    internal class PlcController
    {
        private AdsClient _plCconnection = new();

        /// <summary>
        /// Constructor for plc controller to start connection
        /// </summary>
        /// <param name="amsnetid"></param>
        /// <exception cref="Exception"></exception>
        public PlcController(AmsAddress amsnetid) 
        {
            _plCconnection.Connect(amsnetid);
            Console.WriteLine("Connected: " + _plCconnection.IsConnected.ToString());
            if (!_plCconnection.IsConnected)
            {
                throw new Exception("Twincat not available");
            }
            Console.WriteLine("Local Address: " + _plCconnection.Address);
        }

        /// <summary>
        /// Check if a plc symbol is true of false
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool PLCSymbol_bool(PlcSymbols symbol)
        {
            ISymbol result = _plCconnection.ReadSymbol(symbol.GetDescription());
            return (bool)_plCconnection.ReadValue(result);
        }

        /// <summary>
        /// Activate emergency on the plc
        /// </summary>
        public void Emergency_Active()
        {
            ISymbol result = _plCconnection.ReadSymbol(PlcSymbols.EmergencyStop.GetDescription());
            _plCconnection.WriteValue(result, true);
        }


        /// <summary>
        /// Stop the running emergency and set plc to normal again
        /// </summary>
        public void Emergency_InActive()
        {
            ISymbol result = _plCconnection.ReadSymbol(PlcSymbols.EmergencyStop.GetDescription());
            _plCconnection.WriteValue(result, false);
        }
    }

    /// <summary>
    /// enum with all enum symbols
    /// </summary>
    public enum PlcSymbols
    {
        [Description("IO.StartButton")]
        StartButton,
        [Description("IO.PLC_Ready")]
        PlcReady,
        [Description("IO.TrayRequest")]
        TrayRequest,
        [Description("IO.ResetButton")]
        ResetButton, 
        [Description("IO.EmergencyStop")]
        EmergencyStop
    }
}
