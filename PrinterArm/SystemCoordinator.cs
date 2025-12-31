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
            public PrinterArm PrinterArm { get; private set; }
            public double Time { get; private set; }

            public SystemCoordinator()
            {
                Init();                
            }

            private void Init()
            {
                PrinterArm = new PrinterArm();

                CommandHandler0.RegisterCommand("TOGGLE_ARM_CTRL", (args) => ToggleArmControl());
                CommandHandler0.RegisterCommand("TOGGLE_REMOTE_CTRL", (args) => ToggleRemoteControl());
                CommandHandler0.RegisterCommand("CYCLE_ARM_CTRL_MODE", (args) => CycleArmControlMode());
                CommandHandler0.RegisterCommand("CYCLE_ATTACHMENT", (args) => CycleAttachment());
                CommandHandler0.RegisterCommand("CYCLE_TRANSLATION_MODE", (args) => CycleTranslationMode());
            }

            public void Run(double time)
            {
                if (Time == 0)
                {
                    Time = time;
                    return;
                }
                PrinterArm.Run(time);
                Time = time;
            }

            public void ToggleArmControl()
            {
                PrinterArm.ToggleArmControl();
            }

            public void ToggleRemoteControl()
            {
                PrinterArm.ToggleRemoteControl();
            }

            public void CycleArmControlMode()
            {
                PrinterArm.CycleArmControlMode();
            }

            public void CycleAttachment()
            {
                PrinterArm.CycleAttachment();
            }

            public void CycleTranslationMode()
            {
                PrinterArm.CycleTranslationMode();
            }
        }
    }
}
