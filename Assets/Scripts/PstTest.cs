using System;
using System.Collections.Generic;
using pst;
using UnityEngine;

public class PstTest : MonoBehaviour
{
    [SerializeField] private List<Mapping> m_mappedPstTargets;

    private PstInterface m_pstInterface;

    private void Start()
    {
        m_pstInterface = GetComponent<PstInterface>();
    }

    private void Update()
    {
        for (int i = 0; i < m_mappedPstTargets.Count; i++)
        {
            SetPose(
                m_mappedPstTargets[i].relatedGameObject,
                m_pstInterface.GetLatestTargetPoseOf(m_mappedPstTargets[i].pstTargetName)
            );
        }
    }

    private void SetPose(Transform t, TargetPose p)
    {
        t.SetPositionAndRotation(p.position, p.rotation);
    }

    [Serializable]
    public class Mapping
    {
        public string    pstTargetName;
        public Transform relatedGameObject;
    }
}
