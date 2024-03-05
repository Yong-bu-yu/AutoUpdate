using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoUpdate.Exceptions;
using AutoUpdate.Services;

namespace AutoUpdate
{
    public class UpdateManager
    {
        private readonly string title;
        private readonly string message;
        private readonly string confirm;
        private readonly string cancel;
        private readonly Func<Task<UpdatesCheckResponse>> checkForUpdatesFunction;
        private readonly TimeSpan? runEvery;

        private bool didCheck;
        private Page mainPage;

        private readonly UpdateMode mode;

#if DEBUG
        public static string AppIDDummy
        {
            get
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                    return "com.spotify.music";
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    return "9ncbcszsjrsb";
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                    return "id324684580";

                return string.Empty;
            }
        }

#endif
        private static UpdateManager instance;
        public static UpdateManager Initialize(UpdateManagerParameters parameters, UpdateMode mode)
        {
            if (instance != null)
                throw new AutoUpdateException("UpdateManager is already initialized.");

            instance = new UpdateManager(parameters, mode);
            return instance;
        }

        public static void Clearance()
        {
            instance = null;
        }

        private UpdateManager(string title, string message, string confirm, string cancel, Func<Task<UpdatesCheckResponse>> checkForUpdatesFunction, TimeSpan? runEvery = null)
        {
            this.title = title;
            this.message = message;
            this.confirm = confirm;
            this.cancel = cancel;
            this.runEvery = runEvery;
            this.checkForUpdatesFunction = checkForUpdatesFunction ?? throw new AutoUpdateException("Check for updates function not provided. You must pass it in the constructor.");
        }

        private UpdateManager(UpdateManagerParameters parameters, UpdateMode mode)
            : this(parameters.Title, parameters.Message, parameters.Confirm, parameters.Cancel, parameters.CheckForUpdatesFunction, parameters.RunEvery)
        {
            if (mode == UpdateMode.MissingNo)
                throw new AutoUpdateException("You are not supposed to select this mode.");

            this.mode = mode;
        }

        private async void OnMainPageAppearing(object sender, EventArgs e)
        {
            await CheckUpdateAppAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            if (mode == UpdateMode.AutoInstall)
                await CheckAndUpdateAsync();
            else if (mode == UpdateMode.OpenAppStore)
                await CheckAndOpenAppStoreAsync();
            Preferences.Set("UpdateManager.LastUpdateTime", DateTime.Now);
            //Application.Current.Properties["UpdateManager.LastUpdateTime"] = DateTime.Now;
        }

        private async Task CheckAndUpdateAsync()
        {
            UpdatesCheckResponse response = await checkForUpdatesFunction();
            if (response.IsNewVersionAvailable && await mainPage.DisplayAlert(title, message, confirm, cancel))
            {
                if (DeviceInfo.Platform == DevicePlatform.WinUI || DeviceInfo.Platform == DevicePlatform.Android)
                {
                    HttpResponseMessage httpResponse = await new HttpClient().GetAsync(response.DownloadUrl);
                    var contentDisposition = httpResponse.Content.Headers.ContentDisposition;
                    byte[] data = await httpResponse.Content.ReadAsByteArrayAsync();

                    string fileName = contentDisposition.FileName ?? Guid.NewGuid().ToString();
                    DependencyService.Get<IFileOpener>().OpenFile(data, fileName);
                }
                else
                    throw new AutoUpdateException("Only Android and UWP are supported for automatic installation.");
            }
        }

        private async Task CheckAndOpenAppStoreAsync()
        {
            UpdatesCheckResponse response = await checkForUpdatesFunction();
            if (response.IsNewVersionAvailable && await mainPage.DisplayAlert(title, message, confirm, cancel))
            {
                DependencyService.Get<IStoreOpener>().OpenStore();
            }
        }

        public static string VersionName => DependencyService.Get<Services.IAppInfo>().VersionName;
        public static long VersionCode => DependencyService.Get<Services.IAppInfo>().VersionCode;

        public UpdateManager SetUpdateAppPage(Page mainPage)
        {
            this.mainPage = mainPage ?? Application.Current.MainPage;
            mainPage.Appearing += OnMainPageAppearing;
            return instance;
        }

        public async Task CheckUpdateAppAsync()
        {
            if (!didCheck)
            {
                didCheck = true;

                bool run = true;
                if (runEvery.HasValue && Preferences.ContainsKey("UpdateManager.LastUpdateTime"))
                {
                    DateTime lastUpdateTime = Preferences.Get("UpdateManager.LastUpdateTime", DateTime.Now);
                    if (lastUpdateTime + runEvery.Value < DateTime.Now)
                        run = false;
                }

                if (run)
                    await CheckForUpdatesAsync();
            }
        }
    }
}
