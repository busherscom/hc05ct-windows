using System;
using System.Windows;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Globalization;



namespace HC05CT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppCenter.Start("30255fe3-aaa8-4365-8b05-10547b7106e3",
                    typeof(Analytics), typeof(Crashes));
            SetCountryCode();
            Analytics.TrackEvent("Running");
            // AppCenter.LogLevel = LogLevel.Verbose;
            // Crashes.GenerateTestCrash();
            // System.Guid? installId = await AppCenter.GetInstallIdAsync();
            Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);
        }
        private static void SetCountryCode()
        {
            try
            {
                // This fallback country code doesn't reflect the physical device location, but rather the
                // country that corresponds to the culture it uses.
                var countryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
                AppCenter.SetCountryCode(countryCode);
            }
            catch (Exception exception)
            {
                Crashes.TrackError(exception);
            }
        }
    }


}
