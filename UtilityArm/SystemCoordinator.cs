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
        public class SystemCoordinator
        {
            public UtilityArm UtilityArm { get; private set; }

            private double _lastRunTime = 0;
            public SystemCoordinator()
            {
                Init();
            }

            private void Init()
            {
                string armID = Config.Get("Config", "ArmID").ToString("Utility");
                Config.Set("Config", "ArmID", armID);
                UtilityArm = new UtilityArm(armID);

                CommandHandlerInst.RegisterCommand("TOGGLE_ARM_CTRL", (args) => ToggleArmControl());
                CommandHandlerInst.RegisterCommand("CYCLE_ARM_CTRL_MODE", (args) => CycleArmControlMode());
                CommandHandlerInst.RegisterCommand("CYCLE_ATTACHMENT", (args) => CycleAttachment());
                CommandHandlerInst.RegisterCommand("CYCLE_TRANSLATION_MODE", (args) => CycleTranslationMode());
                CommandHandlerInst.RegisterCommand("SET_TRANS_SPEED", (args) => { if (args.Length > 0) SetTranslationSpeed(args[0]); });
                CommandHandlerInst.RegisterCommand("ADJUST_TRANS_SPEED", (args) => { if (args.Length > 0) AdjustTranslationSpeed(args[0]); });
            }

            public void Run(double time)
            {
                if (_lastRunTime == 0)
                {
                    _lastRunTime = time;
                    return;
                }
                UtilityArm.Run(time);
                _lastRunTime = time;
            }

            public void ToggleArmControl()
            {
                UtilityArm.ToggleArmControl();
            }

            public void CycleArmControlMode()
            {
                UtilityArm.CycleArmControlMode();
            }

            public void CycleAttachment()
            {
                UtilityArm.CycleAttachment();
            }

            public void CycleTranslationMode()
            {
                UtilityArm.CycleTranslationMode();
            }

            public void SetTranslationSpeed(string speedStr)
            {
                float speed;
                if (!float.TryParse(speedStr, out speed))
                    return;
                UtilityArm.SetTranslationSpeed(speed);
            }

            public void AdjustTranslationSpeed(string deltaStr)
            {
                float delta;
                if (!float.TryParse(deltaStr, out delta))
                    return;
                UtilityArm.AdjustTranslationSpeed(delta);
            }
        }
    }
}
