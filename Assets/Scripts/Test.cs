using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using pst;
using Simteam.Utility.ExtensionMethods;
using UnityEngine;

public class Test : MonoBehaviour
{
    public  GameObject   g1, g2;
    private PstInterface m_pstInterface;

    // Start is called before the first frame update
    private void Start()
    {
        m_pstInterface = GetComponent<PstInterface>();

        // m_pstInterface.PSTController.SetDefaultReference();
        // m_pstInterface.PSTController.GetSupportedFramerates().PrintElements( x => x + " " );
        // Debug.Log( m_pstInterface.PSTController.GetReference() );
        // (double d1, double d2) = m_pstInterface.PSTController.GetExposureRange();
        // Debug.Log(
        //     $"{d1.ToString( NumberFormatInfo.InvariantInfo )}, {d2.ToString( NumberFormatInfo.InvariantInfo )}" );
        // m_pstInterface.PSTController.SetFramerate( 30 );
        // Debug.Log( m_pstInterface.PSTController.GetFramerate() );
        //
        // double e = m_pstInterface.PSTController.GetExposure();
        // m_pstInterface.PSTController.SetExposure( e );
        // Debug.Log( m_pstInterface.PSTController.GetExposure() );
    }

    // Update is called once per frame
    private void Update()
    {
        // Debug.Log(
        //     $"{m_pstInterface.GetLatestTargetPoseOf( "Reference" ).position} " +
        //     $"{m_pstInterface.GetLatestTargetPoseOf( "Reference" ).rotation}" );

        SetPose( g1.transform, m_pstInterface.GetLatestTargetPoseOf( "Reference" ) );
        SetPose( g2.transform, m_pstInterface.GetLatestTargetPoseOf( "Torch (11 LEDs)" ) );

        // Tuple<Vector3, Quaternion> tmp = m_pstInterface.GetReferencePose();
        // Debug.Log( $"{tmp.Item1}, {tmp.Item2.eulerAngles}" );
    }

    private void SetPose( Transform t, TargetPose p )
    {
        t.SetPositionAndRotation( p.position, p.rotation );
    }
}
