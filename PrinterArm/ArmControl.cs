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

            private bool _oob;

            public bool OCtrl { get; private set; }
            public Vector3 EEPosition { get; private set; }

            public ArmControl()
            {
                _joint0 = new Rotor("Joint0");
                _joint1 = new Rotor("Joint1");
                Piston joint2_0 = new Piston("Joint2_0");
                Piston joint2_1 = new Piston("Joint2_1");
                _joint2 = new PistonSeries(joint2_0, joint2_1);
                _joint3 = new Rotor("Joint3");
                Piston joint4_0 = new Piston("Joint4_0");
                Piston joint4_1 = new Piston("Joint4_1");
                _joint4 = new PistonSeries(joint4_0, joint4_1);
                _joint5 = new Rotor("Joint5");
                _joint6 = new Rotor("Joint6");
                _joint7 = new Rotor("Joint7");

                _seg0Vector = new Vector3(0, 0, 0);
                _seg1Vector = new Vector3(0, 0, 0);
                _seg2Vector = new Vector3(0, 0, 0);
                _seg3Vector = new Vector3(0, 0, 0);
                _seg4Vector = new Vector3(0, 0, 0);
                _seg5Vector = new Vector3(0, 0, 0);
                _seg6Vector = new Vector3(0, 0, 0);
                _seg7Vector = new Vector3(0, 0, 0);
            }

            public bool Control(UserInput input)
            {
                Matrix H0 = Matrix.CreateRotationY(_joint0.CurrentAngle);
                H0.Translation = new Vector3(0, 0, 0);
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
                Matrix H7 = Matrix.CreateRotationZ(_joint7.CurrentAngle);
                H7.Translation = _seg6Vector;
                Matrix H8 = Matrix.Identity;
                H8.Translation = _seg7Vector;

                Matrix HT = H8 * H7 * H6 * H5 * H4 * H3 * H2 * H1 * H0;
                Vector3 currentCoord = HT.Translation;
                EEPosition = currentCoord;

                Matrix H0_1 = H1 * H0;
                Matrix H0_2 = H2 * H1 * H0;
                Matrix H0_3 = H3 * H2 * H1 * H0;
                Matrix H0_4 = H4 * H3 * H2 * H1 * H0;
                Matrix H0_5 = H5 * H4 * H3 * H2 * H1 * H0;
                Matrix H0_6 = H6 * H5 * H4 * H3 * H2 * H1 * H0;
                Matrix H0_7 = H7 * H6 * H5 * H4 * H3 * H2 * H1 * H0;

                //DebugDraw.DrawMatrix(HT * _joint0.RotorBlock.WorldMatrix, length: 2f);

                Vector3 J0v = Vector3.Cross(H0.Up, currentCoord - H0.Translation);
                Vector3 J0w = H0.Up;
                double[] J0 = new double[6] { J0v.X, J0v.Y, J0v.Z, J0w.X, J0w.Y, J0w.Z };

                Vector3 J1v = Vector3.Cross(H0_1.Right, currentCoord - H0_1.Translation);
                Vector3 J1w = H0_1.Right;
                double[] J1 = new double[6] { J1v.X, J1v.Y, J1v.Z, J1w.X, J1w.Y, J1w.Z };

                Vector3 J2v = -1f * H0_2.Backward;
                Vector3 J2w = Vector3.Zero;
                double[] J2 = new double[6] { J2v.X, J2v.Y, J2v.Z, J2w.X, J2w.Y, J2w.Z };

                Vector3 J3v = Vector3.Cross(H0_3.Right, currentCoord - H0_3.Translation);
                Vector3 J3w = H0_3.Right;
                double[] J3 = new double[6] { J3v.X, J3v.Y, J3v.Z, J3w.X, J3w.Y, J3w.Z };

                Vector3 J4v = -1f * H0_4.Backward;
                Vector3 J4w = Vector3.Zero;
                double[] J4 = new double[6] { J4v.X, J4v.Y, J4v.Z, J4w.X, J4w.Y, J4w.Z };

                Vector3 J5v = Vector3.Cross(H0_5.Up, currentCoord - H0_5.Translation);
                Vector3 J5w = H0_5.Up;
                double[] J5 = new double[6] { J5v.X, J5v.Y, J5v.Z, J5w.X, J5w.Y, J5w.Z };

                Vector3 J6v = Vector3.Cross(H0_6.Right, currentCoord - H0_6.Translation);
                Vector3 J6w = H0_6.Right;
                double[] J6 = new double[6] { J6v.X, J6v.Y, J6v.Z, J6w.X, J6w.Y, J6w.Z };

                Vector3 J7v = Vector3.Cross(H0_7.Backward, currentCoord - H0_7.Translation);
                Vector3 J7w = H0_7.Backward;
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

                double[] taskWeights = new double[6] { 1, 1, 1, 100, 100, 100 };

                double[] jointWeights = new double[8]
                { 
                    1 + 200 * Math.Exp((_joint0.CurrentAngle - _joint0.MaxAngle) / 0.52d) + 200 * Math.Exp((_joint0.MinAngle - _joint0.CurrentAngle) / 0.52d),
                    double.MaxValue,
                    100 + 200 * Math.Exp((_joint2.CurrentExtension - _joint2.MaxExtension) / 3d) + 200 * Math.Exp((_joint2.MinExtension - _joint2.CurrentExtension) / 3d),
                    1 + 200 * Math.Exp((_joint3.CurrentAngle - _joint3.MaxAngle) / 0.52d) + 200 * Math.Exp((_joint3.MinAngle - _joint3.CurrentAngle) / 0.52d),
                    1 + 200 * Math.Exp((_joint4.CurrentExtension - _joint4.MaxExtension) / 6d) + 200 * Math.Exp((_joint4.MinExtension - _joint4.CurrentExtension) / 6d),
                    1 + 200 * Math.Exp((_joint5.CurrentAngle - _joint5.MaxAngle) / 0.52d) + 200 * Math.Exp((_joint5.MinAngle - _joint5.CurrentAngle) / 0.52d),
                    1,
                    1
                };

                double[,] J_pseudoInv = MyMath.DampedWeightedPseudoInverseWide(J, taskWeights, jointWeights, 0.01f);

                double[] inputSignal = new double[6];

                Vector3 trans0 = Vector3.Zero;
                Vector3 trans1 = Vector3.Zero;
                Vector3 trans2 = Vector3.Zero;

                Vector3 rot0 = Vector3.Zero;
                Vector3 rot1 = Vector3.Zero;
                Vector3 rot2 = Vector3.Zero;

                if (OCtrl)
                {
                    if (input.WPress) rot1 = 0.5f * H0_7.Right;
                    else if (input.SPress) rot1 = -0.5f * H0_7.Right;

                    if (input.APress) rot0 = 0.5f * H0_7.Up;
                    else if (input.DPress) rot0 = -0.5f * H0_7.Up;

                    if (input.SpacePress) rot2 = 0.5f * H0_7.Backward;
                    else if (input.CPress) rot2 = -0.5f * H0_7.Backward;
                }
                else
                {
                    if (input.WPress) trans0 = -1f * H0_7.Backward;
                    else if (input.SPress) trans0 = 1f * H0_7.Backward;

                    if (input.APress) trans1 = -1f * H0_7.Right;
                    else if (input.DPress) trans1 = 1f * H0_7.Right;

                    if (input.SpacePress) trans2 = 1f * H0_7.Up;
                    else if (input.CPress) trans2 = -1f * H0_7.Up;
                }

                Vector3 transInput = trans0 + trans1 + trans2;
                Vector3 rotInput = rot0 + rot1 + rot2;

                inputSignal[0] = transInput.X;
                inputSignal[1] = transInput.Y;
                inputSignal[2] = transInput.Z;
                inputSignal[3] = rotInput.X;
                inputSignal[4] = rotInput.Y;
                inputSignal[5] = rotInput.Z;

                double[] inputSignalNull = new double[8];

                if (input.QPress)
                {
                    inputSignalNull[1] = 0.1f;
                }
                else if (input.EPress)
                {
                    inputSignalNull[1] = -0.1f;
                }

                double[] outputSignal = MyMath.MultiplyMatrixVector(J_pseudoInv, inputSignal);
                double[,] N = MyMath.NullSpaceProjector(J, J_pseudoInv);
                double[] outputSignalNull = MyMath.MultiplyMatrixVector(N, inputSignalNull);
                double[] totalOutputSignal = new double[8];

                for (int i = 0; i < 8; i++)
                {
                    totalOutputSignal[i] = outputSignal[i] + outputSignalNull[i];
                }

                _joint0.Velocity = (float)totalOutputSignal[0];
                if (_joint0.IsSaturated)
                {
                    jointWeights[0] = double.MaxValue;
                    _oob = true;
                }
                _joint1.Velocity = (float)totalOutputSignal[1];
                if (_joint1.IsSaturated)
                {
                    jointWeights[1] = double.MaxValue;
                    _oob = true;
                }
                _joint2.Velocity = (float)totalOutputSignal[2];
                if (_joint2.IsSaturated)
                {
                    jointWeights[2] = double.MaxValue;
                    _oob = true;
                }
                _joint3.Velocity = (float)totalOutputSignal[3];
                if (_joint3.IsSaturated)
                {
                    jointWeights[3] = double.MaxValue;
                    _oob = true;
                }
                _joint4.Velocity = (float)totalOutputSignal[4];
                if (_joint4.IsSaturated)
                {
                    jointWeights[4] = double.MaxValue;
                    _oob = true;
                }
                _joint5.Velocity = (float)totalOutputSignal[5];
                if (_joint5.IsSaturated)
                {
                    jointWeights[5] = double.MaxValue;
                    _oob = true;
                }
                _joint6.Velocity = (float)totalOutputSignal[6];
                if (_joint6.IsSaturated)
                {
                    jointWeights[6] = double.MaxValue;
                    _oob = true;
                }
                _joint7.Velocity = (float)totalOutputSignal[7];
                if (_joint7.IsSaturated)
                {
                    jointWeights[7] = double.MaxValue;
                    _oob = true;
                }

                if (_oob)
                {
                    // recompute with updated joint weights
                    J_pseudoInv = MyMath.DampedWeightedPseudoInverseWide(J, taskWeights, jointWeights, 0.01f);
                    outputSignal = MyMath.MultiplyMatrixVector(J_pseudoInv, inputSignal);
                    N = MyMath.NullSpaceProjector(J, J_pseudoInv);
                    outputSignalNull = MyMath.MultiplyMatrixVector(N, inputSignalNull);
                    for (int i = 0; i < 8; i++)
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
                    _oob = false;
                }

                double[] achievableVelocites = MyMath.MultiplyMatrixVector(J, totalOutputSignal);
                double achievableNorm = MyMath.Norm(achievableVelocites);
                double desiredNorm = MyMath.Norm(inputSignal);

                double[] achievableDir = new double[6];
                if (achievableNorm > 1e-6)
                {
                    achievableDir = MyMath.MultiplyScalar(achievableVelocites, 1d / achievableNorm);
                }

                double[] desiredDir = new double[6];
                if (desiredNorm > 1e-6)
                {
                    desiredDir = MyMath.MultiplyScalar(inputSignal, 1d / desiredNorm);
                }

                double alignment = MyMath.DotProduct(achievableDir, desiredDir);

                if (achievableNorm < 1e-1 && desiredNorm < 1e-1)
                {
                    alignment = 1d;
                }

                if (alignment < 0.98d)
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
