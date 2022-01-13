namespace ServerBrowser.Models
{
    /// <summary>
    /// Basic server information.
    /// </summary>
    /// <see cref="BssbServerDetail">Extended model</see>
    public class BssbServer
    {
        public string Key;
        public string OwnerId;
        public string ServerCode;
        public string HostSecret;
        public int PlayerLimit;
    }
}