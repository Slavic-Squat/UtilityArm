using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public static class MyMathExt
        {
            // ============================================================================
            // VECTOR OPERATIONS
            // ============================================================================

            public static void Add(ref MyVector a, ref MyVector b, out MyVector result)
            {
                if (a.Length != b.Length)
                    throw new ArgumentException("Vector lengths must match");

                result = new MyVector(a.Length);
                for (int i = 0; i < a.Length; i++)
                    result[i] = a[i] + b[i];
            }

            public static void Subtract(ref MyVector a, ref MyVector b, out MyVector result)
            {
                if (a.Length != b.Length)
                    throw new ArgumentException("Vector lengths must match");

                result = new MyVector(a.Length);
                for (int i = 0; i < a.Length; i++)
                    result[i] = a[i] - b[i];
            }

            public static void Scale(ref MyVector v, double scalar, out MyVector result)
            {
                result = new MyVector(v.Length);
                for (int i = 0; i < v.Length; i++)
                    result[i] = v[i] * scalar;
            }

            public static double Dot(ref MyVector a, ref MyVector b)
            {
                if (a.Length != b.Length)
                    throw new ArgumentException("Vector lengths must match");

                double sum = 0;
                for (int i = 0; i < a.Length; i++)
                    sum += a[i] * b[i];
                return sum;
            }

            public static double Norm(ref MyVector v)
            {
                double sum = 0;
                for (int i = 0; i < v.Length; i++)
                    sum += v[i] * v[i];
                return Math.Sqrt(sum);
            }

            // ============================================================================
            // MATRIX OPERATIONS
            // ============================================================================

            public static void Transpose(ref MyMatrix m, out MyMatrix result)
            {
                result = new MyMatrix(m.Cols, m.Rows); // Swap dimensions
                for (int i = 0; i < m.Rows; i++)
                    for (int j = 0; j < m.Cols; j++)
                        result[j, i] = m[i, j];
            }

            public static void Multiply(ref MyMatrix a, ref MyMatrix b, out MyMatrix result)
            {
                if (a.Cols != b.Rows)
                    throw new ArgumentException($"Cannot multiply {a.Rows}×{a.Cols} by {b.Rows}×{b.Cols}");

                result = new MyMatrix(a.Rows, b.Cols);
                for (int i = 0; i < a.Rows; i++)
                {
                    for (int j = 0; j < b.Cols; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < a.Cols; k++)
                            sum += a[i, k] * b[k, j];
                        result[i, j] = sum;
                    }
                }
            }

            public static void Multiply(ref MyMatrix m, ref MyVector v, out MyVector result)
            {
                if (m.Cols != v.Length)
                    throw new ArgumentException($"Cannot multiply {m.Rows}×{m.Cols} matrix by length-{v.Length} vector");

                result = new MyVector(m.Rows);
                for (int i = 0; i < m.Rows; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < m.Cols; j++)
                        sum += m[i, j] * v[j];
                    result[i] = sum;
                }
            }

            public static void Add(ref MyMatrix a, ref MyMatrix b, out MyMatrix result)
            {
                if (a.Rows != b.Rows || a.Cols != b.Cols)
                    throw new ArgumentException("Matrix dimensions must match");

                result = new MyMatrix(a.Rows, a.Cols);
                for (int i = 0; i < a.Rows; i++)
                    for (int j = 0; j < a.Cols; j++)
                        result[i, j] = a[i, j] + b[i, j];
            }

            public static void Subtract(ref MyMatrix a, ref MyMatrix b, out MyMatrix result)
            {
                if (a.Rows != b.Rows || a.Cols != b.Cols)
                    throw new ArgumentException("Matrix dimensions must match");

                result = new MyMatrix(a.Rows, a.Cols);
                for (int i = 0; i < a.Rows; i++)
                    for (int j = 0; j < a.Cols; j++)
                        result[i, j] = a[i, j] - b[i, j];
            }

            public static void Scale(ref MyMatrix m, double scalar, out MyMatrix result)
            {
                result = new MyMatrix(m.Rows, m.Cols);
                for (int i = 0; i < m.Rows; i++)
                    for (int j = 0; j < m.Cols; j++)
                        result[i, j] = m[i, j] * scalar;
            }

            // ============================================================================
            // INVERSE AND PSEUDO-INVERSE
            // ============================================================================

            public static void InverseGaussJordan(ref MyMatrix matrix, out MyMatrix result)
            {
                if (matrix.Rows != matrix.Cols)
                    throw new ArgumentException("Matrix must be square");

                int n = matrix.Rows;

                // Create augmented matrix [A | I]
                double[,] aug = new double[6, 12];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                        aug[i, j] = matrix[i, j];
                    aug[i, 6 + i] = 1.0;
                }

                // Forward elimination with partial pivoting
                for (int col = 0; col < n; col++)
                {
                    // Find pivot
                    int pivotRow = col;
                    double maxVal = Math.Abs(aug[col, col]);
                    for (int i = col + 1; i < n; i++)
                    {
                        if (Math.Abs(aug[i, col]) > maxVal)
                        {
                            maxVal = Math.Abs(aug[i, col]);
                            pivotRow = i;
                        }
                    }

                    if (Math.Abs(maxVal) < 1e-10)
                        throw new InvalidOperationException("Matrix is singular");

                    // Swap rows
                    if (pivotRow != col)
                    {
                        for (int j = 0; j < 12; j++)
                        {
                            double temp = aug[col, j];
                            aug[col, j] = aug[pivotRow, j];
                            aug[pivotRow, j] = temp;
                        }
                    }

                    // Scale pivot row
                    double pivot = aug[col, col];
                    for (int j = 0; j < 12; j++)
                        aug[col, j] /= pivot;

                    // Eliminate column
                    for (int i = 0; i < n; i++)
                    {
                        if (i != col)
                        {
                            double factor = aug[i, col];
                            for (int j = 0; j < 12; j++)
                                aug[i, j] -= factor * aug[col, j];
                        }
                    }
                }

                // Extract inverse
                result = new MyMatrix(n, n);
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        result[i, j] = aug[i, 6 + j];
            }

            public static void DampedPseudoInverseWide(ref MyMatrix J, out MyMatrix result, double lambda = 0.01)
            {
                // For wide matrix (Rows ≤ Cols): J† = J^T(JJ^T + λ²I)^(-1)
                // Input: Rows × Cols, Output: Cols × Rows

                if (J.Rows > J.Cols)
                    throw new ArgumentException("Use DampedPseudoInverseTall for tall matrices");

                // Compute J^T (Cols × Rows)
                MyMatrix JT;
                Transpose(ref J, out JT);

                // Compute JJ^T (Rows × Rows)
                MyMatrix JJT;
                Multiply(ref J, ref JT, out JJT);

                // Add damping: JJ^T + λ²I
                for (int i = 0; i < JJT.Rows; i++)
                    JJT[i, i] += lambda * lambda;

                // Invert (Rows × Rows)
                MyMatrix invJJT;
                InverseGaussJordan(ref JJT, out invJJT);

                // J^T × invJJT (Cols × Rows)
                Multiply(ref JT, ref invJJT, out result);
            }

            public static void DampedPseudoInverseTall(ref MyMatrix J, out MyMatrix result, double lambda = 0.01)
            {
                // For tall matrix (Rows > Cols): J† = (J^T J + λ²I)^(-1) J^T
                // Input: Rows × Cols, Output: Cols × Rows

                if (J.Rows <= J.Cols)
                    throw new ArgumentException("Use DampedPseudoInverseWide for wide/square matrices");

                // Compute J^T (Cols × Rows)
                MyMatrix JT;
                Transpose(ref J, out JT);

                // Compute J^T J (Cols × Cols)
                MyMatrix JTJ;
                Multiply(ref JT, ref J, out JTJ);

                // Add damping: J^T J + λ²I
                for (int i = 0; i < JTJ.Rows; i++)
                    JTJ[i, i] += lambda * lambda;

                // Invert (Cols × Cols)
                MyMatrix invJTJ;
                InverseGaussJordan(ref JTJ, out invJTJ);

                // invJTJ × J^T (Cols × Rows)
                Multiply(ref invJTJ, ref JT, out result);
            }

            public static void DampedWeightedPseudoInverseWide(ref MyMatrix J, out MyMatrix result,
                ref MyVector taskWeights, ref MyVector jointWeights, double lambda = 0.01)
            {
                if (J.Rows > J.Cols)
                    throw new ArgumentException("Use DampedWeightedPseudoInverseTall for tall matrices");
                if (taskWeights.Length != J.Rows || jointWeights.Length != J.Cols)
                    throw new ArgumentException("Weight vector lengths must match matrix dimensions");

                // Create Wj_inv diagonal matrix (Cols × Cols)
                MyMatrix Wj_inv = MyMatrix.Zero(J.Cols, J.Cols);
                for (int i = 0; i < J.Cols; i++)
                    Wj_inv[i, i] = 1.0 / jointWeights[i];

                // Compute J × Wj_inv (Rows × Cols)
                MyMatrix J_Wji;
                Multiply(ref J, ref Wj_inv, out J_Wji);

                // Compute J^T (Cols × Rows)
                MyMatrix JT;
                Transpose(ref J, out JT);

                // Compute J × Wj_inv × J^T (Rows × Rows)
                MyMatrix J_Wji_JT;
                Multiply(ref J_Wji, ref JT, out J_Wji_JT);

                // Add damping: J × Wj_inv × J^T + λ² Wt^(-1)
                for (int i = 0; i < J.Rows; i++)
                    J_Wji_JT[i, i] += lambda * lambda / taskWeights[i];

                // Invert (Rows × Rows)
                MyMatrix invTerm;
                InverseGaussJordan(ref J_Wji_JT, out invTerm);

                // Compute Wj_inv × J^T (Cols × Rows)
                MyMatrix Wji_JT;
                Multiply(ref Wj_inv, ref JT, out Wji_JT);

                // Final: Wj_inv × J^T × invTerm (Cols × Rows)
                Multiply(ref Wji_JT, ref invTerm, out result);
            }

            public static void DampedWeightedPseudoInverseTall(ref MyMatrix J, out MyMatrix result,
                ref MyVector taskWeights, ref MyVector jointWeights, double lambda = 0.01)
            {
                if (J.Rows <= J.Cols)
                    throw new ArgumentException("Use DampedWeightedPseudoInverseWide for wide/square matrices");
                if (taskWeights.Length != J.Rows || jointWeights.Length != J.Cols)
                    throw new ArgumentException("Weight vector lengths must match matrix dimensions");

                // Create Wt diagonal matrix (Rows × Rows)
                MyMatrix Wt = MyMatrix.Zero(J.Rows, J.Rows);
                for (int i = 0; i < J.Rows; i++)
                    Wt[i, i] = taskWeights[i];

                // Compute J^T (Cols × Rows)
                MyMatrix JT;
                Transpose(ref J, out JT);

                // Compute J^T × Wt (Cols × Rows)
                MyMatrix JT_Wt;
                Multiply(ref JT, ref Wt, out JT_Wt);

                // Compute J^T × Wt × J (Cols × Cols)
                MyMatrix JT_Wt_J;
                Multiply(ref JT_Wt, ref J, out JT_Wt_J);

                // Add damping: J^T × Wt × J + λ² Wj
                for (int i = 0; i < J.Cols; i++)
                    JT_Wt_J[i, i] += lambda * lambda * jointWeights[i];

                // Invert (Cols × Cols)
                MyMatrix invTerm;
                InverseGaussJordan(ref JT_Wt_J, out invTerm);

                // Compute invTerm × J^T (Cols × Rows)
                MyMatrix invTerm_JT;
                Multiply(ref invTerm, ref JT, out invTerm_JT);

                // Final: invTerm × J^T × Wt (Cols × Rows)
                Multiply(ref invTerm_JT, ref Wt, out result);
            }

            public static void NullSpaceProjector(ref MyMatrix J, ref MyMatrix J_pi, out MyMatrix result)
            {
                // N = I - J†J
                // Result is Cols × Cols (joint space)

                if (J.Cols != J_pi.Rows)
                    throw new ArgumentException("Incompatible dimensions for null space projector");

                // Compute J†J (Cols × Cols)
                MyMatrix J_pi_J;
                Multiply(ref J_pi, ref J, out J_pi_J);

                // N = I - J†J
                result = MyMatrix.Identity(J.Cols);
                Subtract(ref result, ref J_pi_J, out result);
            }

            // ============================================================================
            // UTILITY OPERATIONS
            // ============================================================================

            public static void SetRow(ref MyMatrix m, int row, ref MyVector data)
            {
                if (data.Length != m.Cols)
                    throw new ArgumentException("Vector length must match matrix columns");

                for (int j = 0; j < m.Cols; j++)
                    m[row, j] = data[j];
            }

            public static void GetRow(ref MyMatrix m, int row, out MyVector result)
            {
                result = new MyVector(m.Cols);
                for (int j = 0; j < m.Cols; j++)
                    result[j] = m[row, j];
            }

            public static void SetColumn(ref MyMatrix m, int col, ref MyVector data)
            {
                if (data.Length != m.Rows)
                    throw new ArgumentException("Vector length must match matrix rows");

                for (int i = 0; i < m.Rows; i++)
                    m[i, col] = data[i];
            }

            public static void GetColumn(ref MyMatrix m, int col, out MyVector result)
            {
                result = new MyVector(m.Rows);
                for (int i = 0; i < m.Rows; i++)
                    result[i] = m[i, col];
            }
        }
    }
}
