namespace pst
{
    internal static class PstUtility
    {
        public static string GetPstLogMessage( string message )
        {
            return $"{Constants.LOG_PREFIX}: {message}";
        }
    }
}
