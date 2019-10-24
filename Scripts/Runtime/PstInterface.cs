using pst.Utility;
using UnityEngine;

namespace pst
{
    /// <summary>
    ///     The interface used in Unity, encapsulating all the PST REST API.
    /// </summary>
    public class PstInterface : MonoBehaviour
    {
        private PstController         m_pstController;
        private PstTrackerDataHandler m_pstTrackerDataHandler;

        public PstController PSTController => m_pstController;

        protected virtual void Awake()
        {
            m_pstController = new PstController(
                m_pollIntervalInMilliseconds, m_trackerDataStreamBufferSizeInBytes,
                m_logSingleResponses, m_logContinuousResponses);

            m_pstTrackerDataHandler          =  new PstTrackerDataHandler();
            m_pstController.OnNewTrackerData += m_pstTrackerDataHandler.ProcessTrackerDataString;

            m_pstController.Start();
        }

        [ContextMenu(nameof(Start))]
        protected virtual void Start()
        {
            m_pstController.StartTrackerDataStream();
        }

        [ContextMenu("Close")]
        protected virtual void OnApplicationQuit()
        {
            m_pstController.OnNewTrackerData -= m_pstTrackerDataHandler.ProcessTrackerDataString;

            m_pstController.CloseStreams();
            m_pstController.Pause();
        }

        /// <summary>
        ///     The returned pose is transformed s.t. it can be directly used in Unity.
        /// </summary>
        public TargetPose GetLatestTargetPoseOf(string targetName)
        {
            TargetPose pose = m_pstTrackerDataHandler.GetLatestTargetPoseOf(targetName);
            pose.ToUnityCoordinateSystem();
            return pose;
        }

        /// <summary>
        ///     The returned pose is transformed s.t. it can be directly used in Unity.
        /// </summary>
        public TargetPose GetLatestTargetPoseOf(int targetId)
        {
            TargetPose pose = m_pstTrackerDataHandler.GetLatestTargetPoseOf(targetId);
            pose.ToUnityCoordinateSystem();
            return pose;
        }

        /// <summary>
        ///     Returns the position and rotation of the reference.
        /// </summary>
        public (Vector3, Quaternion) GetReferencePose()
        {
            Matrix4x4  m = m_pstController.GetReference();
            Vector3    p = m.GetPosition();
            Quaternion r = m.GetRotation();

            // z-axis is forward, x-axis is parallel to the tracker
            // position and rotation are transformed from a left-handed (PST) to a right-handed (Unity) coordinate system

            return (
                p.ToggleCoordinateSystem(),
                r.ToggleCoordinateSystem()
            );
        }

        public void SetReferencePose(TargetPose pose)
        {
            SetReferencePose(pose.position, pose.rotation);
        }

        public void SetReferencePose(Vector3 position, Quaternion rotation)
        {
            SetReferencePose(position, rotation, Vector3.one);
        }

        /// <summary>
        ///     Creates a translation/rotation/scale matrix from the given parameters and via the PST REST API sets this matrix as
        ///     new reference matrix. Note that this method encapsulates the transformation of <paramref name="position" /> and
        ///     <paramref name="rotation" /> from a right-handed (Unity) to a left-handed (PST) coordinate system.
        /// </summary>
        public void SetReferencePose(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix4x4 m = new Matrix4x4();
            m.SetTRS(position.ToggleCoordinateSystem(), rotation.ToggleCoordinateSystem(), scale);
            m_pstController.SetReference(m);
        }

#region Serialized Field

        [SerializeField]                      private bool m_logSingleResponses;
        [SerializeField]                      private bool m_logContinuousResponses;
        [Range(2, 100)] [SerializeField]      private int  m_pollIntervalInMilliseconds         = 10;
        [Range(1024, 10240)] [SerializeField] private int  m_trackerDataStreamBufferSizeInBytes = 4096;

#endregion
    }
}
