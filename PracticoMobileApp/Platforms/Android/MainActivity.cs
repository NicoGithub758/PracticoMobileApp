using Android.App;
using Android.Content.PM;
using Android.OS;

namespace PracticoMobileApp;

[Activity(Theme = "@style/Maui.MainTheme.NoActionBar", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // CREAR EL CANAL DE NOTIFICACIONES (VITAL)
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            // El ID "default_channel_id" debe coincidir con el que pusimos en el AndroidManifest.xml
            var channelId = "default_channel_id";
            var channelName = "Notificaciones Generales";
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            var channel = new NotificationChannel(channelId, channelName, NotificationImportance.High);

            notificationManager.CreateNotificationChannel(channel);
        }
    }
}
