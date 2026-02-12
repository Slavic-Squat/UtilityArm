using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public struct MyMatrix
        {
            // Row-major storage (up to 6×6)
            public double M00, M01, M02, M03, M04, M05;
            public double M10, M11, M12, M13, M14, M15;
            public double M20, M21, M22, M23, M24, M25;
            public double M30, M31, M32, M33, M34, M35;
            public double M40, M41, M42, M43, M44, M45;
            public double M50, M51, M52, M53, M54, M55;

            public int Rows, Cols; // Actual dimensions (1-6 each)

            public MyMatrix(int rows, int cols)
            {
                if (rows < 1 || rows > 6 || cols < 1 || cols > 6)
                    throw new ArgumentException("Dimensions must be 1-6");
                Rows = rows;
                Cols = cols;
                M00 = M01 = M02 = M03 = M04 = M05 = 0;
                M10 = M11 = M12 = M13 = M14 = M15 = 0;
                M20 = M21 = M22 = M23 = M24 = M25 = 0;
                M30 = M31 = M32 = M33 = M34 = M35 = 0;
                M40 = M41 = M42 = M43 = M44 = M45 = 0;
                M50 = M51 = M52 = M53 = M54 = M55 = 0;
            }

            public double this[int row, int col]
            {
                get
                {
                    if (row < 0 || row >= Rows || col < 0 || col >= Cols)
                        throw new Exception($"Index [{row},{col}] out of range [{Rows},{Cols}]");
                    switch (row * 6 + col)
                    {
                        case 0: return M00;
                        case 1: return M01;
                        case 2: return M02;
                        case 3: return M03;
                        case 4: return M04;
                        case 5: return M05;
                        case 6: return M10;
                        case 7: return M11;
                        case 8: return M12;
                        case 9: return M13;
                        case 10: return M14;
                        case 11: return M15;
                        case 12: return M20;
                        case 13: return M21;
                        case 14: return M22;
                        case 15: return M23;
                        case 16: return M24;
                        case 17: return M25;
                        case 18: return M30;
                        case 19: return M31;
                        case 20: return M32;
                        case 21: return M33;
                        case 22: return M34;
                        case 23: return M35;
                        case 24: return M40;
                        case 25: return M41;
                        case 26: return M42;
                        case 27: return M43;
                        case 28: return M44;
                        case 29: return M45;
                        case 30: return M50;
                        case 31: return M51;
                        case 32: return M52;
                        case 33: return M53;
                        case 34: return M54;
                        case 35: return M55;
                        default: throw new Exception();
                    }
                }
                set
                {
                    if (row < 0 || row >= Rows || col < 0 || col >= Cols)
                        throw new Exception($"Index [{row},{col}] out of range [{Rows},{Cols}]");
                    switch (row * 6 + col)
                    {
                        case 0: M00 = value; break;
                        case 1: M01 = value; break;
                        case 2: M02 = value; break;
                        case 3: M03 = value; break;
                        case 4: M04 = value; break;
                        case 5: M05 = value; break;
                        case 6: M10 = value; break;
                        case 7: M11 = value; break;
                        case 8: M12 = value; break;
                        case 9: M13 = value; break;
                        case 10: M14 = value; break;
                        case 11: M15 = value; break;
                        case 12: M20 = value; break;
                        case 13: M21 = value; break;
                        case 14: M22 = value; break;
                        case 15: M23 = value; break;
                        case 16: M24 = value; break;
                        case 17: M25 = value; break;
                        case 18: M30 = value; break;
                        case 19: M31 = value; break;
                        case 20: M32 = value; break;
                        case 21: M33 = value; break;
                        case 22: M34 = value; break;
                        case 23: M35 = value; break;
                        case 24: M40 = value; break;
                        case 25: M41 = value; break;
                        case 26: M42 = value; break;
                        case 27: M43 = value; break;
                        case 28: M44 = value; break;
                        case 29: M45 = value; break;
                        case 30: M50 = value; break;
                        case 31: M51 = value; break;
                        case 32: M52 = value; break;
                        case 33: M53 = value; break;
                        case 34: M54 = value; break;
                        case 35: M55 = value; break;
                    }
                }
            }

            public static MyMatrix Identity(int size)
            {
                var m = new MyMatrix(size, size);
                for (int i = 0; i < size; i++)
                    m[i, i] = 1.0;
                return m;
            }

            public static MyMatrix Zero(int rows, int cols) => new MyMatrix(rows, cols);
        }
    }
}
