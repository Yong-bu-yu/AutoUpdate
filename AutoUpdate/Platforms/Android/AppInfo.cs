using AutoUpdate.Droid;
using AutoUpdate.Services;
using System.Runtime.Versioning;

[assembly: Dependency(typeof(AutoUpdate.Droid.AppInfo))]
namespace AutoUpdate.Droid
{
    public class AppInfo : Services.IAppInfo
    {
        [Obsolete]
        public string VersionName { get => Android.App.Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Android.App.Application.Context.ApplicationContext.PackageName, 0).VersionName; }

        [Obsolete]
        [ObsoletedOSPlatform("Android 5.0")]
        public long VersionCode { get => Android.App.Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Android.App.Application.Context.ApplicationContext.PackageName, 0).LongVersionCode; }
    }
}