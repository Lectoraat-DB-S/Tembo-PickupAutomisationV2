using System.Diagnostics;
using TwinCAT.Ads;

namespace Tembo;


public class AmrHandler
{
    private readonly AmsAddress _amsnetid = new("10.100.1.10.1.1");
    private readonly string _amrIp = "10.38.4.171";
    private readonly int _amrPort = 7171;

    private AmrController? _amrControllerRun;
    private AmrController? _amrControllerEStop; 
    private PlcController? _plcController;

    private bool _amrReady;
    private bool _amrRun = true;
    private bool _amrStop;
    private bool _plcStop;

    private Thread _amrEstop;
    private Thread _amrHandle;
    private Thread _plcEstop;
    
    /// <summary>
    /// Constructor for AmrHandler 
    /// </summary>
    public AmrHandler()
    {
        Connect();
        Console.WriteLine("Connected");
        _amrEstop = new Thread(EmergencyStopAmr);
        _amrHandle = new Thread(HandleAmr);
        //_plcEstop = new Thread(EmergencyStopPlc);
    }

    /// <summary>
    /// Start all the threads in amr handler
    /// </summary>
    public void Run()
    {
        _amrEstop.Start();
        _amrHandle.Start();
        //_plcEstop.Start();
    }

    private void Connect()
    {
        // setup PLC connection
        //_plcController = new PlcController(_amsnetid);

        // setup AMR Run connection 
        _amrControllerRun = new AmrController(_amrPort, _amrIp, "Run");

        // setop AMR Estop connection
        _amrControllerEStop = new AmrController(_amrPort, _amrIp, "Estop");
    }

    private void HandleAmr()
    {
        if (_amrRun)
        {
            SetToBeginPositionAndWait();
        }
        // ReSharper disable once FunctionNeverReturns
    }


    private void EmergencyStopAmr()
    {
        // Check if amr is running and plc is not in stop
        while (_amrRun && !_plcStop)
        {
            Debug.Assert(_amrControllerEStop != null, nameof(_amrControllerEStop) + " != null");
            if (!_amrStop && _amrControllerEStop.CheckForEstop() && !_plcStop)
            {
                Console.WriteLine("EMERGENCY! AMR");
                _amrStop = true;
                EmergencyActive();
            }else if (_amrStop && _amrControllerEStop.CheckMotorsEnabled())
            {
                _amrStop = false;
                EmergencyInActive();
            }
        }
    }

    private void EmergencyStopPlc()
    {
        while (_amrRun && !_amrStop)
        {
            Debug.Assert(_plcController != null, nameof(_plcController) + " != null");
            bool emergency = _plcController.PLCSymbol_bool(PlcSymbols.EmergencyStop);
            if (!_plcStop && emergency && !_amrStop)
            {
                _plcStop = true;
                EmergencyActive();
            }else if (_plcStop && !emergency)
            {
                _plcStop = false;
                EmergencyInActive();
            }
        }
    }

    
    /// <summary>
    /// check if amr is at begin position if not set to begin position
    /// </summary>
    private void SetToBeginPositionAndWait()
    {
        SetToBeginPosition();
        while (_amrReady && _amrRun)
        {
            //Debug.Assert(_plcController != null, nameof(_plcController) + " != null");
            Console.Write("TrayRequest:");
            string consoleResponse = Console.ReadLine();
            if (consoleResponse.Equals("True")) //_plcController.PLCSymbol_bool(PlcSymbols.TrayRequest))
            {
                _amrReady = false;
                TrayRequest();
            }
        }
    }

    private void SetToBeginPosition()
    {
        Console.WriteLine("Beginpos");
        if (!_amrReady)
        {
            Console.WriteLine("amr ready true");
            Debug.Assert(_amrControllerRun != null, nameof(_amrControllerRun) + " != null");
            //Console.WriteLine(_amrController._connection.ReadMessage());
            _amrControllerRun.Sent_to(AmrPositions.Beginpos);
            _amrControllerRun.WaitForArrival(AmrResponses.Beginpos);
            _amrReady = true;
        }
    }

    private void TrayRequest()
    {
        Debug.Assert(_amrControllerRun != null, nameof(_amrControllerRun) + " != null");
        if (_amrRun)
        {
            _amrControllerRun.TrayRequest();
        }
        SetToBeginPositionAndWait();
    }

    private void EmergencyActive()
    {
        _amrRun = false;
        _amrControllerEStop?.Emergency_Active();
        _plcController?.Emergency_Active();
    }

    private void EmergencyInActive()
    {
        if (!_amrStop && !_plcStop)
        {
            _amrRun = true;
            _plcController?.Emergency_InActive();
            _amrControllerEStop?.Emergency_InActive();
        }
    }

}