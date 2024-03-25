using System.Diagnostics;
using TwinCAT.Ads;

namespace Tembo;


public class AmrHandler
{
    private readonly AmsAddress _amsnetid = new("10.100.1.10.1.1");
    private readonly string _amrIp = "10.38.4.171";
    private readonly int _amrPort = 7171;

    private AmrController? _amrController;
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
        _amrEstop = new Thread(EmergencyStopAmr);
        _amrHandle = new Thread(HandleAmr);
        _plcEstop = new Thread(EmergencyStopPlc);
    }

    /// <summary>
    /// Start all the threads in amr handler
    /// </summary>
    public void Run()
    {
        _amrEstop.Start();
        _amrHandle.Start();
        _plcEstop.Start();
    }

    private void Connect()
    {
        // setup PLC connection
        _plcController = new PlcController(_amsnetid);

        // setup AMR connection 
        _amrController = new AmrController(_amrPort, _amrIp);
    }

    private void HandleAmr()
    {
        while (_amrRun)
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
            Debug.Assert(_amrController != null, nameof(_amrController) + " != null");
            if (!_amrStop && _amrController.CheckforEstop() && !_plcStop)
            {
                _amrStop = true;
                EmergencyActive();
            }else if (_amrStop && _amrController.CheckMotorsEnabled())
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
            Debug.Assert(_plcController != null, nameof(_plcController) + " != null");
            if (_plcController.PLCSymbol_bool(PlcSymbols.TrayRequest))
            {
                _amrReady = false;
                TrayRequest();
            }
        }
    }

    private void SetToBeginPosition()
    {
        if (!_amrReady)
        {
            Debug.Assert(_amrController != null, nameof(_amrController) + " != null");
            _amrController.Sent_to(AmrPositions.Beginpos);
            _amrController.WaitForArrival(AmrResponses.Beginpos);
            _amrReady = true;
        }
    }

    private void TrayRequest()
    {
        Debug.Assert(_amrController != null, nameof(_amrController) + " != null");
        if (_amrRun)
        {
            _amrController.Sent_to(AmrPositions.DemoRoute);
        }
        // todo: _amrController.WaitForArrival(AmrResponses.klaarroute);
    }

    private void EmergencyActive()
    {
        _amrRun = false; 
        _amrController?.Emergency_Active();
        _plcController?.Emergency_Active();
    }

    private void EmergencyInActive()
    {
        if (!_amrStop && !_plcStop)
        {
            _amrRun = true;
            _plcController?.Emergency_InActive();
            _amrController?.Emergency_InActive();
        }
    }

}