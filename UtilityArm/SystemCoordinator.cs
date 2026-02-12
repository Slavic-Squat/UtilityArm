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

            private double _time = 0;
            public SystemCoordinator()
            {
                Init();                
            }

            private void Init()
            {
                string armID = Config.Get("Config", "ArmID").ToString("Utility");
                Config.Set("Config", "ArmID", armID);
                UtilityArm = new UtilityArm(armID);

                CommandHandler0.RegisterCommand("TOGGLE_ARM_CTRL", (args) => ToggleArmControl());
                CommandHandler0.RegisterCommand("CYCLE_ARM_CTRL_MODE", (args) => CycleArmControlMode());
                CommandHandler0.RegisterCommand("CYCLE_ATTACHMENT", (args) => CycleAttachment());
                CommandHandler0.RegisterCommand("CYCLE_TRANSLATION_MODE", (args) => CycleTranslationMode());
            }

            public void Run(double time)
            {
                if (_time == 0)
                {
                    _time = time;
                    return;
                }
                UtilityArm.Run(time);
                _time = time;
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
        }
    }
}
