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
        public class ArmControl
        {
            private PistonSeries _basePistons;
            private Vector3 _baseVector;
            private Rotor _joint0;
            private Vector3 _seg0Vector;
            private Rotor _joint1;
            private Vector3 _seg1Vector;
            private PistonSeries _joint2;
            private Vector3 _seg2Vector;
            private Rotor _joint3;
            private Vector3 _seg3Vector;
            private PistonSeries _joint4;
            private Vector3 _seg4Vector;
            private Rotor _joint5;
            private Vector3 _seg5Vector;
            private Rotor _joint6;
            private Vector3 _seg6Vector;
            private Rotor _joint7;
            private Vector3 _seg7Vector;

            private Vector3 _seg6VectorDetached = new Vector3(0, 0, -1.3f);
            private Vector3 _seg6VectorAttached = new Vector3(0, 0, -1.7f);

            private Vector3 _seg7VectorWelder = new Vector3(0, 0, -3.1f);
            private Vector3 _seg7VectorGrinder = new Vector3(0, 0, -3.1f);
            private Vector3 _seg7VectorDrill = new Vector3(0, 0, -3.1f);
            private Vector3 _seg7VectorConnector = new Vector3(0, 0, -1.0f);
            private Vector3 _seg7VectorMagnet = new Vector3(0, 0, -1.5f);
            private Vector3 _seg7VectorEmpty = Vector3.Zero;

            public ArmControlMode ControlMode { get; private set; } = ArmControlMode.Translate;
            public TranslationMode TranslationMode { get; private set; } = TranslationMode.World;
            public ArmAttachment Attachment { get; private set; } = ArmAttachment.Empty;
            public bool HasAttachment => Attachment != ArmAttachment.Empty;
            public Vector3 EEPosition { get; private set; }
            public string ID { get; private set; }

            public ArmControl(string id)
            {
                ID = id.ToUpper();
                Piston basePiston0 = new Piston($"{ID} BASE PISTON 0");
                Piston basePiston1 = new Piston($"{ID} BASE PISTON 1");
                Piston basePiston2 = new Piston($"{ID} BASE PISTON 2");
                Piston basePiston3 = new Piston($"{ID} BASE PISTON 3");
                Piston basePiston4 = new Piston($"{ID} BASE PISTON 4");
                Piston basePiston5 = new Piston($"{ID} BASE PISTON 5");
                Piston basePiston6 = new Piston($"{ID} BASE PISTON 6");
                Piston basePiston7 = new Piston($"{ID} BASE PISTON 7");
                _basePistons = new PistonSeries(basePiston0, basePiston1, basePiston2, basePiston3, basePiston4, basePiston5, basePiston6, basePiston7);
                _joint0 = new Rotor($"{ID} ARM JOINT 0");
                _joint1 = new Rotor($"{ID} ARM JOINT 1");
                Piston joint2_0 = new Piston($"{ID} ARM JOINT 2_0");
                Piston joint2_1 = new Piston($"{ID} ARM JOINT 2_1");
                _joint2 = new PistonSeries(joint2_0, joint2_1);
                _joint3 = new Rotor($"{ID} ARM JOINT 3");
                Piston joint4_0 = new Piston($"{ID} ARM JOINT 4_0");
                Piston joint4_1 = new Piston($"{ID} ARM JOINT 4_1");
                _joint4 = new PistonSeries(joint4_0, joint4_1);
                _joint5 = new Rotor($"{ID} ARM JOINT 5");
                _joint6 = new Rotor($"{ID} ARM JOINT 6");
                _joint7 = new Rotor($"{ID} ARM JOINT 7");

                _baseVector = new Vector3(0, 1.45f, 0);
                _seg0Vector = new Vector3(4.0f, 1.25f, 0);
                _seg1Vector = new Vector3(1.25f, 0, -11.6f);
                _seg2Vector = new Vector3(-4.0f, 0, -1.25f);
                _seg3Vector = new Vector3(-1.25f, 0, -11.6f);
                _seg4Vector = new Vector3(0, 0, -1.25f);
                _seg5Vector = new Vector3(0, 0, -1.5f);
            }

            public void Control(UserInput userInput)
            {
                if (!_joint7.RotorBlock.IsAttached)
                {
                    Attachment = ArmAttachment.Empty;
                }

                _seg6Vector = HasAttachment ? _seg6VectorAttached : _seg6VectorDetached;

                switch (Attachment)
                {
                    case ArmAttachment.Welder:
                        _seg7Vector = _seg7VectorWelder;
                        break;
                    case ArmAttachment.Grinder:
                        _seg7Vector = _seg7VectorGrinder;
                        break;
                    case ArmAttachment.Drill:
                        _seg7Vector = _seg7VectorDrill;
                        break;
                    case ArmAttachment.Connector:
                        _seg7Vector = _seg7VectorConnector;
                        break;
                    case ArmAttachment.Magnet:
                        _seg7Vector = _seg7VectorMagnet;
                        break;
                    case ArmAttachment.Empty:
                        _seg7Vector = _seg7VectorEmpty;
                        break;
                }

                Matrix H0 = Matrix.CreateRotationY(_joint0.AngleRad);
                H0.Translation = _baseVector + _joint0.RotorBlock.Displacement * H0.Up;
                Matrix H1 = Matrix.CreateRotationX(_joint1.AngleRad);
                H1.Translation = _seg0Vector + _joint1.RotorBlock.Displacement * H1.Right;
                Matrix H2 = Matrix.Identity;
                H2.Translation = _seg1Vector + _joint2.Extension * H2.Forward;
                Matrix H3 = Matrix.CreateRotationX(_joint3.AngleRad);
                H3.Translation = _seg2Vector + _joint3.RotorBlock.Displacement * H3.Left;
                Matrix H4 = Matrix.Identity;
                H4.Translation = _seg3Vector + _joint4.Extension * H4.Forward;
                Matrix H5 = Matrix.CreateRotationY(_joint5.AngleRad);
                H5.Translation = _seg4Vector;
                Matrix H6 = Matrix.CreateRotationX(_joint6.AngleRad);
                H6.Translation = _seg5Vector;
                Matrix H7 = HasAttachment ? Matrix.CreateRotationZ(_joint7.AngleRad) : Matrix.Identity;
                H7.Translation = _seg6Vector + _joint7.RotorBlock.Displacement * H7.Forward;
                Matrix H8 = Matrix.Identity;
                H8.Translation = _seg7Vector;

                Matrix HT = H8 * H7 * H6 * H5 * H4 * H3 * H2 * H1 * H0;
                Vector3 currentCoord = HT.Translation;

                EEPosition = currentCoord;

                Matrix H0_1 = H1 * H0;
                Matrix H0_2 = H2 * H0_1;
                Matrix H0_3 = H3 * H0_2;
                Matrix H0_4 = H4 * H0_3;
                Matrix H0_5 = H5 * H0_4;
                Matrix H0_6 = H6 * H0_5;
                Matrix H0_7 = H7 * H0_6;

                if (ControlMode == ArmControlMode.Joint)
                {
                    if (userInput.APress)
                    {
                        _joint0.VelocityRad = 0.25f;
                    }
                    else if (userInput.DPress)
                    {
                        _joint0.VelocityRad = -0.25f;
                    }
                    else
                    {
                        _joint0.VelocityRad = 0f;
                    }

                    if (userInput.WPress)
                    {
                        _joint3.VelocityRad = 0.25f;
                    }
                    else if (userInput.SPress)
                    {
                        _joint3.VelocityRad = -0.25f;
                    }
                    else
                    {
                        _joint3.VelocityRad = 0f;
                    }

                    if (userInput.SpacePress)
                    {
                        _joint1.VelocityRad = 0.25f;
                    }
                    else if (userInput.CPress)
                    {
                        _joint1.VelocityRad = -0.25f;
                    }
                    else
                    {
                        _joint1.VelocityRad = 0f;
                    }

                    if (userInput.QPress)
                    {
                        _joint2.Velocity = -1f;
                        _joint4.Velocity = -1f;
                    }
                    else if (userInput.EPress)
                    {
                        _joint2.Velocity = 1f;
                        _joint4.Velocity = 1f;
                    }
                    else
                    {
                        _joint2.Velocity = 0f;
                        _joint4.Velocity = 0f;
                    }

                    return;
                }

                Vector3 J0v = Vector3.Cross(H0.Up, currentCoord - H0.Translation);
                Vector3 J0w = Vector3.TransformNormal(H0.Up, Matrix.Transpose(HT.GetOrientation()));
                MyVector J0 = new MyVector(6) { V0 = J0v.X, V1 = J0v.Y, V2 = J0v.Z, V3 = J0w.X, V4 = J0w.Y, V5 = J0w.Z };

                Vector3 J1v = Vector3.Cross(H0_1.Right, currentCoord - H0_1.Translation);
                Vector3 J1w = Vector3.TransformNormal(H0_1.Right, Matrix.Transpose(HT.GetOrientation()));
                MyVector J1 = new MyVector(6) { V0 = J1v.X, V1 = J1v.Y, V2 = J1v.Z, V3 = J1w.X, V4 = J1w.Y, V5 = J1w.Z };

                Vector3 J3v = Vector3.Cross(H0_3.Right, currentCoord - H0_3.Translation);
                Vector3 J3w = Vector3.TransformNormal(H0_3.Right, Matrix.Transpose(HT.GetOrientation()));
                MyVector J3 = new MyVector(6) { V0 = J3v.X, V1 = J3v.Y, V2 = J3v.Z, V3 = J3w.X, V4 = J3w.Y, V5 = J3w.Z };

                Vector3 J5v = Vector3.Cross(H0_5.Up, currentCoord - H0_5.Translation);
                Vector3 J5w = Vector3.TransformNormal(H0_5.Up, Matrix.Transpose(HT.GetOrientation()));
                MyVector J5 = new MyVector(6) { V0 = J5v.X, V1 = J5v.Y, V2 = J5v.Z, V3 = J5w.X, V4 = J5w.Y, V5 = J5w.Z };

                Vector3 J6v = Vector3.Cross(H0_6.Right, currentCoord - H0_6.Translation);
                Vector3 J6w = Vector3.TransformNormal(H0_6.Right, Matrix.Transpose(HT.GetOrientation()));
                MyVector J6 = new MyVector(6) { V0 = J6v.X, V1 = J6v.Y, V2 = J6v.Z, V3 = J6w.X, V4 = J6w.Y, V5 = J6w.Z };

                Vector3 J7v = Vector3.Cross(H0_7.Backward, currentCoord - H0_7.Translation);
                Vector3 J7w = Vector3.TransformNormal(H0_7.Backward, Matrix.Transpose(HT.GetOrientation()));
                MyVector J7 = new MyVector(6) { V0 = J7v.X, V1 = J7v.Y, V2 = J7v.Z, V3 = J7w.X, V4 = J7w.Y, V5 = J7w.Z };

                MyMatrix J = new MyMatrix(6, 6)
                {
                    M00 = J0[0],
                    M01 = J1[0],
                    M02 = J3[0],
                    M03 = J5[0],
                    M04 = J6[0],
                    M05 = J7[0],
                    M10 = J0[1],
                    M11 = J1[1],
                    M12 = J3[1],
                    M13 = J5[1],
                    M14 = J6[1],
                    M15 = J7[1],
                    M20 = J0[2],
                    M21 = J1[2],
                    M22 = J3[2],
                    M23 = J5[2],
                    M24 = J6[2],
                    M25 = J7[2],
                    M30 = J0[3],
                    M31 = J1[3],
                    M32 = J3[3],
                    M33 = J5[3],
                    M34 = J6[3],
                    M35 = J7[3],
                    M40 = J0[4],
                    M41 = J1[4],
                    M42 = J3[4],
                    M43 = J5[4],
                    M44 = J6[4],
                    M45 = J7[4],
                    M50 = J0[5],
                    M51 = J1[5],
                    M52 = J3[5],
                    M53 = J5[5],
                    M54 = J6[5],
                    M55 = J7[5]
                };

                if (!HasAttachment)
                {
                    MyVector zeroVec = MyVector.Zero(6);
                    MyMathExt.SetRow(ref J, 5, ref zeroVec);
                    MyMathExt.SetColumn(ref J, 5, ref zeroVec);
                }

                MyVector inputSignal = new MyVector(6);

                Vector3 trans0 = Vector3.Zero;
                Vector3 trans1 = Vector3.Zero;
                Vector3 trans2 = Vector3.Zero;

                Vector3 rot0 = Vector3.Zero;
                Vector3 rot1 = Vector3.Zero;
                Vector3 rot2 = Vector3.Zero;

                TranslationMode = HasAttachment ? TranslationMode : TranslationMode.World;

                Vector3 transXDir;
                Vector3 transYDir;
                Vector3 transZDir;

                Vector3 rotXDir = Vector3.Right;
                Vector3 rotYDir = Vector3.Up;
                Vector3 rotZDir = Vector3.Backward;

                switch (TranslationMode)
                {
                    case TranslationMode.World:
                        transXDir = Vector3.Right;
                        transYDir = Vector3.Up;
                        transZDir = Vector3.Backward;
                        break;
                    case TranslationMode.Local:
                        transXDir = HT.Right;
                        transYDir = HT.Up;
                        transZDir = HT.Backward;
                        break;
                    default:
                        transXDir = Vector3.Right;
                        transYDir = Vector3.Up;
                        transZDir = Vector3.Backward;
                        break;
                }

                switch (ControlMode)
                {
                    case ArmControlMode.Translate:
                        {
                            if (userInput.WPress) trans0 = -1f * transZDir;
                            else if (userInput.SPress) trans0 = 1f * transZDir;

                            if (userInput.APress) trans1 = -1f * transXDir;
                            else if (userInput.DPress) trans1 = 1f * transXDir;

                            if (userInput.SpacePress) trans2 = 1f * transYDir;
                            else if (userInput.CPress) trans2 = -1f * transYDir;
                            break;
                        }
                    case ArmControlMode.Rotate:
                        {
                            if (userInput.SpacePress) rot1 = 0.2f * rotXDir;
                            else if (userInput.CPress) rot1 = -0.2f * rotXDir;

                            if (userInput.APress) rot0 = 0.2f * rotYDir;
                            else if (userInput.DPress) rot0 = -0.2f * rotYDir;

                            if (userInput.QPress) rot2 = 0.2f * rotZDir;
                            else if (userInput.EPress) rot2 = -0.2f * rotZDir;
                            break;
                        }
                }

                Vector3 transInput = trans0 + trans1 + trans2;
                Vector3 rotInput = rot0 + rot1 + rot2;

                inputSignal[0] = transInput.X;
                inputSignal[1] = transInput.Y;
                inputSignal[2] = transInput.Z;
                inputSignal[3] = rotInput.X;
                inputSignal[4] = rotInput.Y;
                inputSignal[5] = rotInput.Z;

                if (!HasAttachment)
                {
                    inputSignal[5] = 0f;
                }

                MyMatrix J_pi;
                MyMathExt.DampedPseudoInverseWide(ref J, out J_pi, 0.1f);
                MyVector outputSignal;
                MyMathExt.Multiply(ref J_pi, ref inputSignal, out outputSignal);
                bool oob = false;

                _joint0.VelocityRad = (float)outputSignal[0];
                if (_joint0.IsSaturated)
                {
                    oob = true;
                }
                _joint1.VelocityRad = (float)outputSignal[1];
                if (_joint1.IsSaturated)
                {
                    oob = true;
                }
                _joint3.VelocityRad = (float)outputSignal[2];
                if (_joint3.IsSaturated)
                {
                    oob = true;
                }
                _joint5.VelocityRad = (float)outputSignal[3];
                if (_joint5.IsSaturated)
                {
                    oob = true;
                }
                _joint6.VelocityRad = (float)outputSignal[4];
                if (_joint6.IsSaturated)
                {
                    oob = true;
                }
                _joint7.VelocityRad = (float)outputSignal[5];
                if (_joint7.IsSaturated)
                {
                    oob = true;
                }

                if (oob)
                {
                    _joint0.VelocityRad = 0f;
                    _joint1.VelocityRad = 0f;
                    _joint3.VelocityRad = 0f;
                    _joint5.VelocityRad = 0f;
                    _joint6.VelocityRad = 0f;
                    _joint7.VelocityRad = 0f;
                }
            }

            public void CycleControlMode()
            {
                ControlMode = ArmEnumsHelper.NextArmControlMode(ControlMode);
            }

            public void CycleAttachment()
            {
                Attachment = ArmEnumsHelper.NextArmAttachment(Attachment);
            }

            public void CycleTranslationMode()
            {
                TranslationMode = ArmEnumsHelper.NextTranslationMode(TranslationMode);
            }
        }
    }
}
