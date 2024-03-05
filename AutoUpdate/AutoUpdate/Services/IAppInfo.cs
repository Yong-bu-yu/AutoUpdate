namespace AutoUpdate.Services
{
    public interface IAppInfo
    {
        string VersionName { get; }
        long VersionCode { get;}
    }
}