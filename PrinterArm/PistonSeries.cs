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
        public class PistonSeries
        {
            private List<Piston> _pistons = new List<Piston>();

            public float CurrentExtension => _pistons.Sum(p => p.CurrentExtension);
            public float MinExtension => _pistons.Sum(p => p.MinExtension);
            public float MaxExtension => _pistons.Sum(p => p.MaxExtension);
            public float Range => MaxExtension - MinExtension;
            public float Velocity
            {
                get
                {
                    return _pistons.Sum(p => p.Velocity);
                }
                set
                {
                    _targetVelocity = value;
                    if (value < 0)
                    {
                        float totalRemaining = CurrentExtension - MinExtension;
                        if (totalRemaining == 0)
                        {
                            _pistons.ForEach(p => p.Velocity = 0);
                            return;
                        }
                        _pistons.ForEach(p => p.Velocity = (p.CurrentExtension - p.MinExtension) / totalRemaining * value);
                    }
                    else if (value > 0)
                    {
                        float totalRemaining = MaxExtension - CurrentExtension;
                        if (totalRemaining == 0)
                        {
                            _pistons.ForEach(p => p.Velocity = 0);
                            return;
                        }
                        _pistons.ForEach(p => p.Velocity = (p.MaxExtension - p.CurrentExtension) / totalRemaining * value);
                    }
                    else
                    {
                        _pistons.ForEach(p => p.Velocity = 0);
                    }
                }
            }

            public bool IsMaxed => _pistons.All(p => p.IsMaxed);
            public bool IsMinned => _pistons.All(p => p.IsMinned);
            public bool IsSaturated
            {
                get
                {
                    if (_targetVelocity < 0)
                    {
                        return IsMinned;
                    }
                    else if (_targetVelocity > 0)
                    {
                        return IsMaxed;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            private float _targetVelocity;

            public PistonSeries(string groupName)
            {
                GTS.GetBlockGroupWithName(groupName).GetBlocksOfType(_pistons);
                if (_pistons.Count == 0)
                {
                    throw new Exception($"Piston Series {groupName} Not Found");
                }
            }

            public PistonSeries(params Piston[] pistons)
            {
                _pistons = pistons.ToList();
            }
        }
    }
}
