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
            private Vector3 _baseVector;
            private PistonSeries _joint0;
            private Vector3 _seg0Vector;
            private Rotor _joint1;
            private Vector3 _seg1Vector;
            private Rotor _joint2;
            private Vector3 _seg2Vector;
            private Piston _joint3;
            private Vector3 _seg3Vector;
            private Rotor _joint4;
            private Vector3 _seg4Vector;
            private Piston _joint5;
            private Vector3 _seg5Vector;
            private Rotor _joint6;
            private Vector3 _seg6Vector;
            private Rotor _joint7;
            private Vector3 _seg7Vector;
            private Rotor _joint8;
            private Vector3 _seg8Vector;

            public bool OCtrl { get; private set; }
            public Vector3 EEPosition { get; private set; }

            public ArmControl()
            {
                Piston joint0_0 = new Piston("Joint0_0");
                Piston joint0_1 = new Piston("Joint0_1");
                Piston joint0_2 = new Piston("Joint0_2");
                Piston joint0_3 = new Piston("Joint0_3");
                _joint0 = new PistonSeries(joint0_0, joint0_1, joint0_2, joint0_3);
                _joint1 = new Rotor("Joint1");
                _joint2 = new Rotor("Joint2");
                _joint3 = new Piston("Joint3");
                _joint4 = new Rotor("Joint4");
                _joint5 = new Piston("Joint5");
                _joint6 = new Rotor("Joint6");
                _joint7 = new Rotor("Joint7");
                _joint8 = new Rotor("Joint8");

                _baseVector = new Vector3(0, 0, -18.2f);
                _seg0Vector = new Vector3(0, -4.15f, -1.25f);
                _seg1Vector = new Vector3(3.95f, -1.25f, 0);
                _seg2Vector = new Vector3(1.25f, 0, -6.35f);
                _seg3Vector = new Vector3(-3.95f, 0, -1.25f);
                _seg4Vector = new Vector3(-1.25f, 0, -6.4f);
                _seg5Vector = new Vector3(0, 0, -1.25f);
                _seg6Vector = new Vector3(0, 0, -1.5f);
                _seg7Vector = new Vector3(0, 0, -1.7f);
                _seg8Vector = new Vector3(0, 0, -3.5f);
            }

            public bool Control(UserInput input)
            {
                Matrix H0 = Matrix.Identity;
                Matrix H1 = Matrix.CreateRotationY(_joint1.CurrentAngle);
                H1.Translation = _seg0Vector + -1f * _joint0.CurrentExtension * H0.Backward;
                Matrix H2 = Matrix.CreateRotationX(_joint2.CurrentAngle);
                H2.Translation = _seg1Vector;
                Matrix H3 = Matrix.Identity;
                H3.Translation = _seg2Vector;
                Matrix H4 = Matrix.CreateRotationX(_joint4.CurrentAngle);
                H4.Translation = _seg3Vector + -1f * _joint3.CurrentExtension * H3.Backward;
                Matrix H5 = Matrix.Identity;
                H5.Translation = _seg4Vector;
                Matrix H6 = Matrix.CreateRotationY(_joint6.CurrentAngle);
                H6.Translation = _seg5Vector + -1f * _joint5.CurrentExtension * H5.Backward;
                Matrix H7 = Matrix.CreateRotationX(_joint7.CurrentAngle);
                H7.Translation = _seg6Vector;
                Matrix H8 = Matrix.CreateRotationZ(_joint8.CurrentAngle);
                H8.Translation = _seg7Vector;
                Matrix H9 = Matrix.Identity;
                H9.Translation = _seg8Vector;

                Matrix HT = H9 * H8 * H7 * H6 * H5 * H4 * H3 * H2 * H1 * H0;
                Vector3 currentCoord = HT.Translation;
                EEPosition = currentCoord;

                Matrix H0_1 = H1 * H0;
                Matrix H0_2 = H2 * H1 * H0;
                Matrix H0_3 = H3 * H2 * H1 * H0;
                Matrix H0_4 = H4 * H3 * H2 * H1 * H0;
                Matrix H0_5 = H5 * H4 * H3 * H2 * H1 * H0;
                Matrix H0_6 = H6 * H5 * H4 * H3 * H2 * H1 * H0;
                Matrix H0_7 = H7 * H6 * H5 * H4 * H3 * H2 * H1 * H0;
                Matrix H0_8 = H8 * H7 * H6 * H5 * H4 * H3 * H2 * H1 * H0;

                //DebugDraw.DrawMatrix(HT * _joint0.RotorBlock.WorldMatrix, length: 2f);

                Vector3 J0v = -1f * H0.Backward;
                Vector3 J0w = Vector3.Zero;
                double[] J0 = new double[6] { J0v.X, J0v.Y, J0v.Z, J0w.X, J0w.Y, J0w.Z };

                Vector3 J1v = Vector3.Cross(H0_1.Up, currentCoord - H0_1.Translation);
                //Vector3 J1w = H0_1.Up;
                Vector3 J1w = Vector3.TransformNormal(H0_1.Up, Matrix.Transpose(H0_8.GetOrientation()));
                double[] J1 = new double[6] { J1v.X, J1v.Y, J1v.Z, J1w.X, J1w.Y, J1w.Z };

                Vector3 J2v = Vector3.Cross(H0_2.Right, currentCoord - H0_2.Translation);
                //Vector3 J2w = H0_2.Right;
                Vector3 J2w = Vector3.TransformNormal(H0_2.Right, Matrix.Transpose(H0_8.GetOrientation()));
                double[] J2 = new double[6] { J2v.X, J2v.Y, J2v.Z, J2w.X, J2w.Y, J2w.Z };

                Vector3 J3v = -1f * H0_3.Backward;
                Vector3 J3w = Vector3.Zero;
                double[] J3 = new double[6] { J3v.X, J3v.Y, J3v.Z, J3w.X, J3w.Y, J3w.Z };

                Vector3 J4v = Vector3.Cross(H0_4.Right, currentCoord - H0_4.Translation);
                //Vector3 J4w = H0_4.Right;
                Vector3 J4w = Vector3.TransformNormal(H0_4.Right, Matrix.Transpose(H0_8.GetOrientation()));
                double[] J4 = new double[6] { J4v.X, J4v.Y, J4v.Z, J4w.X, J4w.Y, J4w.Z };

                Vector3 J5v = -1f * H0_5.Backward;
                Vector3 J5w = Vector3.Zero;
                double[] J5 = new double[6] { J5v.X, J5v.Y, J5v.Z, J5w.X, J5w.Y, J5w.Z };

                Vector3 J6v = Vector3.Cross(H0_6.Up, currentCoord - H0_6.Translation);
                //Vector3 J6w = H0_6.Up;
                Vector3 J6w = Vector3.TransformNormal(H0_6.Up, Matrix.Transpose(H0_8.GetOrientation()));
                double[] J6 = new double[6] { J6v.X, J6v.Y, J6v.Z, J6w.X, J6w.Y, J6w.Z };

                Vector3 J7v = Vector3.Cross(H0_7.Right, currentCoord - H0_7.Translation);
                //Vector3 J7w = H0_7.Right;
                Vector3 J7w = Vector3.TransformNormal(H0_7.Right, Matrix.Transpose(H0_8.GetOrientation()));
                double[] J7 = new double[6] { J7v.X, J7v.Y, J7v.Z, J7w.X, J7w.Y, J7w.Z };

                Vector3 J8v = Vector3.Cross(H0_8.Backward, currentCoord - H0_8.Translation);
                //Vector3 J8w = H0_8.Backward;
                Vector3 J8w = Vector3.TransformNormal(H0_8.Backward, Matrix.Transpose(H0_8.GetOrientation()));
                double[] J8 = new double[6] { J8v.X, J8v.Y, J8v.Z, J8w.X, J8w.Y, J8w.Z };

                double[,] J = new double[6, 9]
                {
                    { J0[0], J1[0], J2[0], J3[0], J4[0], J5[0], J6[0], J7[0], J8[0] },
                    { J0[1], J1[1], J2[1], J3[1], J4[1], J5[1], J6[1], J7[1], J8[1] },
                    { J0[2], J1[2], J2[2], J3[2], J4[2], J5[2], J6[2], J7[2], J8[2] },
                    { J0[3], J1[3], J2[3], J3[3], J4[3], J5[3], J6[3], J7[3], J8[3] },
                    { J0[4], J1[4], J2[4], J3[4], J4[4], J5[4], J6[4], J7[4], J8[4] },
                    { J0[5], J1[5], J2[5], J3[5], J4[5], J5[5], J6[5], J7[5], J8[5] }
                };

                double[] taskWeights = new double[6] { 1, 1, 1, 100, 100, 100 };

                double[] jointWeights = new double[9]
                { 
                    1000 + 200 * Math.Exp((_joint0.CurrentExtension - _joint0.MaxExtension) / 3d) + 200 * Math.Exp((_joint0.MinExtension - _joint0.CurrentExtension) / 3d),
                    1 + 200 * Math.Exp((_joint1.CurrentAngle - _joint1.MaxAngle) / 0.52d) + 200 * Math.Exp((_joint1.MinAngle - _joint1.CurrentAngle) / 0.52d),
                    1 + 200 * Math.Exp((_joint2.CurrentAngle - _joint2.MaxAngle) / 0.52d) + 200 * Math.Exp((_joint2.MinAngle - _joint2.CurrentAngle) / 0.52d),
                    1 + 200 * Math.Exp((_joint3.CurrentExtension - _joint3.MaxExtension) / 3d) + 200 * Math.Exp((_joint3.MinExtension - _joint3.CurrentExtension) / 3d),
                    1 + 200 * Math.Exp((_joint4.CurrentAngle - _joint4.MaxAngle) / 0.52d) + 200 * Math.Exp((_joint4.MinAngle - _joint4.CurrentAngle) / 0.52d),
                    1 + 200 * Math.Exp((_joint5.CurrentExtension - _joint5.MaxExtension) / 3d) + 200 * Math.Exp((_joint5.MinExtension - _joint5.CurrentExtension) / 3d),
                    1 + 200 * Math.Exp((_joint6.CurrentAngle - _joint6.MaxAngle) / 0.52d) + 200 * Math.Exp((_joint6.MinAngle - _joint6.CurrentAngle) / 0.52d),
                    1 + 200 * Math.Exp((_joint7.CurrentAngle - _joint7.MaxAngle) / 0.52d) + 200 * Math.Exp((_joint7.MinAngle - _joint7.CurrentAngle) / 0.52d),
                    1 + 200 * Math.Exp((_joint8.CurrentAngle - _joint8.MaxAngle) / 0.52d) + 200 * Math.Exp((_joint8.MinAngle - _joint8.CurrentAngle) / 0.52d)
                };

                double[,] J_pseudoInv = MyMath.DampedWeightedPseudoInverseWide(J, taskWeights, jointWeights, 0.05f);

                double[] inputSignal = new double[6];

                Vector3 trans0 = Vector3.Zero;
                Vector3 trans1 = Vector3.Zero;
                Vector3 trans2 = Vector3.Zero;

                Vector3 rot0 = Vector3.Zero;
                Vector3 rot1 = Vector3.Zero;
                Vector3 rot2 = Vector3.Zero;

                if (OCtrl)
                {
                    if (input.WPress) rot1 = 0.2f * Vector3.Right;
                    else if (input.SPress) rot1 = -0.2f * Vector3.Right;

                    if (input.APress) rot0 = 0.2f * Vector3.Up;
                    else if (input.DPress) rot0 = -0.2f * Vector3.Up;

                    if (input.SpacePress) rot2 = 0.2f * Vector3.Backward;
                    else if (input.CPress) rot2 = -0.2f * Vector3.Backward;
                }
                else
                {
                    if (input.WPress) trans0 = -1f * Vector3.Backward;
                    else if (input.SPress) trans0 = 1f * Vector3.Backward;

                    if (input.APress) trans1 = -1f * Vector3.Right;
                    else if (input.DPress) trans1 = 1f * Vector3.Right;

                    if (input.SpacePress) trans2 = 1f * Vector3.Up;
                    else if (input.CPress) trans2 = -1f * Vector3.Up;
                }

                Vector3 transInput = trans0 + trans1 + trans2;
                Vector3 rotInput = rot0 + rot1 + rot2;

                inputSignal[0] = transInput.X;
                inputSignal[1] = transInput.Y;
                inputSignal[2] = transInput.Z;
                inputSignal[3] = rotInput.X;
                inputSignal[4] = rotInput.Y;
                inputSignal[5] = rotInput.Z;

                double[] inputSignalNull = new double[9];

                if (input.QPress)
                {
                    inputSignalNull[0] = 0.5f;
                }
                else if (input.EPress)
                {
                    inputSignalNull[0] = -0.5f;
                }

                double[] outputSignal = MyMath.MultiplyMatrixVector(J_pseudoInv, inputSignal);
                double[,] N = MyMath.NullSpaceProjector(J, J_pseudoInv);
                double[] outputSignalNull = MyMath.MultiplyMatrixVector(N, inputSignalNull);
                double[] totalOutputSignal = new double[9];

                for (int i = 0; i < 9; i++)
                {
                    totalOutputSignal[i] = outputSignal[i] + outputSignalNull[i];
                }

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
                _joint8.Velocity = (float)totalOutputSignal[8];
                if (_joint8.IsSaturated)
                {
                    jointWeights[8] = double.MaxValue;
                    oob = true;
                }

                if (oob)
                {
                    // recompute with updated joint weights
                    J_pseudoInv = MyMath.DampedWeightedPseudoInverseWide(J, taskWeights, jointWeights, 0.05f);
                    outputSignal = MyMath.MultiplyMatrixVector(J_pseudoInv, inputSignal);
                    N = MyMath.NullSpaceProjector(J, J_pseudoInv);
                    outputSignalNull = MyMath.MultiplyMatrixVector(N, inputSignalNull);
                    for (int i = 0; i < 9; i++)
                    {
                        totalOutputSignal[i] = outputSignal[i] + outputSignalNull[i];
                    }
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
                    _joint8.Velocity = (float)totalOutputSignal[8];
                    if (_joint8.IsSaturated)
                    {
                        totalOutputSignal[8] = 0f;
                        _joint8.Velocity = 0f;
                    }
                    oob = false;
                }

                double[] achievableVelocities = MyMath.MultiplyMatrixVector(J, totalOutputSignal);
                double[] errors = MyMath.SubtractVectors(inputSignal, achievableVelocities);
                double[] tolerances = new double[6] { 0.1, 0.1, 0.1, 0.01, 0.01, 0.01 };

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
                    _joint8.Velocity = 0f;
                }

                return true;
            }

            public bool ToggleOrientationControl()
            {
                OCtrl = !OCtrl;
                return true;
            }
        }
    }
}
