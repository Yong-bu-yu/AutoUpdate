using System.IO;
using Android.Content;
using Android.OS;
using AndroidX.Core.Content;
using AutoUpdate.Droid;
using AutoUpdate.Services;

[assembly: Dependency(typeof(FileOpener))]
namespace AutoUpdate.Droid
{
    public class FileOpener : IFileOpener
    {
        public void OpenFile(byte[] data, string name)
        {
            string directory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string path = Path.Combine(directory, name);
            foreach (string file in Directory.GetFiles(directory))
            {
                if (Path.GetExtension(file) == ".apk")
                    File.Delete(file);
            }

            File.WriteAllBytes(path, data);

            Intent intent = new Intent(Intent.ActionView);
            Android.Net.Uri fileUri = null;
            if ((int)Build.VERSION.SdkInt < 23)
                fileUri = Android.Net.Uri.FromFile(new Java.IO.File(path));
            else
                fileUri = AndroidX.Core.Content.FileProvider.GetUriForFile(AutoUpdate.Context, AutoUpdate.Authority, new Java.IO.File(path));
            intent.SetDataAndType(fileUri, "application/vnd.android.package-archive");
            intent.SetFlags(ActivityFlags.GrantReadUriPermission);
            AutoUpdate.Context.StartActivity(intent);
        }
    }
}