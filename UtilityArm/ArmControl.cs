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
            private PistonSeries _basePistonsA;
            private PistonSeries _basePistonsB;
            private Vector3 _baseVector;
            private Rotor _joint0;
            private Vector3 _seg0Vector;
            private Rotor _joint1A;
            private Rotor _joint1B;
            private Vector3 _seg1Vector;
            private Rotor _joint2A;
            private Rotor _joint2B;
            private Vector3 _seg2Vector;
            private Rotor _joint3;
            private Vector3 _seg3Vector;
            private Rotor _joint4;
            private Vector3 _seg4Vector;

            private Vector3 _seg4VectorWelder = new Vector3(0, -3f, 0);
            private Vector3 _seg4VectorGrinder = new Vector3(0, -3f, 0);
            private Vector3 _seg4VectorDrill = new Vector3(0, -3f, 0);
            private Vector3 _seg4VectorConnector = new Vector3(0, -1f, 0);
            private Vector3 _seg4VectorMagnet = new Vector3(0, -1.5f, 0);
            private Vector3 _seg4VectorEmpty = Vector3.Zero;

            private float _translationSpeed = 1f;

            public ArmControlMode ControlMode { get; private set; } = ArmControlMode.Translate;
            public TranslationMode TranslationMode { get; private set; } = TranslationMode.World;
            public ArmAttachment Attachment { get; private set; } = ArmAttachment.Empty;
            public bool HasAttachment => Attachment != ArmAttachment.Empty;
            public Vector3 EEPosition { get; private set; }
            public float TranslationSpeed => _translationSpeed;
            public string ID { get; private set; }

            public ArmControl(string id)
            {
                ID = id.ToUpper();
                Piston basePistonA_0 = new Piston($"{ID} BASE PISTON A_0");
                Piston basePistonA_1 = new Piston($"{ID} BASE PISTON A_1");
                Piston basePistonB_0 = new Piston($"{ID} BASE PISTON B_0");
                Piston basePistonB_1 = new Piston($"{ID} BASE PISTON B_1");
                _basePistonsA = new PistonSeries(basePistonA_0, basePistonA_1);
                _basePistonsB = new PistonSeries(basePistonB_0, basePistonB_1);
                _joint0 = new Rotor($"{ID} ARM JOINT 0");
                _joint1A = new Rotor($"{ID} ARM JOINT 1A");
                _joint1B = new Rotor($"{ID} ARM JOINT 1B");
                _joint2A = new Rotor($"{ID} ARM JOINT 2A");
                _joint2B = new Rotor($"{ID} ARM JOINT 2B");
                _joint3 = new Rotor($"{ID} ARM JOINT 3");
                _joint4 = new Rotor($"{ID} ARM JOINT 4");

                _baseVector = new Vector3(0, 2.7f, 0);
                _seg0Vector = new Vector3(0, 1.25f, 0);
                _seg1Vector = new Vector3(0, 0, -50f);
                _seg2Vector = new Vector3(0, -2.5f, -50f);
                _seg3Vector = new Vector3(0, -1.65f, 0);
            }

            public void Control(UserInput userInput)
            {
                if (!_joint4.RotorBlock.IsAttached)
                {
                    Attachment = ArmAttachment.Empty;
                }

                switch (Attachment)
                {
                    case ArmAttachment.Welder:
                        _seg4Vector = _seg4VectorWelder;
                        break;
                    case ArmAttachment.Grinder:
                        _seg4Vector = _seg4VectorGrinder;
                        break;
                    case ArmAttachment.Drill:
                        _seg4Vector = _seg4VectorDrill;
                        break;
                    case ArmAttachment.Connector:
                        _seg4Vector = _seg4VectorConnector;
                        break;
                    case ArmAttachment.Magnet:
                        _seg4Vector = _seg4VectorMagnet;
                        break;
                    case ArmAttachment.Empty:
                        _seg4Vector = _seg4VectorEmpty;
                        break;
                }

                Matrix H0 = Matrix.CreateRotationY(_joint0.AngleRad);
                H0.Translation = _baseVector + _joint0.RotorBlock.Displacement * H0.Up;
                Matrix H1 = Matrix.CreateRotationX(_joint1A.AngleRad);
                H1.Translation = _seg0Vector;
                Matrix H2 = Matrix.CreateRotationX(_joint2A.AngleRad);
                H2.Translation = _seg1Vector;
                Matrix H3 = Matrix.CreateRotationX(_joint3.AngleRad);
                H3.Translation = _seg2Vector;
                Matrix H4 = HasAttachment ? Matrix.CreateRotationY(_joint4.AngleRad) : Matrix.Identity;
                H4.Translation = _seg3Vector;
                Matrix H5 = Matrix.Identity;
                H5.Translation = _seg4Vector;

                Matrix H0_1 = H1 * H0;
                Matrix H0_2 = H2 * H0_1;
                Matrix H0_3 = H3 * H0_2;
                Matrix H0_4 = H4 * H0_3;
                Matrix H0_5 = H5 * H0_4;

                Vector3 currentCoord = H0_5.Translation;

                EEPosition = currentCoord;

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
                        _joint1A.VelocityRad = -0.25f;
                        _joint1B.VelocityRad = -0.25f;
                    }
                    else if (userInput.SPress)
                    {
                        _joint1A.VelocityRad = 0.25f;
                        _joint1B.VelocityRad = 0.25f;
                    }
                    else
                    {
                        _joint1A.VelocityRad = 0f;
                        _joint1B.VelocityRad = 0f;
                    }

                    if (userInput.SpacePress)
                    {
                        _basePistonsA.Velocity = 0.5f;
                        _basePistonsB.Velocity = 0.5f;
                    }
                    else if (userInput.CPress)
                    {
                        _basePistonsA.Velocity = -0.5f;
                        _basePistonsB.Velocity = -0.5f;
                    }
                    else
                    {
                        _basePistonsA.Velocity = 0f;
                        _basePistonsB.Velocity = 0f;
                    }

                    if (userInput.QPress)
                    {
                        _joint2A.VelocityRad = 0.25f;
                        _joint2B.VelocityRad = 0.25f;
                    }
                    else if (userInput.EPress)
                    {
                        _joint2A.VelocityRad = -0.25f;
                        _joint2B.VelocityRad = -0.25f;
                    }
                    else
                    {
                        _joint2A.VelocityRad = 0f;
                        _joint2B.VelocityRad = 0f;
                    }

                    return;
                }

                Vector3 J0v = Vector3.Cross(H0.Up, currentCoord - H0.Translation);
                float J0wX = Vector3.Dot(H0.Up, H0_3.Right);
                float J0wY = Vector3.Dot(H0.Up, H0_4.Up);
                MyVector J0 = new MyVector(5) { V0 = J0v.X, V1 = J0v.Y, V2 = J0v.Z, V3 = J0wX, V4 = J0wY };

                Vector3 J1v = Vector3.Cross(H0_1.Right, currentCoord - H0_1.Translation);
                float J1wX = Vector3.Dot(H0_1.Right, H0_3.Right);
                float J1wY = Vector3.Dot(H0_1.Right, H0_4.Up);
                MyVector J1 = new MyVector(5) { V0 = J1v.X, V1 = J1v.Y, V2 = J1v.Z, V3 = J1wX, V4 = J1wY };

                Vector3 J2v = Vector3.Cross(H0_2.Right, currentCoord - H0_2.Translation);
                float J2wX = Vector3.Dot(H0_2.Right, H0_3.Right);
                float J2wY = Vector3.Dot(H0_2.Right, H0_4.Up);
                MyVector J2 = new MyVector(5) { V0 = J2v.X, V1 = J2v.Y, V2 = J2v.Z, V3 = J2wX, V4 = J2wY };

                Vector3 J3v = Vector3.Cross(H0_3.Right, currentCoord - H0_3.Translation);
                float J3wX = Vector3.Dot(H0_3.Right, H0_3.Right);
                float J3wY = Vector3.Dot(H0_3.Right, H0_4.Up);
                MyVector J3 = new MyVector(5) { V0 = J3v.X, V1 = J3v.Y, V2 = J3v.Z, V3 = J3wX, V4 = J3wY };

                Vector3 J4v = Vector3.Cross(H0_4.Up, currentCoord - H0_4.Translation);
                float J4wX = Vector3.Dot(H0_4.Up, H0_3.Right);
                float J4wY = Vector3.Dot(H0_4.Up, H0_4.Up);
                MyVector J4 = new MyVector(5) { V0 = J4v.X, V1 = J4v.Y, V2 = J4v.Z, V3 = J4wX, V4 = J4wY };

                MyMatrix J = new MyMatrix(5, 5)
                {
                    M00 = J0[0],
                    M01 = J1[0],
                    M02 = J2[0],
                    M03 = J3[0],
                    M04 = J4[0],
                    M10 = J0[1],
                    M11 = J1[1],
                    M12 = J2[1],
                    M13 = J3[1],
                    M14 = J4[1],
                    M20 = J0[2],
                    M21 = J1[2],
                    M22 = J2[2],
                    M23 = J3[2],
                    M24 = J4[2],
                    M30 = J0[3],
                    M31 = J1[3],
                    M32 = J2[3],
                    M33 = J3[3],
                    M34 = J4[3],
                    M40 = J0[4],
                    M41 = J1[4],
                    M42 = J2[4],
                    M43 = J3[4],
                    M44 = J4[4]
                };

                if (!HasAttachment)
                {
                    MyVector zeroVec = MyVector.Zero(5);
                    MyMathExt.SetRow(ref J, 4, ref zeroVec);
                    MyMathExt.SetColumn(ref J, 4, ref zeroVec);
                }

                MyVector inputSignal = new MyVector(5);

                Vector3 trans0 = Vector3.Zero;
                Vector3 trans1 = Vector3.Zero;
                Vector3 trans2 = Vector3.Zero;

                Vector3 rot0 = Vector3.Zero;
                Vector3 rot1 = Vector3.Zero;

                TranslationMode = HasAttachment ? TranslationMode : TranslationMode.World;

                Vector3 transXDir;
                Vector3 transYDir;
                Vector3 transZDir;

                Vector3 rotXDir = Vector3.Right;
                Vector3 rotYDir = Vector3.Up;

                switch (TranslationMode)
                {
                    case TranslationMode.World:
                        transXDir = Vector3.Right;
                        transYDir = Vector3.Up;
                        transZDir = Vector3.Backward;
                        break;
                    case TranslationMode.Local:
                        transXDir = H0_5.Right;
                        transYDir = H0_5.Up;
                        transZDir = H0_5.Backward;
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
                            if (userInput.WPress) trans0 = -_translationSpeed * transZDir;
                            else if (userInput.SPress) trans0 = _translationSpeed * transZDir;

                            if (userInput.APress) trans1 = -_translationSpeed * transXDir;
                            else if (userInput.DPress) trans1 = _translationSpeed * transXDir;

                            if (userInput.SpacePress) trans2 = _translationSpeed * transYDir;
                            else if (userInput.CPress) trans2 = -_translationSpeed * transYDir;
                            break;
                        }
                    case ArmControlMode.Rotate:
                        {
                            if (userInput.SpacePress) rot1 = 0.2f * rotXDir;
                            else if (userInput.CPress) rot1 = -0.2f * rotXDir;

                            if (userInput.APress) rot0 = 0.2f * rotYDir;
                            else if (userInput.DPress) rot0 = -0.2f * rotYDir;
                            break;
                        }
                }

                Vector3 transInput = trans0 + trans1 + trans2;
                Vector3 rotInput = rot0 + rot1;

                inputSignal[0] = transInput.X;
                inputSignal[1] = transInput.Y;
                inputSignal[2] = transInput.Z;
                inputSignal[3] = rotInput.X;
                inputSignal[4] = rotInput.Y;

                if (!HasAttachment)
                {
                    inputSignal[4] = 0f;
                }

                MyMatrix J_pi;
                MyMathExt.DampedPseudoInverseWide(ref J, out J_pi, 0.1f);
                MyVector outputSignal;
                MyMathExt.Multiply(ref J_pi, ref inputSignal, out outputSignal);

                _joint0.VelocityRad = (float)outputSignal[0];
                if (_joint0.IsSaturated)
                {
                    outputSignal[0] = 0f;
                }
                _joint1A.VelocityRad = (float)outputSignal[1];
                if (_joint1A.IsSaturated)
                {
                    outputSignal[1] = 0f;
                }
                _joint2A.VelocityRad = (float)outputSignal[2];
                if (_joint2A.IsSaturated)
                {
                    outputSignal[2] = 0f;
                }
                _joint3.VelocityRad = (float)outputSignal[3];
                if (_joint3.IsSaturated)
                {
                    outputSignal[3] = 0f;
                }
                _joint4.VelocityRad = (float)outputSignal[4];
                if (_joint4.IsSaturated)
                {
                    outputSignal[4] = 0f;
                }

                MyVector achieved;
                MyMathExt.Multiply(ref J, ref outputSignal, out achieved);
                MyVector tolerances = new MyVector(5) { V0 = 0.1, V1 = 0.1, V2 = 0.1, V3 = 0.05, V4 = 0.05 };

                for (int i = 0; i < 5; i++)
                {
                    if (Math.Abs(achieved[i] - inputSignal[i]) > tolerances[i])
                    {
                        outputSignal[0] = 0f;
                        outputSignal[1] = 0f;
                        outputSignal[2] = 0f;
                        outputSignal[3] = 0f;
                        outputSignal[4] = 0f;
                        break;
                    }
                }

                _joint0.VelocityRad = (float)outputSignal[0];
                _joint1A.VelocityRad = (float)outputSignal[1];
                _joint1B.VelocityRad = _joint1A.VelocityRad;
                _joint2A.VelocityRad = (float)outputSignal[2];
                _joint2B.VelocityRad = _joint2A.VelocityRad;
                _joint3.VelocityRad = (float)outputSignal[3];
                _joint4.VelocityRad = (float)outputSignal[4];
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

            public void SetTranslationSpeed(float speed)
            {
                _translationSpeed = speed;
            }

            public void AdjustTranslationSpeed(float delta)
            {
                _translationSpeed += delta;
                _translationSpeed = MathHelper.Clamp(_translationSpeed, 0.1f, 5f);
            }
        }
    }
}
