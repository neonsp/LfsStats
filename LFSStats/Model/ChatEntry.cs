namespace LFSStatistics
{
    /// <summary>
    /// Represents a chat message captured during a session
    /// </summary>
    class ChatEntry
    {
        public string nickName;
        public string text;
        public int ucid;

        public ChatEntry(string nickName, string text, int ucid = 0)
        {
            this.nickName = nickName;
            this.text = text;
            this.ucid = ucid;
        }
    }
}
