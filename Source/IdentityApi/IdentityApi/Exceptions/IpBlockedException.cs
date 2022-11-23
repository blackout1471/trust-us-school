namespace IdentityApi.Exceptions
{
    public class IpBlockedException : Exception
    {
        public IpBlockedException() : base("Ip has been blocked from further requests")
        {

        }
    }
}
