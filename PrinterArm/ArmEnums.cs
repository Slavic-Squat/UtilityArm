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
        public enum ArmControlMode
        {
            Translate,
            Rotate,
            Pose,
            Undefined
        }

        public static class ArmEnumsHelper
        {
            private static readonly ArmControlMode[] _armControlModeCycles = new ArmControlMode[] { ArmControlMode.Translate, ArmControlMode.Rotate, ArmControlMode.Pose };

            public static ArmControlMode NextArmControlMode(ArmControlMode mode)
            {
                int index = Array.IndexOf(_armControlModeCycles, mode);
                if (index < 0) return _armControlModeCycles[0];
                index = (index + 1) % _armControlModeCycles.Length;
                return _armControlModeCycles[index];
            }

            public static string GetArmControlModeStr(ArmControlMode mode)
            {
                switch (mode)
                {
                    case ArmControlMode.Translate: return "TRANS";
                    case ArmControlMode.Rotate: return "ROT";
                    case ArmControlMode.Pose: return "POSE";
                    default: return "N/A";
                }
            }

            public static ArmControlMode GetArmControlMode(string modeStr)
            {
                switch (modeStr.ToUpper())
                {
                    case "TRANS": return ArmControlMode.Translate;
                    case "ROT": return ArmControlMode.Rotate;
                    case "POSE": return ArmControlMode.Pose;
                    default: return ArmControlMode.Undefined;
                }
            }
        }
    }
}
