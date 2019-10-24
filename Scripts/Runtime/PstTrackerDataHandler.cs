using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using pst.REST;
using Simteam.Utility.CustomCollections;
using UnityEngine;

namespace pst
{
    internal class PstTrackerDataHandler
    {
        private CircularBuffer<PstTrackerData> m_cachedTrackerData =
            new CircularBuffer<PstTrackerData>( 100 );

        private Dictionary<int, TargetPose> m_latestTargetPoses =
            new Dictionary<int, TargetPose>( 10 );

        private Dictionary<string, int> m_targetNameToId = new Dictionary<string, int>( 10 );

        public void ProcessTrackerDataString( string restResponseContent )
        {
            string[] trackerData = restResponseContent.Split(
                new[] { "\r\n", "data: " }, StringSplitOptions.RemoveEmptyEntries );

            //trackerData.PrintElements();
            for ( int i = 0; i < trackerData.Length; i++ )
            {
                // sanity check
                if ( !trackerData[i].StartsWith( @"{""TrackerData"":" ) )
                    continue;

                try
                {
                    TrackerData td = JsonConvert.DeserializeObject<TrackerDataWrapper>( trackerData[i] ).trackerData;
                    CacheData( td );
                }
                catch ( JsonException e )
                {
                    Debug.LogWarning(
                        PstUtility.GetPstLogMessage(
                            "Consider adjusting the stream's polling interval or buffer size if this happens more often. " +
                            $"Error: {e.Message}." ) );
                }
                catch ( Exception e )
                {
                    Debug.LogWarning( PstUtility.GetPstLogMessage( $"An error occured: {e.Message}." ) );
                }
            }
        }

        private void CacheData( TrackerData internalTrackerData )
        {
            PstTrackerData td = new PstTrackerData( internalTrackerData );
            m_cachedTrackerData.PushBack( td );

            foreach ( TargetPose targetPose in td.TargetPoses )
            {
                if ( !m_latestTargetPoses.ContainsKey( targetPose.id ) )
                {
                    m_targetNameToId.Add( targetPose.name, targetPose.id );
                    m_latestTargetPoses.Add( targetPose.id, targetPose );
                }
                else
                    m_latestTargetPoses[targetPose.id] = targetPose;
            }
        }

        /// <summary>
        ///     Requires the <paramref name="targetName" /> to be exact.
        ///     <para />
        ///     Note that the pose is given in a left-handed coordinate system (PST). Unity uses a right-handed coordinate system.
        /// </summary>
        public TargetPose GetLatestTargetPoseOf( string targetName )
        {
            if ( !m_targetNameToId.ContainsKey( targetName ) )
                return new TargetPose();

            int targetId = m_targetNameToId[targetName];
            return GetLatestTargetPoseOf( targetId );
        }

        /// <summary>
        ///     Note that the pose is given in a left-handed coordinate system (PST). Unity uses a right-handed coordinate system.
        /// </summary>
        public TargetPose GetLatestTargetPoseOf( int targetId )
        {
            if ( !m_latestTargetPoses.ContainsKey( targetId ) )
                return new TargetPose();

            return m_latestTargetPoses[targetId];
        }

        public string[] GetTrackedTargets()
        {
            return m_targetNameToId.Select( x => $"{x.Key} ({x.Value})" ).ToArray();
        }
    }
}
