using System;
using UnityEngine;

namespace pst.Utility
{
    internal static class Matrix4x4Extensions
    {
        private static float[] Values(this Vector4 v)
        {
            return new[] { v.x, v.y, v.z, v.w };
        }

        public static Matrix4x4 FromRowMajor(this float[] m)
        {
            Matrix4x4 tmp = new Matrix4x4();
            for (int i = 0; i < 4; i++)
            {
                tmp.SetRow(i, new Vector4(m[i * 4 + 0], m[i * 4 + 1], m[i * 4 + 2], m[i * 4 + 3]));
            }

            return tmp;
        }

        public static Matrix4x4 FromColumnMajor(this float[] m)
        {
            Matrix4x4 tmp = new Matrix4x4();
            for (int i = 0; i < 4; i++)
            {
                tmp.SetColumn(i, new Vector4(m[i * 4 + 0], m[i * 4 + 1], m[i * 4 + 2], m[i * 4 + 3]));
            }

            return tmp;
        }

        public static float[] ToRowMajor(this Matrix4x4 m)
        {
            float[] tmp = new float[16];

            for (int i = 0; i < 4; i++)
            {
                Array.Copy(m.GetRow(i).Values(), 0, tmp, i * 4, 4);
            }

            return tmp;
        }

        public static float[] ToColumnMajor(this Matrix4x4 m)
        {
            float[] tmp = new float[16];

            for (int i = 0; i < 4; i++)
            {
                Array.Copy(m.GetColumn(i).Values(), 0, tmp, i * 4, 4);
            }

            return tmp;
        }

        public static void SetPosition(this ref Matrix4x4 m, Vector3 pos)
        {
            m[0, 3] = pos.x;
            m[1, 3] = pos.y;
            m[2, 3] = pos.z;
        }

        public static Vector3 GetPosition(this Matrix4x4 m)
        {
            return m.GetColumn(3);
        }

        public static Vector3 GetPosition(this float[] m)
        {
            return new Vector3(m[3], m[7], m[11]);
        }

        public static Quaternion GetRotation(this Matrix4x4 m)
        {
            return Quaternion.LookRotation(
                m.GetColumn(2),
                m.GetColumn(1)
            );
        }

        public static Quaternion GetRotation(this float[] m)
        {
            return Quaternion.LookRotation(
                new Vector3(m[2], m[6], m[10]),
                new Vector3(m[1], m[5], m[9]));
        }

        public static Vector3 GetScale(this Matrix4x4 m)
        {
            return new Vector3(
                m.GetColumn(0).magnitude,
                m.GetColumn(1).magnitude,
                m.GetColumn(2).magnitude
            );
        }

        public static Vector3 GetScale(this float[] m)
        {
            return new Vector3(
                new Vector3(m[0], m[4], m[8]).magnitude,
                new Vector3(m[1], m[5], m[9]).magnitude,
                new Vector3(m[2], m[6], m[10]).magnitude);
        }
    }
}
