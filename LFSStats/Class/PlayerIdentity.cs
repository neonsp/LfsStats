namespace LFSStatistics
{
    /// <summary>
    /// Associates a player's license username with their in-game nickname
    /// </summary>
    class PlayerIdentity
    {
        public string userName;
        public string nickName;

        public PlayerIdentity(string userName, string nickName)
        {
            this.userName = userName;
            this.nickName = nickName;
        }
    }
}
