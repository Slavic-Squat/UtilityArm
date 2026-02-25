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
using static IngameScript.Program;

namespace IngameScript
{
    partial class Program
    {
        public class UtilityArm
        {
            private IMyShipController _armController;
            private IMyTextSurface _display;
            private UserInput _armInput;
            private ArmControl _armControl;
            private StringBuilder _sb = new StringBuilder();
            private double _lastRunTime = 0;
            private bool _armCtrl = false;
            public string ID { get; private set; }
            public UtilityArm(string id)
            {
                ID = id.ToUpper();
                _armController = AllBlocks.FirstOrDefault(b => b is IMyShipController && b.CustomName.ToUpper().Contains($"{ID} ARM CONTROLLER")) as IMyShipController;
                if (_armController == null)
                {
                    throw new Exception("Controller for arm not found!");
                }
                IMyTextSurfaceProvider surfaceProvider = _armController as IMyTextSurfaceProvider;
                _display = surfaceProvider.GetSurface(0);

                _armInput = new UserInput(_armController);
                _armControl = new ArmControl(ID);
            }

            public void Run(double time)
            {
                if (_lastRunTime == 0)
                {
                    _lastRunTime = time;
                    return;
                }
                _armInput.Run(time);
                _sb.Clear();
                AppendOverview(_sb);
                _display.WriteText(_sb);

                if (_armCtrl)
                {
                    _armControl.Control(_armInput);
                }

                _lastRunTime = time;
            }

            public void ToggleArmControl()
            {
                _armCtrl = !_armCtrl;
            }

            public void CycleArmControlMode()
            {
                _armControl.CycleControlMode();
            }

            public void CycleAttachment()
            {
                _armControl.CycleAttachment();
            }

            public void CycleTranslationMode()
            {
                if (_armControl.ControlMode != ArmControlMode.Translate) return;
                _armControl.CycleTranslationMode();
            }

            public void SetTranslationSpeed(float speed)
            {
                if (_armControl.ControlMode != ArmControlMode.Translate) return;
                _armControl.SetTranslationSpeed(speed);
            }

            public void AdjustTranslationSpeed(float delta)
            {
                if (_armControl.ControlMode != ArmControlMode.Translate) return;
                _armControl.AdjustTranslationSpeed(delta);
            }

            public void AppendOverview(StringBuilder sb)
            {
                sb.AppendLine("[ARM OVERVIEW]");
                sb.Append("  ARM CTRL: ").AppendLine(_armCtrl ? "ON" : "OFF");
                sb.Append("  CTRL MODE: ").AppendLine(ArmEnumsHelper.GetArmControlModeStr(_armControl.ControlMode));
                if (_armControl.ControlMode == ArmControlMode.Translate)
                {
                    sb.Append("    - TRANS MODE: ").AppendLine(ArmEnumsHelper.GetTranslationModeStr(_armControl.TranslationMode));
                    sb.Append("    - SPEED: ").AppendFormat("{0:F2} m/s", _armControl.TranslationSpeed).AppendLine();
                }
                sb.Append("  ATTACHMENT: ").AppendLine(ArmEnumsHelper.GetAttachmentStr(_armControl.Attachment));
                sb.AppendLine("  ARM POS:");
                sb.AppendFormat("    - X: {0:F2} m, Y: {1:F2} m, Z: {2:F2} m", _armControl.EEPosition.X, _armControl.EEPosition.Y, _armControl.EEPosition.Z);
            }
        }
    }
}
