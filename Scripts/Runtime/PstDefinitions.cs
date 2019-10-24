using System;
using pst.REST;
using pst.Utility;
using UnityEngine;

namespace pst
{
    // only exposing relevant data, in "Unity-style"

    public class PstTrackerData
    {
        /// <summary>
        ///     A frames per second related sequential number. E.g. when the fps is set to 30 it is in range [0,30].
        /// </summary>
        private int m_sequenceNumber;

        private TargetPose[] m_targetPoses;
        private double       m_timestamp;

        public PstTrackerData(int sequenceNumber, double timestamp, TargetPose[] targetPoses)
        {
            m_sequenceNumber = sequenceNumber;
            m_timestamp      = timestamp;
            m_targetPoses    = targetPoses;
        }

        /// <summary>
        ///     Constructor to convert to this exposed format.
        /// </summary>
        internal PstTrackerData(TrackerData td)
        {
            m_sequenceNumber = td.seqnumber;
            m_timestamp      = td.timestamp;
            m_targetPoses    = new TargetPose[td.targetPoses.Length];

            for (int i = 0; i < td.targetPoses.Length; i++)
            {
                m_targetPoses[i] = new TargetPose(td.targetPoses[i].targetPose);
            }
        }

        public TargetPose[] TargetPoses    => m_targetPoses;
        public double       Timestamp      => m_timestamp;
        public int          SequenceNumber => m_sequenceNumber;
    }

    public struct TargetPose
    {
        public int    id;
        public string name;

        public Vector3    position;
        public Quaternion rotation;

        public TargetPose(int id, string name, Vector3 position, Quaternion rotation)
        {
            this.id       = id;
            this.name     = name;
            this.position = position;
            this.rotation = rotation;
        }

        /// <summary>
        ///     Constructor to convert to this exposed format.
        /// </summary>
        internal TargetPose(REST.TargetPose tp)
        {
            id       = tp.id;
            name     = tp.name;
            position = tp.transformationMatrix.GetPosition();
            rotation = tp.transformationMatrix.GetRotation();
        }

        /// <summary>
        ///     Converting from a left-handed coordinate system (PST) to a right-handed coordinate system (Unity).
        /// </summary>
        internal void ToUnityCoordinateSystem()
        {
            position.ToggleCoordinateSystemRef();
            rotation.ToggleCoordinateSystemRef();
        }
    }
}

// @formatter:off
namespace pst.REST
{
    // classes used for (de)serialization of the PST REST requests 
    
    [Serializable]
    internal class TrackerDataWrapper
    {
        public TrackerData trackerData;
    }

    [Serializable]
    internal class TrackerData
    {
        public PointWrapper[]      points;
        public int                 seqnumber;
        public TargetPoseWrapper[] targetPoses;
        public double              timestamp;
    }

    [Serializable]
    internal class PointWrapper
    {
        public Point dataPoint;
    }
    
    [Serializable]
    internal class Position
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    internal class Point
    {
        public int      id;
        public Position position;
    }

    [Serializable]
    internal class TargetPoseWrapper
    {
        public TargetPose targetPose;
    }

    [Serializable]
    internal class TargetPose
    {
        public int     id;
        public string  name;
        public float[] transformationMatrix;
        public string  uuid;
    }
}
// @formatter:on
