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
            private Vector3 _seg7VectorUndefined = Vector3.Zero;
            private Vector3 _seg7VectorEmpty = Vector3.Zero;

            public ArmControlMode ControlMode { get; private set; } = ArmControlMode.Translate;
            public TranslationMode TranslationMode { get; private set; } = TranslationMode.World;
            public ArmAttachment Attachment { get; private set; } = ArmAttachment.Undefined;
            public bool HasAttachment => Attachment != ArmAttachment.Empty;
            public Vector3 EEPosition { get; private set; }
            public string ID { get; private set; }

            public ArmControl(string id)
            {
                ID = id.ToUpper();
                Piston basePiston0 = new Piston($"{ID} BASE PISTON 0");
                Piston basePiston1 = new Piston($"{ID} BASE PISTON 1");
                _basePistons = new PistonSeries(basePiston0, basePiston1);
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
                else
                {
                    if (Attachment == ArmAttachment.Empty)
                    {
                        Attachment = ArmAttachment.Undefined;
                    }
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
                    case ArmAttachment.Undefined:
                        _seg7Vector = _seg7VectorUndefined;
                        break;
                    case ArmAttachment.Empty:
                        _seg7Vector = _seg7VectorEmpty;
                        break;
                    default:
                        _seg7Vector = Vector3.Zero;
                        break;
                }

                Matrix H0 = Matrix.CreateRotationY(_joint0.CurrentAngle);
                H0.Translation = _baseVector;
                Matrix H1 = Matrix.CreateRotationX(_joint1.CurrentAngle);
                H1.Translation = _seg0Vector;
                Matrix H2 = Matrix.Identity;
                H2.Translation = _seg1Vector;
                Matrix H3 = Matrix.CreateRotationX(_joint3.CurrentAngle);
                H3.Translation = _seg2Vector + -1f * _joint2.CurrentExtension * H2.Backward;
                Matrix H4 = Matrix.Identity;
                H4.Translation = _seg3Vector;
                Matrix H5 = Matrix.CreateRotationY(_joint5.CurrentAngle);
                H5.Translation = _seg4Vector + -1f * _joint4.CurrentExtension * H4.Backward;
                Matrix H6 = Matrix.CreateRotationX(_joint6.CurrentAngle);
                H6.Translation = _seg5Vector;
                Matrix H7 = HasAttachment ? Matrix.CreateRotationZ(_joint7.CurrentAngle) : Matrix.Identity;
                H7.Translation = _seg6Vector;
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

                Vector3 J0v = Vector3.Cross(H0.Up, currentCoord - H0.Translation);
                //float J0w = Vector3.Dot(H0.Up, H0_5.Right);
                Vector3 J0w = Vector3.TransformNormal(H0.Up, Matrix.Transpose(HT.GetOrientation()));
                double[] J0 = new double[6] { J0v.X, J0v.Y, J0v.Z, J0w.X, J0w.Y, J0w.Z };

                Vector3 J1v = Vector3.Cross(H0_1.Right, currentCoord - H0_1.Translation);
                //float J1w = Vector3.Dot(H0_1.Right, H0_5.Right);
                Vector3 J1w = Vector3.TransformNormal(H0_1.Right, Matrix.Transpose(HT.GetOrientation()));
                double[] J1 = new double[6] { J1v.X, J1v.Y, J1v.Z, J1w.X, J1w.Y, J1w.Z };

                Vector3 J2v = -1f * H0_2.Backward;
                //float J2w = 0f;
                Vector3 J2w = Vector3.Zero;
                double[] J2 = new double[6] { J2v.X, J2v.Y, J2v.Z, J2w.X, J2w.Y, J2w.Z };

                Vector3 J3v = Vector3.Cross(H0_3.Right, currentCoord - H0_3.Translation);
                //float J3w = Vector3.Dot(H0_3.Right, H0_5.Right);
                Vector3 J3w = Vector3.TransformNormal(H0_3.Right, Matrix.Transpose(HT.GetOrientation()));
                double[] J3 = new double[6] { J3v.X, J3v.Y, J3v.Z, J3w.X, J3w.Y, J3w.Z };

                Vector3 J4v = -1f * H0_4.Backward;
                //float J4w = 0f;
                Vector3 J4w = Vector3.Zero;
                double[] J4 = new double[6] { J4v.X, J4v.Y, J4v.Z, J4w.X, J4w.Y, J4w.Z };

                Vector3 J5v = Vector3.Cross(H0_5.Up, currentCoord - H0_5.Translation);
                //float J5w = 1f;
                Vector3 J5w = Vector3.TransformNormal(H0_5.Up, Matrix.Transpose(HT.GetOrientation()));
                double[] J5 = new double[6] { J5v.X, J5v.Y, J5v.Z, J5w.X, J5w.Y, J5w.Z };

                Vector3 J6v = Vector3.Cross(H0_6.Right, currentCoord - H0_6.Translation);
                Vector3 J6w = Vector3.TransformNormal(H0_6.Right, Matrix.Transpose(HT.GetOrientation()));
                double[] J6 = new double[6] { J6v.X, J6v.Y, J6v.Z, J6w.X, J6w.Y, J6w.Z };

                Vector3 J7v = Vector3.Cross(H0_7.Backward, currentCoord - H0_7.Translation);
                Vector3 J7w = Vector3.TransformNormal(H0_7.Backward, Matrix.Transpose(HT.GetOrientation()));
                double[] J7 = new double[6] { J7v.X, J7v.Y, J7v.Z, J7w.X, J7w.Y, J7w.Z };

                double[,] J = new double[6, 8]
                {
                    { J0[0], J1[0], J2[0], J3[0], J4[0], J5[0], J6[0], J7[0] },
                    { J0[1], J1[1], J2[1], J3[1], J4[1], J5[1], J6[1], J7[1] },
                    { J0[2], J1[2], J2[2], J3[2], J4[2], J5[2], J6[2], J7[2] },
                    { J0[3], J1[3], J2[3], J3[3], J4[3], J5[3], J6[3], J7[3] },
                    { J0[4], J1[4], J2[4], J3[4], J4[4], J5[4], J6[4], J7[4] },
                    { J0[5], J1[5], J2[5], J3[5], J4[5], J5[5], J6[5], J7[5] }
                };

                if (!HasAttachment)
                {
                    MyMath.SetRow(J, 5, new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
                    MyMath.SetColumn(J, 7, new double[6] { 0, 0, 0, 0, 0, 0 });
                }

                double[] taskWeights = new double[6] { 1, 1, 1, 10, 10, 10 };
                double[] jointWeights = new double[8] { 1, 1, 1, 1, 1, 1, 1, 1 };

                double[] inputSignal = new double[6];
                double[] inputSignalNull = new double[8];

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
                    case ArmControlMode.Pose:
                        {
                            if (userInput.QPress)
                            {
                                inputSignalNull[2] = -1f;
                                inputSignalNull[4] = -1f;
                                jointWeights[2] = double.MaxValue;
                                jointWeights[4] = double.MaxValue;

                            }
                            else if (userInput.EPress)
                            {
                                inputSignalNull[2] = 1f;
                                inputSignalNull[4] = 1f;
                                jointWeights[2] = double.MaxValue;
                                jointWeights[4] = double.MaxValue;
                            }

                            if (userInput.SpacePress)
                            {
                                inputSignalNull[1] = 0.2f;
                                jointWeights[1] = double.MaxValue;
                            }
                            else if (userInput.CPress)
                            {
                                inputSignalNull[1] = -0.2f;
                                jointWeights[1] = double.MaxValue;
                            }

                            if (userInput.APress)
                            {
                                _basePistons.Velocity = 1f;
                            }
                            else if (userInput.DPress)
                            {
                                _basePistons.Velocity = -1f;
                            }
                            else
                            {
                                _basePistons.Velocity = 0f;
                            }

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
                    inputSignalNull[7] = 0f;
                }

                double[,] J_pseudoInv = MyMath.DampedWeightedPseudoInverseWide(J, taskWeights, jointWeights, 0.1f);
                double[] outputSignal = MyMath.MultiplyMatrixVector(J_pseudoInv, inputSignal);
                double[,] N = MyMath.NullSpaceProjector(J, J_pseudoInv);
                double[] outputSignalNull = MyMath.MultiplyMatrixVector(N, inputSignalNull);
                double[] totalOutputSignal = MyMath.AddVectors(outputSignal, outputSignalNull);
                bool oob = false;

                _joint0.Velocity = (float)totalOutputSignal[0];
                if (_joint0.IsSaturated)
                {
                    jointWeights[0] = double.MaxValue;
                    oob = true;
                }
                _joint1.Velocity = (float)totalOutputSignal[1];
                if (_joint1.IsSaturated)
                {
                    jointWeights[1] = double.MaxValue;
                    oob = true;
                }
                _joint2.Velocity = (float)totalOutputSignal[2];
                if (_joint2.IsSaturated)
                {
                    jointWeights[2] = double.MaxValue;
                    oob = true;
                }
                _joint3.Velocity = (float)totalOutputSignal[3];
                if (_joint3.IsSaturated)
                {
                    jointWeights[3] = double.MaxValue;
                    oob = true;
                }
                _joint4.Velocity = (float)totalOutputSignal[4];
                if (_joint4.IsSaturated)
                {
                    jointWeights[4] = double.MaxValue;
                    oob = true;
                }
                _joint5.Velocity = (float)totalOutputSignal[5];
                if (_joint5.IsSaturated)
                {
                    jointWeights[5] = double.MaxValue;
                    oob = true;
                }
                _joint6.Velocity = (float)totalOutputSignal[6];
                if (_joint6.IsSaturated)
                {
                    jointWeights[6] = double.MaxValue;
                    oob = true;
                }
                _joint7.Velocity = (float)totalOutputSignal[7];
                if (_joint7.IsSaturated)
                {
                    jointWeights[7] = double.MaxValue;
                    oob = true;
                }

                if (oob)
                {
                    // recompute with updated joint weights
                    J_pseudoInv = MyMath.DampedWeightedPseudoInverseWide(J, taskWeights, jointWeights, 0.1f);
                    outputSignal = MyMath.MultiplyMatrixVector(J_pseudoInv, inputSignal);
                    N = MyMath.NullSpaceProjector(J, J_pseudoInv);
                    outputSignalNull = MyMath.MultiplyMatrixVector(N, inputSignalNull);
                    totalOutputSignal = MyMath.AddVectors(outputSignal, outputSignalNull);

                    _joint0.Velocity = (float)totalOutputSignal[0];
                    if (_joint0.IsSaturated)
                    {
                        totalOutputSignal[0] = 0f;
                        _joint0.Velocity = 0f;
                    }
                    _joint1.Velocity = (float)totalOutputSignal[1];
                    if (_joint1.IsSaturated)
                    {
                        totalOutputSignal[1] = 0f;
                        _joint1.Velocity = 0f;
                    }
                    _joint2.Velocity = (float)totalOutputSignal[2];
                    if (_joint2.IsSaturated)
                    {
                        totalOutputSignal[2] = 0f;
                        _joint2.Velocity = 0f;
                    }
                    _joint3.Velocity = (float)totalOutputSignal[3];
                    if (_joint3.IsSaturated)
                    {
                        totalOutputSignal[3] = 0f;
                        _joint3.Velocity = 0f;
                    }
                    _joint4.Velocity = (float)totalOutputSignal[4];
                    if (_joint4.IsSaturated)
                    {
                        totalOutputSignal[4] = 0f;
                        _joint4.Velocity = 0f;
                    }
                    _joint5.Velocity = (float)totalOutputSignal[5];
                    if (_joint5.IsSaturated)
                    {
                        totalOutputSignal[5] = 0f;
                        _joint5.Velocity = 0f;
                    }
                    _joint6.Velocity = (float)totalOutputSignal[6];
                    if (_joint6.IsSaturated)
                    {
                        totalOutputSignal[6] = 0f;
                        _joint6.Velocity = 0f;
                    }
                    _joint7.Velocity = (float)totalOutputSignal[7];
                    if (_joint7.IsSaturated)
                    {
                        totalOutputSignal[7] = 0f;
                        _joint7.Velocity = 0f;
                    }
                    oob = false;
                }

                double[] achievableVelocities = MyMath.MultiplyMatrixVector(J, totalOutputSignal);
                double[] errors = MyMath.SubtractVectors(inputSignal, achievableVelocities);
                double[] tolerances = new double[6] { 1, 1, 1, 0.1, 0.1, 0.1 };

                for (int i = 0; i < 6; i++)
                {
                    double absError = Math.Abs(errors[i]);

                    if (absError > tolerances[i])
                    {
                        oob = true;
                        break;
                    }
                }

                if (oob)
                {
                    _joint0.Velocity = 0f;
                    _joint1.Velocity = 0f;
                    _joint2.Velocity = 0f;
                    _joint3.Velocity = 0f;
                    _joint4.Velocity = 0f;
                    _joint5.Velocity = 0f;
                    _joint6.Velocity = 0f;
                    _joint7.Velocity = 0f;
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
