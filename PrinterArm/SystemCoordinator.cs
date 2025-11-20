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

            private Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>();

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
                CommandHandler = new CommandHandler(MePB, _commands);
                PrinterArm = new PrinterArm();

                _commands["TOGGLE_ARM_CTRL"] = (args) => ToggleArmControl();
                _commands["TOGGLE_REMOTE_CTRL"] = (args) => ToggleRemoteControl();
                _commands["TOGGLE_O_CTRL"] = (args) => ToggleOrientationControl();
            }

            public void Run()
            {
                SystemTime += RuntimeInfo.TimeSinceLastRun.TotalSeconds;
                DebugEcho(SystemTime.ToString("F2"));
                PrinterArm.Run(SystemTime);
            }

            public bool Command(string command)
            {
                return CommandHandler.RunCommands(command);
            }

            public bool ToggleArmControl()
            {
                return PrinterArm.ToggleArmControl();
            }

            public bool ToggleRemoteControl()
            {
                return PrinterArm.ToggleRemoteControl();
            }

            public bool ToggleOrientationControl()
            {
                return PrinterArm.ToggleOrientationControl();
            }
        }
    }
}
