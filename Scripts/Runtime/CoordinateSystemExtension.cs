using UnityEngine;

namespace pst
{
    /// <summary>
    ///     See e.g. <a href="https://www.evl.uic.edu/ralph/508S98/coordinates.html">here</a>.
    /// </summary>
    internal static class CoordinateSystemExtension
    {
        /// <summary>
        ///     The z-axis of the given vector <paramref name="v" /> if flipped. Depending on the initial coordinate system
        ///     a right-handed vector is transformed into a left-handed vector and vice versa.
        /// </summary>
        public static void ToggleCoordinateSystemRef( ref this Vector3 v )
        {
            v.z = -v.z;
        }

        /// <summary>
        ///     The z-axis of the given vector <paramref name="v" /> if flipped. Depending on the initial coordinate system
        ///     a right-handed vector is transformed into a left-handed vector and vice versa.
        /// </summary>
        public static Vector3 ToggleCoordinateSystem( this Vector3 v )
        {
            return new Vector3( v.x, v.y, -v.z );
        }

        /// <summary>
        ///     The z-axis of the given quaternion <paramref name="q" /> if flipped. Depending on the initial coordinate system
        ///     a right-handed quaternion is transformed into a left-handed quaternion and vice versa.
        /// </summary>
        public static void ToggleCoordinateSystemRef( ref this Quaternion q )
        {
            q.x = -q.x;
            q.y = -q.y;
        }

        /// <summary>
        ///     The z-axis of the given quaternion <paramref name="q" /> if flipped. Depending on the initial coordinate system
        ///     a right-handed quaternion is transformed into a left-handed quaternion and vice versa.
        /// </summary>
        public static Quaternion ToggleCoordinateSystem( this Quaternion q )
        {
            return new Quaternion( -q.x, -q.y, q.z, q.w );
        }
    }
}
