namespace pst
{
    internal static class PstUtility
    {
        public const string LOG_PREFIX = "PST";
        
        public static string GetPstLogMessage(string message)
        {
            return $"{LOG_PREFIX}: {message}";
        }
    }
}
