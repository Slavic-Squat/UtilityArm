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
            private float _yaw;
            private float _pitch;
            private float _roll;

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

                Vector3 gravVector = _armController.GetNaturalGravity();
                Matrix inputDir = Matrix.Identity;
                Vector3 rightVector = Vector3.Normalize(Vector3.Cross(_armController.WorldMatrix.Backward, gravVector));
                Vector3 backwardVector = Vector3.Normalize(Vector3.Cross(gravVector, rightVector));
                Vector3 upVector = -1f * Vector3.Normalize(gravVector);
                inputDir.Backward = backwardVector;
                inputDir.Right = rightVector;
                inputDir.Up = upVector;

                Quaternion roll = Quaternion.CreateFromAxisAngle(backwardVector, _roll);
                Quaternion pitch = Quaternion.CreateFromAxisAngle(rightVector, _pitch);
                Quaternion yaw = Quaternion.CreateFromAxisAngle(upVector, _yaw);
                Quaternion totalRot = yaw * pitch * roll;
                inputDir = Matrix.Transform(inputDir, totalRot);
                Matrix inputDirLocal = inputDir * Matrix.Transpose(_armController.WorldMatrix.GetOrientation());

                if (!RemoteCtrl)
                {
                    return _armControl.Control(_userInput, inputDirLocal);
                }
                else
                {
                    return _armControl.Control(_remoteInput, inputDirLocal);
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

            public bool AdjustYaw(float amountDeg)
            {
                if (!ArmCtrl) return false;
                _yaw += MathHelper.ToRadians(amountDeg);
                _yaw = MathHelper.Clamp(_yaw, MathHelper.ToRadians(-45), MathHelper.ToRadians(45));
                return true;
            }

            public bool AdjustPitch(float amountDeg)
            {
                if (!ArmCtrl) return false;
                _pitch += MathHelper.ToRadians(amountDeg);
                _pitch = MathHelper.Clamp(_pitch, MathHelper.ToRadians(-45), MathHelper.ToRadians(45));
                return true;
            }

            public bool AdjustRoll(float amountDeg)
            {
                if (!ArmCtrl) return false;
                _roll += MathHelper.ToRadians(amountDeg);
                _roll = MathHelper.Clamp(_roll, MathHelper.ToRadians(-45), MathHelper.ToRadians(45));
                return true;
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
