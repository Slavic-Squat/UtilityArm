using System;

namespace IngameScript
{
    partial class Program
    {
        public static class MyMathNoAlloc
        {
            // ============================================================================
            // MATRIX INVERSION
            // ============================================================================

            public static void InverseGaussJordan(double[,] matrix, double[,] result)
            {
                int n = matrix.GetLength(0);
                if (n != matrix.GetLength(1))
                    throw new ArgumentException("Matrix must be square");
                if (result.GetLength(0) != n || result.GetLength(1) != n)
                    throw new ArgumentException("Result matrix must match input dimensions");

                // Create augmented matrix [A | I] - still need temp allocation here
                double[,] aug = new double[n, 2 * n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                        aug[i, j] = matrix[i, j];
                    aug[i, n + i] = 1.0;
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
                        throw new InvalidOperationException("Matrix is singular or near-singular");

                    // Swap rows
                    if (pivotRow != col)
                    {
                        for (int j = 0; j < 2 * n; j++)
                        {
                            double temp = aug[col, j];
                            aug[col, j] = aug[pivotRow, j];
                            aug[pivotRow, j] = temp;
                        }
                    }

                    // Scale pivot row
                    double pivot = aug[col, col];
                    for (int j = 0; j < 2 * n; j++)
                        aug[col, j] /= pivot;

                    // Eliminate column
                    for (int i = 0; i < n; i++)
                    {
                        if (i != col)
                        {
                            double factor = aug[i, col];
                            for (int j = 0; j < 2 * n; j++)
                                aug[i, j] -= factor * aug[col, j];
                        }
                    }
                }

                // Extract inverse from right half into result
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        result[i, j] = aug[i, n + j];
            }

            // ============================================================================
            // DAMPED LEAST SQUARES
            // ============================================================================

            public static void DampedPseudoInverseWide(double[,] J, double[,] result, double lambda = 0.01,
                double[,] tempJJT = null, double[,] tempJT = null, double[,] tempInv = null)
            {
                int m = J.GetLength(0);
                int n = J.GetLength(1);

                if (result.GetLength(0) != n || result.GetLength(1) != m)
                    throw new ArgumentException($"Result must be {n}x{m}");

                // Allocate temps if not provided
                if (tempJT == null) tempJT = new double[n, m];
                if (tempJJT == null) tempJJT = new double[m, m];
                if (tempInv == null) tempInv = new double[m, m];

                // Compute J^T
                Transpose(J, tempJT);

                // Compute JJ^T
                MultiplyMatrices(J, tempJT, tempJJT);

                // Add damping: JJ^T + λ²I
                for (int i = 0; i < m; i++)
                    tempJJT[i, i] += lambda * lambda;

                // Invert
                InverseGaussJordan(tempJJT, tempInv);

                // Compute J^T * inv
                MultiplyMatrices(tempJT, tempInv, result);
            }

            public static void DampedPseudoInverseTall(double[,] J, double[,] result, double lambda = 0.01,
                double[,] tempJTJ = null, double[,] tempJT = null, double[,] tempInv = null)
            {
                int m = J.GetLength(0);
                int n = J.GetLength(1);

                if (result.GetLength(0) != n || result.GetLength(1) != m)
                    throw new ArgumentException($"Result must be {n}x{m}");

                // Allocate temps if not provided
                if (tempJT == null) tempJT = new double[n, m];
                if (tempJTJ == null) tempJTJ = new double[n, n];
                if (tempInv == null) tempInv = new double[n, n];

                // Compute J^T
                Transpose(J, tempJT);

                // Compute J^T*J
                MultiplyMatrices(tempJT, J, tempJTJ);

                // Add damping
                for (int i = 0; i < n; i++)
                    tempJTJ[i, i] += lambda * lambda;

                // Invert
                InverseGaussJordan(tempJTJ, tempInv);

                // Compute inv * J^T
                MultiplyMatrices(tempInv, tempJT, result);
            }

            public static void DampedWeightedPseudoInverseTall(double[,] J, double[,] result,
                double[] taskWeights = null, double[] jointWeights = null, double lambda = 0.01,
                double[,] tempWt = null, double[,] tempJT = null, double[,] tempJT_Wt = null,
                double[,] tempJT_Wt_J = null, double[,] tempInv = null, double[,] tempInv_JT = null)
            {
                int m = J.GetLength(0); // rows (tasks)
                int n = J.GetLength(1); // cols (joints)

                if (result.GetLength(0) != n || result.GetLength(1) != m)
                    throw new ArgumentException($"Result must be {n}x{m}");

                // Allocate temps if not provided
                if (tempWt == null) tempWt = new double[m, m];
                if (tempJT == null) tempJT = new double[n, m];
                if (tempJT_Wt == null) tempJT_Wt = new double[n, m];
                if (tempJT_Wt_J == null) tempJT_Wt_J = new double[n, n];
                if (tempInv == null) tempInv = new double[n, n];
                if (tempInv_JT == null) tempInv_JT = new double[n, m];

                // Create task weight matrix (diagonal)
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        if (i == j)
                            tempWt[i, j] = (taskWeights != null && i < taskWeights.Length) ? taskWeights[i] : 1.0;
                        else
                            tempWt[i, j] = 0;
                    }
                }

                // Compute J^T
                Transpose(J, tempJT);

                // Compute J^T * Wt
                MultiplyMatrices(tempJT, tempWt, tempJT_Wt);

                // Compute J^T * Wt * J
                MultiplyMatrices(tempJT_Wt, J, tempJT_Wt_J);

                // Add damping: J^T * Wt * J + λ² * Wj (diagonal)
                for (int i = 0; i < n; i++)
                {
                    double jointWeight = (jointWeights != null && i < jointWeights.Length) ? jointWeights[i] : 1.0;
                    tempJT_Wt_J[i, i] += lambda * lambda * jointWeight;
                }

                // Invert
                InverseGaussJordan(tempJT_Wt_J, tempInv);

                // Compute invTerm * J^T
                MultiplyMatrices(tempInv, tempJT, tempInv_JT);

                // Compute final: invTerm * J^T * Wt
                MultiplyMatrices(tempInv_JT, tempWt, result);
            }

            public static void DampedWeightedPseudoInverseWide(double[,] J, double[,] result,
                double[] taskWeights = null, double[] jointWeights = null, double lambda = 0.01,
                double[,] tempWj_inv = null, double[,] tempJ_Wji = null, double[,] tempJ_Wji_JT = null,
                double[,] tempInv = null, double[,] tempJT = null, double[,] tempWji_JT = null)
            {
                int m = J.GetLength(0); // rows (tasks)
                int n = J.GetLength(1); // cols (joints)

                if (result.GetLength(0) != n || result.GetLength(1) != m)
                    throw new ArgumentException($"Result must be {n}x{m}");

                // Allocate temps if not provided
                if (tempWj_inv == null) tempWj_inv = new double[n, n];
                if (tempJ_Wji == null) tempJ_Wji = new double[m, n];
                if (tempJT == null) tempJT = new double[n, m];
                if (tempJ_Wji_JT == null) tempJ_Wji_JT = new double[m, m];
                if (tempInv == null) tempInv = new double[m, m];
                if (tempWji_JT == null) tempWji_JT = new double[n, m];

                // Create joint weight inverse matrix (diagonal)
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i == j)
                        {
                            double jointWeight = (jointWeights != null && i < jointWeights.Length) ? jointWeights[i] : 1.0;
                            tempWj_inv[i, j] = 1.0 / jointWeight;
                        }
                        else
                        {
                            tempWj_inv[i, j] = 0;
                        }
                    }
                }

                // Compute J * Wj_inv
                MultiplyMatrices(J, tempWj_inv, tempJ_Wji);

                // Compute J^T
                Transpose(J, tempJT);

                // Compute J * Wj_inv * J^T
                MultiplyMatrices(tempJ_Wji, tempJT, tempJ_Wji_JT);

                // Add damping: J * Wj_inv * J^T + λ² * Wt_inv (diagonal)
                for (int i = 0; i < m; i++)
                {
                    double taskWeight = (taskWeights != null && i < taskWeights.Length) ? taskWeights[i] : 1.0;
                    tempJ_Wji_JT[i, i] += lambda * lambda / taskWeight;
                }

                // Invert
                InverseGaussJordan(tempJ_Wji_JT, tempInv);

                // Compute Wj_inv * J^T
                MultiplyMatrices(tempWj_inv, tempJT, tempWji_JT);

                // Compute final: Wj_inv * J^T * invTerm
                MultiplyMatrices(tempWji_JT, tempInv, result);
            }

            public static void NullSpaceProjector(double[,] J, double[,] J_pi, double[,] result, double[,] tempProduct = null)
            {
                int n = J.GetLength(1);

                if (result.GetLength(0) != n || result.GetLength(1) != n)
                    throw new ArgumentException($"Result must be {n}x{n}");

                if (tempProduct == null) tempProduct = new double[n, n];

                // Compute J†J
                MultiplyMatrices(J_pi, J, tempProduct);

                // N = I - J†J
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        result[i, j] = (i == j ? 1.0 : 0.0) - tempProduct[i, j];
                    }
                }
            }

            // ============================================================================
            // BASIC MATRIX OPERATIONS
            // ============================================================================

            public static void MultiplyMatrices(double[,] A, double[,] B, double[,] result)
            {
                int rowsA = A.GetLength(0);
                int colsA = A.GetLength(1);
                int rowsB = B.GetLength(0);
                int colsB = B.GetLength(1);

                if (colsA != rowsB)
                    throw new ArgumentException("Matrix dimensions don't match for multiplication");
                if (result.GetLength(0) != rowsA || result.GetLength(1) != colsB)
                    throw new ArgumentException($"Result must be {rowsA}x{colsB}");

                for (int i = 0; i < rowsA; i++)
                {
                    for (int j = 0; j < colsB; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < colsA; k++)
                            sum += A[i, k] * B[k, j];
                        result[i, j] = sum;
                    }
                }
            }

            public static void MultiplyScalar(double[,] matrix, double scalar, double[,] result)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                if (result.GetLength(0) != rows || result.GetLength(1) != cols)
                    throw new ArgumentException("Result must match matrix dimensions");

                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                        result[i, j] = matrix[i, j] * scalar;
            }

            public static void MultiplyScalar(double[] vector, double scalar, double[] result)
            {
                int length = vector.Length;

                if (result.Length != length)
                    throw new ArgumentException("Result must match vector length");

                for (int i = 0; i < length; i++)
                    result[i] = vector[i] * scalar;
            }

            public static void AddMatrices(double[,] A, double[,] B, double[,] result)
            {
                int rows = A.GetLength(0);
                int cols = A.GetLength(1);

                if (rows != B.GetLength(0) || cols != B.GetLength(1))
                    throw new ArgumentException("Matrix dimensions don't match");
                if (result.GetLength(0) != rows || result.GetLength(1) != cols)
                    throw new ArgumentException("Result must match matrix dimensions");

                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                        result[i, j] = A[i, j] + B[i, j];
            }

            public static void SubtractMatrices(double[,] A, double[,] B, double[,] result)
            {
                int rows = A.GetLength(0);
                int cols = A.GetLength(1);

                if (rows != B.GetLength(0) || cols != B.GetLength(1))
                    throw new ArgumentException("Matrix dimensions don't match");
                if (result.GetLength(0) != rows || result.GetLength(1) != cols)
                    throw new ArgumentException("Result must match matrix dimensions");

                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                        result[i, j] = A[i, j] - B[i, j];
            }

            public static void Transpose(double[,] matrix, double[,] result)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                if (result.GetLength(0) != cols || result.GetLength(1) != rows)
                    throw new ArgumentException($"Result must be {cols}x{rows}");

                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                        result[j, i] = matrix[i, j];
            }

            public static void MultiplyMatrixVector(double[,] matrix, double[] vector, double[] result)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                if (cols != vector.Length)
                    throw new ArgumentException("Dimensions don't match");
                if (result.Length != rows)
                    throw new ArgumentException($"Result must have length {rows}");

                for (int i = 0; i < rows; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < cols; j++)
                        sum += matrix[i, j] * vector[j];
                    result[i] = sum;
                }
            }

            public static void MultiplyVectorMatrix(double[] vector, double[,] matrix, double[] result)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                if (vector.Length != rows)
                    throw new ArgumentException("Dimensions don't match");
                if (result.Length != cols)
                    throw new ArgumentException($"Result must have length {cols}");

                for (int j = 0; j < cols; j++)
                {
                    double sum = 0;
                    for (int i = 0; i < rows; i++)
                        sum += vector[i] * matrix[i, j];
                    result[j] = sum;
                }
            }

            public static void MultiplyVectorVector(double[] a, double[] b, double[] result)
            {
                if (a.Length != b.Length)
                    throw new ArgumentException("Vectors must be the same length");
                if (result.Length != a.Length)
                    throw new ArgumentException("Result must match vector length");

                for (int i = 0; i < a.Length; i++)
                    result[i] = a[i] * b[i];
            }

            public static void DivideVectorVector(double[] a, double[] b, double[] result)
            {
                if (a.Length != b.Length)
                    throw new ArgumentException("Vectors must be the same length");
                if (result.Length != a.Length)
                    throw new ArgumentException("Result must match vector length");

                for (int i = 0; i < a.Length; i++)
                {
                    if (Math.Abs(b[i]) < 1e-10)
                        throw new DivideByZeroException("Cannot divide by zero");
                    result[i] = a[i] / b[i];
                }
            }

            public static void AddVectors(double[] a, double[] b, double[] result)
            {
                if (a.Length != b.Length)
                    throw new ArgumentException("Vectors must be the same length");
                if (result.Length != a.Length)
                    throw new ArgumentException("Result must match vector length");

                for (int i = 0; i < a.Length; i++)
                    result[i] = a[i] + b[i];
            }

            public static void SubtractVectors(double[] a, double[] b, double[] result)
            {
                if (a.Length != b.Length)
                    throw new ArgumentException("Vectors must be the same length");
                if (result.Length != a.Length)
                    throw new ArgumentException("Result must match vector length");

                for (int i = 0; i < a.Length; i++)
                    result[i] = a[i] - b[i];
            }

            public static void ReciprocalVector(double[] vector, double[] result)
            {
                if (result.Length != vector.Length)
                    throw new ArgumentException("Result must match vector length");

                for (int i = 0; i < vector.Length; i++)
                {
                    if (Math.Abs(vector[i]) < 1e-10)
                        throw new DivideByZeroException("Cannot take reciprocal of zero");
                    result[i] = 1.0 / vector[i];
                }
            }

            // ============================================================================
            // READ-ONLY OPERATIONS (don't need result parameter)
            // ============================================================================

            public static double DotProduct(double[] a, double[] b)
            {
                if (a.Length != b.Length)
                    throw new ArgumentException("Vectors must be the same length");

                double result = 0;
                for (int i = 0; i < a.Length; i++)
                    result += a[i] * b[i];
                return result;
            }

            public static double MaxElement(double[] vector)
            {
                double max = double.NegativeInfinity;
                for (int i = 0; i < vector.Length; i++)
                    if (vector[i] > max) max = vector[i];
                return max;
            }

            public static double MinElement(double[] vector)
            {
                double min = double.PositiveInfinity;
                for (int i = 0; i < vector.Length; i++)
                    if (vector[i] < min) min = vector[i];
                return min;
            }

            public static double Norm(double[] vector)
            {
                double sum = 0;
                for (int i = 0; i < vector.Length; i++)
                    sum += vector[i] * vector[i];
                return Math.Sqrt(sum);
            }

            // ============================================================================
            // GET/SET OPERATIONS
            // ============================================================================

            public static void GetRow(double[,] matrix, int rowIndex, double[] result)
            {
                int cols = matrix.GetLength(1);
                if (result.Length != cols)
                    throw new ArgumentException($"Result must have length {cols}");

                for (int j = 0; j < cols; j++)
                    result[j] = matrix[rowIndex, j];
            }

            public static void SetRow(double[,] matrix, int rowIndex, double[] rowData)
            {
                int cols = matrix.GetLength(1);
                if (rowData.Length != cols)
                    throw new ArgumentException("Row data length must match columns");

                for (int j = 0; j < cols; j++)
                    matrix[rowIndex, j] = rowData[j];
            }

            public static void GetColumn(double[,] matrix, int colIndex, double[] result)
            {
                int rows = matrix.GetLength(0);
                if (result.Length != rows)
                    throw new ArgumentException($"Result must have length {rows}");

                for (int i = 0; i < rows; i++)
                    result[i] = matrix[i, colIndex];
            }

            public static void SetColumn(double[,] matrix, int colIndex, double[] colData)
            {
                int rows = matrix.GetLength(0);
                if (colData.Length != rows)
                    throw new ArgumentException("Column data length must match rows");

                for (int i = 0; i < rows; i++)
                    matrix[i, colIndex] = colData[i];
            }
        }
    }
}