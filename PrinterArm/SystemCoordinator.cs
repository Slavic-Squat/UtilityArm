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
            public static double SystemTime { get; private set; }
            public MyIni Config { get; private set; }
            public CommandHandler CommandHandler { get; private set; }
            public PrinterArm PrinterArm { get; private set; }

            public SystemCoordinator()
            {
                GetBlocks();
                Init();                
            }

            private void GetBlocks()
            {

            }

            private void Init()
            {
                CommandHandler = new CommandHandler();
                PrinterArm = new PrinterArm();

                CommandHandler.RegisterCommand("TOGGLE_ARM_CTRL", (args) => ToggleArmControl());
                CommandHandler.RegisterCommand("TOGGLE_REMOTE_CTRL", (args) => ToggleRemoteControl());
                CommandHandler.RegisterCommand("CYCLE_ARM_CTRL_MODE", (args) => CycleArmControlMode());
            }

            public void Run()
            {
                SystemTime += RuntimeInfo.TimeSinceLastRun.TotalSeconds;
                DebugEcho($"System Time: {SystemTime:F2}s\n");
                DebugWrite($"System Time: {SystemTime:F2}s\n", false);
                DebugEcho($"Last Run Time: {RuntimeInfo.LastRunTimeMs}ms\n");
                DebugWrite($"Last Run Time: {RuntimeInfo.LastRunTimeMs}ms\n", true);
                PrinterArm.Run(SystemTime);
            }

            public void Command(string command)
            {
                CommandHandler.RunCommands(command);
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
        }
    }
}
