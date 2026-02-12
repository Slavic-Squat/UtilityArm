using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public struct MyVector
        {
            public double V0, V1, V2, V3, V4, V5;
            public int Length; // Actual length (1-6)

            public MyVector(int length)
            {
                if (length < 1 || length > 6)
                    throw new ArgumentException("Length must be 1-6");
                Length = length;
                V0 = V1 = V2 = V3 = V4 = V5 = 0;
            }

            public double this[int i]
            {
                get
                {
                    if (i < 0 || i >= Length)
                        throw new Exception($"Index {i} out of range [0, {Length})");
                    switch (i)
                    {
                        case 0: return V0;
                        case 1: return V1;
                        case 2: return V2;
                        case 3: return V3;
                        case 4: return V4;
                        case 5: return V5;
                        default: throw new Exception();
                    }
                }
                set
                {
                    if (i < 0 || i >= Length)
                        throw new Exception($"Index {i} out of range [0, {Length})");
                    switch (i)
                    {
                        case 0: V0 = value; break;
                        case 1: V1 = value; break;
                        case 2: V2 = value; break;
                        case 3: V3 = value; break;
                        case 4: V4 = value; break;
                        case 5: V5 = value; break;
                    }
                }
            }

            public static MyVector Zero(int length) => new MyVector(length);
        }
    }
}
