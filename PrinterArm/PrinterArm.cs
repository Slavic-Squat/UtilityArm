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
            private IMyTextSurface _display0;
            private IMyTextSurface _display1;
            private UserInput _userInput;
            private UserInput _remoteInput;
            private ArmControl _armControl;

            public bool ArmCtrl { get; private set; } = false;
            public bool RemoteCtrl { get; private set; } = false;
            public PrinterArm()
            {
                _armController = GTS.GetBlockWithName("HAB_0 Printer Arm Controller") as IMyShipController;
                if (_armController == null)
                    throw new Exception("Printer Arm Controller not found on HAB_0");
                _remoteControl = GTS.GetBlockWithName("HAB_0 Printer Arm RC") as IMyRemoteControl;
                if (_remoteControl == null)
                {
                    throw new Exception("Printer Arm RC not found on HAB_0");
                }
                IMyTextSurfaceProvider surfaceProvider = _armController as IMyTextSurfaceProvider;
                _display0 = surfaceProvider.GetSurface(0);
                _display1 = surfaceProvider.GetSurface(1);

                _userInput = new UserInput(_armController);
                _remoteInput = new UserInput(_remoteControl);
                _armControl = new ArmControl();
            }

            public bool Run(double time)
            {
                _userInput.Run(time);
                _remoteInput.Run(time);
                _display1.WriteText(Status());

                if (!ArmCtrl)
                {
                    return false;
                }

                if (!RemoteCtrl)
                {
                    return _armControl.Control(_userInput);
                }
                else
                {
                    return _armControl.Control(_remoteInput);
                }                
            }

            public bool ToggleArmControl()
            {
                ArmCtrl = !ArmCtrl;
                return true;
            }

            public bool ToggleRemoteControl()
            {
                if (!ArmCtrl) return false;
                RemoteCtrl = !RemoteCtrl;
                return true;
            }

            public bool CycleArmControlMode()
            {
                if (!ArmCtrl) return false;
                return _armControl.CycleControlMode();
            }

            public string Status()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[Printer Arm Status]");
                sb.AppendLine($"  Arm Control: {(ArmCtrl ? "ON" : "OFF")}");
                sb.AppendLine($"  Remote Control: {(RemoteCtrl ? "ON" : "OFF")}");
                sb.AppendLine($"  Control Mode: {GetName(_armControl.ControlMode)}");
                sb.AppendLine("  Arm Position:");
                sb.AppendLine($"    X: {_armControl.EEPosition.X:F2}, Y: {_armControl.EEPosition.Y:F2}, Z: {_armControl.EEPosition.Z:F2}");
                return sb.ToString();
            }
        }
    }
}
