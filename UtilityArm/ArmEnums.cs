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
            Null,
            Joint,
            Undefined
        }

        public enum TranslationMode
        {
            Local,
            World
        }

        public enum ArmAttachment
        {
            Empty,
            Welder,
            Grinder,
            Drill,
            Connector,
            Magnet,
            Undefined
        }

        public static class ArmEnumsHelper
        {
            private static readonly ArmAttachment[] _armAttachmentCycles = new ArmAttachment[] { ArmAttachment.Welder, ArmAttachment.Grinder, ArmAttachment.Drill, ArmAttachment.Connector, ArmAttachment.Magnet };

            private static readonly ArmControlMode[] _armControlModeCycles = new ArmControlMode[] { ArmControlMode.Translate, ArmControlMode.Rotate, ArmControlMode.Null, ArmControlMode.Joint };

            private static readonly TranslationMode[] _translationModeCycles = new TranslationMode[] { TranslationMode.Local, TranslationMode.World };

            public static ArmControlMode NextArmControlMode(ArmControlMode mode)
            {
                int index = Array.IndexOf(_armControlModeCycles, mode);
                if (index < 0) return _armControlModeCycles[0];
                index = (index + 1) % _armControlModeCycles.Length;
                return _armControlModeCycles[index];
            }

            public static ArmAttachment NextArmAttachment(ArmAttachment attachment)
            {
                int index = Array.IndexOf(_armAttachmentCycles, attachment);
                if (index < 0) return _armAttachmentCycles[0];
                index = (index + 1) % _armAttachmentCycles.Length;
                return _armAttachmentCycles[index];
            }

            public static TranslationMode NextTranslationMode(TranslationMode mode)
            {
                int index = Array.IndexOf(_translationModeCycles, mode);
                if (index < 0) return _translationModeCycles[0];
                index = (index + 1) % _translationModeCycles.Length;
                return _translationModeCycles[index];
            }

            public static string GetArmControlModeStr(ArmControlMode mode)
            {
                switch (mode)
                {
                    case ArmControlMode.Translate: return "TRANS";
                    case ArmControlMode.Rotate: return "ROT";
                    case ArmControlMode.Null: return "NULL";
                    case ArmControlMode.Joint: return "JOINT";
                    default: return "N/A";
                }
            }

            public static string GetAttachmentStr(ArmAttachment attachment)
            {
                switch (attachment)
                {
                    case ArmAttachment.Empty: return "EMPTY";
                    case ArmAttachment.Welder: return "WELDER";
                    case ArmAttachment.Grinder: return "GRINDER";
                    case ArmAttachment.Drill: return "DRILL";
                    case ArmAttachment.Connector: return "CONNECTOR";
                    case ArmAttachment.Magnet: return "MAGNET";
                    default: return "N/A";
                }
            }

            public static string GetTranslationModeStr(TranslationMode mode)
            {
                switch (mode)
                {
                    case TranslationMode.Local: return "LOCAL";
                    case TranslationMode.World: return "WORLD";
                    default: return "N/A";
                }
            }

            public static ArmControlMode GetArmControlMode(string modeStr)
            {
                switch (modeStr.ToUpper())
                {
                    case "TRANS": return ArmControlMode.Translate;
                    case "ROT": return ArmControlMode.Rotate;
                    case "NULL": return ArmControlMode.Null;
                    case "JOINT": return ArmControlMode.Joint;
                    default: return ArmControlMode.Undefined;
                }
            }

            public static ArmAttachment GetArmAttachment(string attachmentStr)
            {
                switch (attachmentStr.ToUpper())
                {
                    case "EMPTY": return ArmAttachment.Empty;
                    case "WELDER": return ArmAttachment.Welder;
                    case "GRINDER": return ArmAttachment.Grinder;
                    case "DRILL": return ArmAttachment.Drill;
                    case "CONNECTOR": return ArmAttachment.Connector;
                    case "MAGNET": return ArmAttachment.Magnet;
                    default: return ArmAttachment.Undefined;
                }
            }

            public static TranslationMode GetTranslationMode(string modeStr)
            {
                switch (modeStr.ToUpper())
                {
                    case "LOCAL": return TranslationMode.Local;
                    case "WORLD": return TranslationMode.World;
                    default: return TranslationMode.Local;
                }
            }
        }
    }
}
