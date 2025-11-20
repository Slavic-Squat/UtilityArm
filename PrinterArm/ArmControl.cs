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
            private Piston _joint2;
            private Vector3 _seg2Vector;
            private Rotor _joint3;
            private Vector3 _seg3Vector;
            private PistonSeries _joint4;
            private Vector3 _seg4Vector;
            private Rotor _joint5;
            private Vector3 _seg5Vector;

            private bool _oob;

            public bool OCtrl { get; private set; }
            public Vector3 EEPosition { get; private set; }

            public ArmControl()
            {
                _joint0 = new Rotor("Joint0");
                _joint1 = new Rotor("Joint1");
                _joint2 = new Piston("Joint2");
                _joint3 = new Rotor("Joint3");
                Piston joint4_0 = new Piston("Joint4_0");
                Piston joint4_1 = new Piston("Joint4_1");
                _joint4 = new PistonSeries(joint4_0, joint4_1);
                _joint5 = new Rotor("Joint5");

                _seg0Vector = new Vector3(4.0f, 1.25f, 0);
                _seg1Vector = new Vector3(1.25f, 0, -8.9f);
                _seg2Vector = new Vector3(-4.0f, 0, -1.25f);
                _seg3Vector = new Vector3(-1.25f, 0, -11.6f);
                _seg4Vector = new Vector3(0, 0, -1.25f);
                _seg5Vector = new Vector3(0, 0, -3.6f);
            }

            public bool Control(UserInput input, Matrix inputDir = default(Matrix))
            {
                Matrix H0 = Matrix.CreateRotationY(_joint0.CurrentAngle);
                H0.Translation = new Vector3(0, 1.45f, 0);
                Matrix H1 = Matrix.CreateRotationX(_joint1.CurrentAngle);
                H1.Translation = _seg0Vector;
                Matrix H2 = Matrix.Identity;
                H2.Translation = _seg1Vector;
                Matrix H3 = Matrix.CreateRotationX(_joint3.CurrentAngle);
                H3.Translation = _seg2Vector + -1f * _joint2.CurrentExtension * H2.Backward;
                Matrix H4 = Matrix.Identity;
                H4.Translation = _seg3Vector;
                Matrix H5 = Matrix.CreateRotationX(_joint5.CurrentAngle);
                H5.Translation = _seg4Vector + -1f * _joint4.CurrentExtension * H4.Backward;
                Matrix H6 = Matrix.Identity;
                H6.Translation = _seg5Vector;

                Matrix HT = H6 * H5 * H4 * H3 * H2 * H1 * H0;
                Vector3 currentCoord = HT.Translation;

                EEPosition = Vector3.TransformNormal(currentCoord, Matrix.Transpose(inputDir.GetOrientation()));

                Matrix H0_1 = H1 * H0;
                Matrix H0_2 = H2 * H1 * H0;
                Matrix H0_3 = H3 * H2 * H1 * H0;
                Matrix H0_4 = H4 * H3 * H2 * H1 * H0;
                Matrix H0_5 = H5 * H4 * H3 * H2 * H1 * H0;

                //DebugDraw.DrawMatrix(HT * _joint0.RotorBlock.WorldMatrix, length: 2f);

                Vector3 J0v = Vector3.Cross(H0.Up, currentCoord - H0.Translation);
                float J0w = Vector3.Dot(H0.Up, H0_5.Right);
                double[] J0 = new double[4] { J0v.X, J0v.Y, J0v.Z, J0w };

                Vector3 J1v = Vector3.Cross(H0_1.Right, currentCoord - H0_1.Translation);
                float J1w = Vector3.Dot(H0_1.Right, H0_5.Right);
                double[] J1 = new double[4] { J1v.X, J1v.Y, J1v.Z, J1w };

                Vector3 J2v = -1f * H0_2.Backward;
                float J2w = 0f;
                double[] J2 = new double[4] { J2v.X, J2v.Y, J2v.Z, J2w };

                Vector3 J3v = Vector3.Cross(H0_3.Right, currentCoord - H0_3.Translation);
                float J3w = Vector3.Dot(H0_3.Right, H0_5.Right);
                double[] J3 = new double[4] { J3v.X, J3v.Y, J3v.Z, J3w };

                Vector3 J4v = -1f * H0_4.Backward;
                float J4w = 0f;
                double[] J4 = new double[4] { J4v.X, J4v.Y, J4v.Z, J4w };

                Vector3 J5v = Vector3.Cross(H0_5.Right, currentCoord - H0_5.Translation);
                float J5w = 1f;
                double[] J5 = new double[4] { J5v.X, J5v.Y, J5v.Z, J5w };

                double[,] J = new double[4, 6]
                {
                    { J0[0], J1[0], J2[0], J3[0], J4[0], J5[0] },
                    { J0[1], J1[1], J2[1], J3[1], J4[1], J5[1] },
                    { J0[2], J1[2], J2[2], J3[2], J4[2], J5[2] },
                    { J0[3], J1[3], J2[3], J3[3], J4[3], J5[3] }
                };

                double[] taskWeights = new double[4] { 1, 1, 1, 100 };

                double[] jointWeights = new double[6]
                { 
                    1 + 200 * Math.Exp((_joint0.CurrentAngle - _joint0.MaxAngle) / 0.52d) + 200 * Math.Exp((_joint0.MinAngle - _joint0.CurrentAngle) / 0.52d),
                    double.MaxValue,
                    100 + 200 * Math.Exp((_joint2.CurrentExtension - _joint2.MaxExtension) / 3d) + 200 * Math.Exp((_joint2.MinExtension - _joint2.CurrentExtension) / 3d),
                    1 + 200 * Math.Exp((_joint3.CurrentAngle - _joint3.MaxAngle) / 0.52d) + 200 * Math.Exp((_joint3.MinAngle - _joint3.CurrentAngle) / 0.52d),
                    1 + 200 * Math.Exp((_joint4.CurrentExtension - _joint4.MaxExtension) / 6d) + 200 * Math.Exp((_joint4.MinExtension - _joint4.CurrentExtension) / 6d),
                    1 + 200 * Math.Exp((_joint5.CurrentAngle - _joint5.MaxAngle) / 0.52d) + 200 * Math.Exp((_joint5.MinAngle - _joint5.CurrentAngle) / 0.52d)
                };

                double[,] J_pseudoInv = MyMath.DampedWeightedPseudoInverseWide(J, taskWeights, jointWeights, 0.01f);

                double[] inputSignal = new double[4];

                if (ReferenceEquals(inputDir, default(Matrix)))
                {
                    inputDir = Matrix.Identity;
                }
                Vector3 trans0 = Vector3.Zero;
                Vector3 trans1 = Vector3.Zero;
                Vector3 trans2 = Vector3.Zero;

                float rot = 0f;

                if (OCtrl)
                {
                    if (input.WPress) rot = 0.5f;
                    else if (input.SPress) rot = -0.5f;
                }
                else
                {
                    if (input.WPress) trans0 = -1f * inputDir.Backward;
                    else if (input.SPress) trans0 = 1f * inputDir.Backward;

                    if (input.APress) trans1 = -1f * inputDir.Right;
                    else if (input.DPress) trans1 = 1f * inputDir.Right;

                    if (input.SpacePress) trans2 = 1f * inputDir.Up;
                    else if (input.CPress) trans2 = -1f * inputDir.Up;
                }

                Vector3 transInput = trans0 + trans1 + trans2;

                inputSignal[0] = transInput.X;
                inputSignal[1] = transInput.Y;
                inputSignal[2] = transInput.Z;
                inputSignal[3] = rot;

                double[] inputSignalNull = new double[6];

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
                double[] totalOutputSignal = new double[6];

                for (int i = 0; i < 6; i++)
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

                if (_oob)
                {
                    // recompute with updated joint weights
                    J_pseudoInv = MyMath.DampedWeightedPseudoInverseWide(J, taskWeights, jointWeights, 0.01f);
                    outputSignal = MyMath.MultiplyMatrixVector(J_pseudoInv, inputSignal);
                    N = MyMath.NullSpaceProjector(J, J_pseudoInv);
                    outputSignalNull = MyMath.MultiplyMatrixVector(N, inputSignalNull);
                    for (int i = 0; i < 6; i++)
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
                    _oob = false;
                }

                double[] achievableVelocites = MyMath.MultiplyMatrixVector(J, totalOutputSignal);
                double achievableNorm = MyMath.Norm(achievableVelocites);
                double desiredNorm = MyMath.Norm(inputSignal);

                double[] achievableDir = new double[4];
                if (achievableNorm > 1e-6)
                {
                    achievableDir = MyMath.MultiplyScalar(achievableVelocites, 1d / achievableNorm);
                }

                double[] desiredDir = new double[4];
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
