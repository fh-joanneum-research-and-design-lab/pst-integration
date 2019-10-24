using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using UnityEngine;

namespace pst
{
    /// <summary>
    ///     Communicates with the PST device via its REST API.
    /// </summary>
    public class PstController
    {
        /// <summary>
        ///     See <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_start.html">Start</a>.
        /// </summary>
        public void Start()
        {
            RestRequest   request  = new RestRequest( "Start" );
            IRestResponse response = m_client.Post( request );
            request  = new RestRequest( "Start" );
            response = m_client.Post( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.LogError( PstUtility.GetPstLogMessage( "REST Server not running." ) );
                s_serverIsOnline = false;
                return;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );

            s_serverIsOnline = true;
        }

        /// <summary>
        ///     See <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_close_streams.html">CloseStreams</a>.
        /// </summary>
        public void CloseStreams()
        {
            s_processContinuousTrackerData = false;

            RestRequest   request  = new RestRequest( "CloseStreams" );
            IRestResponse response = m_client.Post( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.Log( PstUtility.GetPstLogMessage( MSG_EMPTY_RESP ) );
                return;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );
        }

        /// <summary>
        ///     See <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_pause.html">Pause</a>.
        /// </summary>
        public void Pause()
        {
            s_processContinuousTrackerData = false;

            RestRequest   request  = new RestRequest( "Pause" );
            IRestResponse response = m_client.Post( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.Log( PstUtility.GetPstLogMessage( MSG_EMPTY_RESP ) );
                return;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );
        }

        /// <summary>
        ///     See <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_get_reference.html">GetReference</a>.
        ///     <para />
        ///     From the documentation: Tracking results are reported relative to a predefined right-handed Cartesian coordinate
        ///     system, called the reference system. The default reference system is located at 1 meter from the PST Tracker. It is
        ///     oriented such that the Z-axis points away from the PST tracker and the X-axis is parallel to the PST tracker. The
        ///     transformation matrix defining the reference system is a row-major 4x4 homogeneous transformation matrix.
        /// </summary>
        public Matrix4x4 GetReference()
        {
            RestRequest   request  = new RestRequest( "GetReference" );
            IRestResponse response = m_client.Get( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.Log( PstUtility.GetPstLogMessage( MSG_EMPTY_RESP ) );
                return Matrix4x4.identity;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );

            Matrix4x4 referenceMatrix =
                Array.ConvertAll(
                         ValuesBetweenBrackets( response.Content ),
                         x => { return float.Parse( x, NumberFormatInfo.InvariantInfo ); } )
                     .FromRowMajor();

            // the identity matrix represents the default PST reference system, which is located at 1 meter away from the tracker
            // meaning whenever we get an identity matrix, we need to return a matrix that accurately represents this 1 meter offset!
            if ( referenceMatrix.isIdentity )
                referenceMatrix.SetPosition( new Vector3( 0, 0, 1 ) );

            return referenceMatrix;
        }

        /// <summary>
        ///     See
        ///     <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_set_default_reference.html">SetDefaultReference</a>
        ///     .
        /// </summary>
        public void SetDefaultReference()
        {
            RestRequest   request  = new RestRequest( "SetDefaultReference" );
            IRestResponse response = m_client.Post( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.Log( PstUtility.GetPstLogMessage( MSG_EMPTY_RESP ) );
                return;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );
        }

        /// <summary>
        ///     See
        ///     <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_set_reference.html">SetReference</a>
        ///     .
        /// </summary>
        public void SetReference( Matrix4x4 newReference )
        {
            if ( !newReference.ValidTRS() )
            {
                Debug.LogWarning( PstUtility.GetPstLogMessage( "No valid TRS matrix given. Not setting reference!" ) );
                return;
            }

            RestRequest request = new RestRequest( "SetReference" );
            var         tmp     = new { ReferenceMatrix = newReference.ToRowMajor() };
            request.AddJsonBody( tmp );

            IRestResponse response = m_client.Post( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.Log( PstUtility.GetPstLogMessage( MSG_EMPTY_RESP ) );
                return;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );
        }

        /// <summary>
        ///     See
        ///     <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_start_tracker_data_stream.html">
        ///         StartTrackerDataStream
        ///     </a>
        ///     .
        /// </summary>
        public void StartTrackerDataStream()
        {
            if ( s_processContinuousTrackerData )
            {
                Debug.Log( PstUtility.GetPstLogMessage( "PST TrackerData is already being processed." ) );
                return;
            }

            s_processContinuousTrackerData = true;

            RestRequest request = new RestRequest( "StartTrackerDataStream" );
            request.ResponseWriter = stream =>
            {
                Task.Run(
                    () =>
                    {
                        using ( stream )
                        {
                            if ( !stream.CanRead )
                            {
                                Debug.LogError( PstUtility.GetPstLogMessage( "Cannot 'Read' stream!" ) );
                                s_processContinuousTrackerData = false;
                                return;
                            }

                            // making buffer sufficiently large s.t. only complete 'TrackerData' is read from the stream
                            // --> no chunk, which has to be put together later on
                            byte[] buffer   = new byte[m_trackerDataStreamBufferSize];
                            int    readSize = 0;
                            while ( s_processContinuousTrackerData )
                            {
                                Array.Clear( buffer, 0, m_trackerDataStreamBufferSize );
                                if ( (readSize = stream.Read( buffer, 0, buffer.Length )) > 0 )
                                {
                                    // cutting the zero bytes of the buffer before converting them to string 
                                    string contentAsString =
                                        Encoding.UTF8.GetString( buffer, 0, readSize );

                                    if ( m_logContinuousResponses )
                                        Debug.Log( $"{readSize}; {contentAsString}" );

                                    OnNewTrackerData?.Invoke( contentAsString );
                                }

                                Thread.Sleep( m_pollInterval );
                            }
                        }
                    } ).Start();
            };

            m_client.ExecuteAsync( request, null );
        }

        /// <summary>
        ///     See
        ///     <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_get_target_list.html">GetTargetList</a>.
        /// </summary>
        public string GetTargetList()
        {
            RestRequest   request  = new RestRequest( "GetTargetList" );
            IRestResponse response = m_client.Get( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.Log( PstUtility.GetPstLogMessage( MSG_EMPTY_RESP ) );
                return string.Empty;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );

            return response.Content;
        }

        /// <summary>
        ///     See
        ///     <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_get_framerate.html">GetFramerate</a>.
        /// </summary>
        public double GetFramerate()
        {
            RestRequest   request  = new RestRequest( "GetFramerate" );
            IRestResponse response = m_client.Get( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.Log( PstUtility.GetPstLogMessage( MSG_EMPTY_RESP ) );
                return default;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );

            double d = double.Parse( ValueBetween( response.Content, ":", "}" ), NumberFormatInfo.InvariantInfo );
            return d;
        }

        /// <summary>
        ///     See
        ///     <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_get_supported_framerates.html">GetSupportedFramerates</a>
        ///     .
        /// </summary>
        public double[] GetSupportedFramerates()
        {
            RestRequest   request  = new RestRequest( "GetSupportedFramerates" );
            IRestResponse response = m_client.Get( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.Log( PstUtility.GetPstLogMessage( MSG_EMPTY_RESP ) );
                return default;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );

            double[] framerates = Array.ConvertAll(
                ValuesBetweenBrackets( response.Content ),
                x => { return double.Parse( x, NumberFormatInfo.InvariantInfo ); } );

            return framerates;
        }

        /// <summary>
        ///     See
        ///     <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_set_framerate.html">SetFramerate</a>
        ///     .
        /// </summary>
        public void SetFramerate( double targetFramerate )
        {
            RestRequest request = new RestRequest( "SetFramerate" );
            var         tmp     = new { Framerate = targetFramerate };
            request.AddJsonBody( tmp );

            IRestResponse response = m_client.Post( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.Log( PstUtility.GetPstLogMessage( MSG_EMPTY_RESP ) );
                return;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );
        }

        /// <summary>
        ///     See
        ///     <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_get_exposure.html">GetExposure</a>
        ///     .
        /// </summary>
        public double GetExposure()
        {
            RestRequest   request  = new RestRequest( "GetExposure" );
            IRestResponse response = m_client.Get( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.Log( PstUtility.GetPstLogMessage( MSG_EMPTY_RESP ) );
                return default;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );

            double d = double.Parse( ValueBetween( response.Content, ":", "}" ), NumberFormatInfo.InvariantInfo );
            return d;
        }

        /// <summary>
        ///     See
        ///     <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_get_exposure_range.html">GetExposureRange</a>
        ///     .
        /// </summary>
        public (double, double) GetExposureRange()
        {
            RestRequest   request  = new RestRequest( "GetExposureRange" );
            IRestResponse response = m_client.Get( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.Log( PstUtility.GetPstLogMessage( MSG_EMPTY_RESP ) );
                return default;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );

            string strMax = ValueBetween( response.Content, "\"max\":", "," );
            string strMin = ValueBetween( response.Content, "\"min\":", "}}" );

            double max = double.Parse( strMax, NumberFormatInfo.InvariantInfo );
            double min = double.Parse( strMin, NumberFormatInfo.InvariantInfo );

            return (min, max);
        }

        /// <summary>
        ///     See
        ///     <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/_set_exposure.html">SetExposure</a>
        ///     .
        /// </summary>
        public void SetExposure( double exposure )
        {
            RestRequest request = new RestRequest( "SetExposure" );
            var         tmp     = new { Exposure = exposure };
            request.AddJsonBody( tmp );

            IRestResponse response = m_client.Post( request );

            if ( string.IsNullOrEmpty( response.Content ) )
            {
                Debug.Log( PstUtility.GetPstLogMessage( MSG_EMPTY_RESP ) );
                return;
            }

            if ( m_logSingleResponses )
                Debug.Log( PstUtility.GetPstLogMessage( response.Content ) );
        }

        private string[] ValuesBetweenBrackets( string s )
        {
            // excluding the square brackets
            int    startIndex = s.IndexOf( '[' ) + 1;
            int    endIndex   = s.LastIndexOf( ']' );
            string tmp        = s.Substring( startIndex, endIndex - startIndex );
            return tmp.Split( ',' );
        }

        private string ValueBetween( string s, string start, string end )
        {
            int startIndex = s.IndexOf( start, StringComparison.InvariantCulture ) + start.Length;
            int endIndex   = s.LastIndexOf( end, StringComparison.InvariantCulture );
            return s.Substring( startIndex, endIndex - startIndex );
        }

#region Ctrs

        // Todo: make a constructor taking fps exposure, etc.; then send this to the rest server

        public PstController()
        {
            m_pollInterval                = 10;
            m_trackerDataStreamBufferSize = 4096;

            m_logSingleResponses     = false;
            m_logContinuousResponses = false;
        }

        public PstController(
            int pollInterval, int trackerDataStreamBufferSize, bool logSingleResponses, bool logContinuousResponses
        )
        {
            m_pollInterval                = pollInterval;
            m_trackerDataStreamBufferSize = trackerDataStreamBufferSize;

            m_logSingleResponses     = logSingleResponses;
            m_logContinuousResponses = logContinuousResponses;
        }

#endregion

#region public fields

        public delegate void PstControllerHandler( string information );

        public event PstControllerHandler OnNewTrackerData;

#endregion

#region private fields

        /// <summary>
        ///     <a href="file:///C:/Program%20Files%20(x86)/PS-Tech/PST/Development/docs/rest.html">REST API Doc</a>
        /// </summary>
        private const string PST_PICO_REST_BASE_URL = "http://localhost:7278/PSTapi/";

        private const string MSG_EMPTY_RESP = "Response was empty.";

        private static bool s_processContinuousTrackerData;
        private static bool s_serverIsOnline; // indicates whether the REST server is up and running

        private RestClient m_client = new RestClient( PST_PICO_REST_BASE_URL );
        private int        m_pollInterval;
        private bool       m_logSingleResponses;
        private bool       m_logContinuousResponses;
        private int        m_trackerDataStreamBufferSize;

#endregion
    }
}
