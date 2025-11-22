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
        public class Piston
        {
            public float CurrentExtension => PistonBlock.CurrentPosition;
            public float MinExtension
            {
                get
                {
                    return PistonBlock.MinLimit;
                }
                set
                {
                    PistonBlock.MinLimit = value;
                }
            }
            public float MaxExtension
            {
                get
                {
                    return PistonBlock.MaxLimit;
                }
                set
                {
                    PistonBlock.MaxLimit = value;
                }
            }
            public float Range => MaxExtension - MinExtension;
            public float Velocity
            {
                get
                {
                    return PistonBlock.Velocity;
                }
                set
                {
                    PistonBlock.Velocity = value;
                }
            }
            public bool IsMaxed => CurrentExtension >= MaxExtension - 0.1f;
            public bool IsMinned => CurrentExtension <= MinExtension + 0.1f;
            public bool IsSaturated
            {
                get
                {
                    if (Velocity > 0)
                    {
                        return IsMaxed;
                    }
                    else if (Velocity < 0)
                    {
                        return IsMinned;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            public IMyPistonBase PistonBlock { get; private set; }
            public Piston(string blockName)
            {
                PistonBlock = GTS.GetBlockWithName(blockName) as IMyPistonBase;
                if (PistonBlock == null)
                    throw new ArgumentException($"Piston block '{blockName}' not found");
            }

            public Piston(IMyPistonBase pistonBlock)
            {
                if (pistonBlock == null)
                    throw new ArgumentException($"Piston block is null");
                PistonBlock = pistonBlock;
            }
        }
    }
}
