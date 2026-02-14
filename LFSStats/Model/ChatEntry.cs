namespace LFSStatistics
{
    /// <summary>
    /// Represents a chat message captured during a session
    /// </summary>
    class ChatEntry
    {
        public string nickName;
        public string text;

        public ChatEntry(string nickName, string text)
        {
            this.nickName = nickName;
            this.text = text;
        }
    }
}
