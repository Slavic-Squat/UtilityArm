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
            private IEnumerator<bool> _anchorCoroutine;

            public bool ArmCtrl { get; private set; } = false;
            public bool RemoteCtrl { get; private set; } = false;
            public PrinterArm()
            {
                _armController = GTS.GetBlockWithName("Arm Controller") as IMyShipController;
                if (_armController == null)
                    throw new Exception("Arm Controller not found");
                _remoteControl = GTS.GetBlockWithName("Remote Control") as IMyRemoteControl;
                if (_remoteControl == null)
                {
                    throw new Exception("Remote Control not found");
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
                _display0.WriteText(Status());

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

            public bool ToggleOrientationControl()
            {
                if (!ArmCtrl) return false;
                return _armControl.ToggleOrientationControl();
            }

            public string Status()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[DrillArm Status]");
                sb.AppendLine($"  Arm Control: {(ArmCtrl ? "ON" : "OFF")}");
                sb.AppendLine($"  Remote Control: {(RemoteCtrl ? "ON" : "OFF")}");
                sb.AppendLine($"  Orientation Control: {(_armControl.OCtrl ? "ON" : "OFF")}");
                sb.AppendLine($"  Yaw: {MathHelper.ToDegrees(_yaw):F2} deg");
                sb.AppendLine($"  Pitch: {MathHelper.ToDegrees(_pitch):F2} deg");
                sb.AppendLine($"  Roll: {MathHelper.ToDegrees(_roll):F2} deg");
                sb.AppendLine("  Arm Position:");
                sb.AppendLine($"    X: {_armControl.EEPosition.X:F2}, Y: {_armControl.EEPosition.Y:F2}, Z: {_armControl.EEPosition.Z:F2}");
                return sb.ToString();
            }
        }
    }
}
