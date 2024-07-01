using System.ComponentModel;
using TwinCAT.Ads;
using TwinCAT.Ads.Server;
using TwinCAT.TypeSystem;

namespace Tembo
{
    internal class PlcController
    {
        private AdsClient _plcConnection = new();

        /// <summary>
        /// Constructor for plc controller to start connection
        /// </summary>
        /// <param name="amsnetid"></param>
        /// <exception cref="Exception"></exception>
        public PlcController(AmsAddress amsnetid) 
        {
            try
            {
                _plcConnection.Connect(amsnetid);
                Console.WriteLine("Connected: " + _plcConnection.IsConnected.ToString());
                if (!_plcConnection.IsConnected)
                {
                    throw new Exception("Twincat not available");
                }
            }
            catch (AdsServerException e)
            {
                _plcConnection.Connect(851);
                Console.WriteLine("Connected: " + _plcConnection.IsConnected.ToString());
                if (!_plcConnection.IsConnected)
                {
                    throw new Exception("Twincat not available");
                }
                Console.WriteLine("Connected via Localhost");
            }
           
            Console.WriteLine("Local Address: " + _plcConnection.Address);
        }

        /// <summary>
        /// Check if a plc symbol is true of false
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool PLCSymbol_bool(PlcSymbols symbol)
        {
            ISymbol result = _plcConnection.ReadSymbol(symbol.GetDescription());
            return (bool)_plcConnection.ReadValue(result);
        }

        public void Set_PLCSymbolFalse(PlcSymbols symbol)
        {
            ISymbol result = _plcConnection.ReadSymbol(symbol.GetDescription());
            _plcConnection.WriteValue(result, false);
        }

        /// <summary>
        /// Activate emergency on the plc
        /// </summary>
        public void Emergency_Active()
        {
            ISymbol result = _plcConnection.ReadSymbol(PlcSymbols.EmergencyStop.GetDescription());
            _plcConnection.WriteValue(result, true);
        }


        /// <summary>
        /// Stop the running emergency and set plc to normal again
        /// </summary>
        public void Emergency_InActive()
        {
            ISymbol result = _plcConnection.ReadSymbol(PlcSymbols.EmergencyStop.GetDescription());
            _plcConnection.WriteValue(result, false);
        }
    }

    /// <summary>
    /// enum with all enum symbols
    /// </summary>
    public enum PlcSymbols
    {
        [Description("GVL.StartButton")]
        StartButton,
        [Description("GVL.PLCReady")]
        PlcReady,
        [Description("GVL.TrayRequest")]
        TrayRequest,
        [Description("GVL.ResetButton")]
        ResetButton, 
        [Description("GVL.stopknop")]
        EmergencyStop
    }
}
