namespace IdentityApi.Models
{
    public class UserLocation
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public byte[] IP { get; set; }
        public string UserAgent { get; set; }


        // Convert ip as string to userlocation ip
        public void SetIPFromString(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return;

            var ipParts = ip.Split(".");

            IP = new byte[ipParts.Length];

            for (int i = 0; i < ipParts.Length; i++)
            {
                IP[i] = Convert.ToByte(ipParts[i]);
            }
        }
    }
}
