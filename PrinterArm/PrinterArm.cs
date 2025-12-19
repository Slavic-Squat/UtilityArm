using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class PrinterArm
        {
            private IMyShipController _armController;
            private IMyRemoteControl _remoteControl;
            private IMyTextSurface _display;
            private UserInput _userInput;
            private UserInput _remoteInput;
            private ArmControl _armControl;

            public bool ArmCtrl { get; private set; } = false;
            public bool RemoteCtrl { get; private set; } = false;
            public double Time { get; private set; }
            public PrinterArm()
            {
                _armController = AllGridBlocks.Find(b => b is IMyShipController && b.CustomName.ToUpper().Contains("PRINTER ARM CONTROLLER")) as IMyShipController;
                if (_armController == null)
                {
                    DebugWrite("Controller for printer arm not found!\n", true);
                    throw new Exception("Controller for printer arm not found!\n");
                }
                _remoteControl = AllGridBlocks.Find(b => b is IMyRemoteControl && b.CustomName.ToUpper().Contains("PRINTER ARM RC")) as IMyRemoteControl;
                if (_remoteControl == null)
                {
                    DebugWrite("RC for printer arm not found!\n", true);
                    throw new Exception("RC for printer arm not found!\n");
                }
                IMyTextSurfaceProvider surfaceProvider = _armController as IMyTextSurfaceProvider;
                _display = surfaceProvider.GetSurface(1);

                _userInput = new UserInput(_armController);
                _remoteInput = new UserInput(_remoteControl);
                _armControl = new ArmControl();
            }

            public void Run(double time)
            {
                if (Time == 0)
                {
                    Time = time;
                    return;
                }
                _userInput.Run(time);
                _remoteInput.Run(time);
                _display.WriteText(Status());

                if (!ArmCtrl)
                {
                    return;
                }

                if (!RemoteCtrl)
                {
                    _armControl.Control(_userInput);
                }
                else
                {
                    _armControl.Control(_remoteInput);
                }
                Time = time;
            }

            public void ToggleArmControl()
            {
                ArmCtrl = !ArmCtrl;
            }

            public void ToggleRemoteControl()
            {
                if (!ArmCtrl) return;
                RemoteCtrl = !RemoteCtrl;
            }

            public void CycleArmControlMode()
            {
                if (!ArmCtrl) return;
                _armControl.CycleControlMode();
            }

            public string Status()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[PRINTER ARM STATUS]");
                sb.AppendLine($"  ARM CTRL: {(ArmCtrl ? "ON" : "OFF")}");
                sb.AppendLine($"  REMOTE CTRL: {(RemoteCtrl ? "ON" : "OFF")}");
                sb.AppendLine($"  CTRL MODE: {ArmEnumsHelper.GetArmControlModeStr(_armControl.ControlMode)}");
                sb.AppendLine("  ARM POS:");
                sb.AppendLine($"    - X: {_armControl.EEPosition.X:F2} m, Y: {_armControl.EEPosition.Y:F2} m, Z: {_armControl.EEPosition.Z:F2} m");
                return sb.ToString();
            }
        }
    }
}
