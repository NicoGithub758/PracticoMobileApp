using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Auth0.OidcClient;

namespace PracticoMobileApp;

[Activity(Theme = "@style/Maui.MainTheme.NoActionBar", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
    ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
    ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channelId = "default_channel_id";
            var channelName = "Notificaciones Generales";
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            var channel = new NotificationChannel(channelId, channelName, NotificationImportance.High);
            notificationManager.CreateNotificationChannel(channel);
        }
    }

    protected override void OnResume()
    {
        base.OnResume();
        Platform.OnResume(this);
    }

    public override void OnNewIntent(Intent intent, Android.App.ComponentCaller caller)
    {
        base.OnNewIntent(intent, caller);
        Platform.OnNewIntent(intent);
    }
}
