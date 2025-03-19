namespace SysBot.Base
{
    /// <summary>
    /// Handles tracking and distribution of raid codes across projects
    /// </summary>
    public static class SharedRaidCodeHandler
    {
        // Track the active raid message with a code button
        private static ulong _currentRaidMessageId = 0;
        // Store the current raid code
        private static string _currentRaidCode = string.Empty;
        // Track the channel where the message is posted
        private static ulong _currentChannelId = 0;

        private static readonly object _lock = new object();

        // Update the current raid tracking information
        public static void UpdateActiveRaid(ulong messageId, ulong channelId, string raidCode)
        {
            lock (_lock)
            {
                _currentRaidMessageId = messageId;
                _currentChannelId = channelId;
                _currentRaidCode = raidCode;
            }
        }

        // Check if a given message is the current active raid message
        public static bool IsActiveRaidMessage(ulong messageId)
        {
            lock (_lock)
            {
                return messageId == _currentRaidMessageId;
            }
        }

        // Get the current raid code
        public static string GetCurrentRaidCode()
        {
            lock (_lock)
            {
                return _currentRaidCode;
            }
        }

        // Clear tracking when raid is complete
        public static void ClearRaidTracking()
        {
            lock (_lock)
            {
                _currentRaidMessageId = 0;
                _currentChannelId = 0;
                _currentRaidCode = string.Empty;
            }
        }

        // Get the channel ID
        public static ulong GetCurrentChannelId()
        {
            lock (_lock)
            {
                return _currentChannelId;
            }
        }
    }
}