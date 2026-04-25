using Android.App;
using Android.Content;
using Firebase.Messaging;

namespace PracticoMobileApp.Platforms.Android
{
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseMessagingService : Firebase.Messaging.FirebaseMessagingService
    {
        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            if (message?.GetNotification() != null)
            {
                SendNotificationToAndroid(
                    message.GetNotification().Title,
                    message.GetNotification().Body
                );
            }
        }

        private void SendNotificationToAndroid(string title, string body)
        {
            NotificationManager manager = (NotificationManager)GetSystemService(NotificationService);

            Notification.Builder builder = new Notification.Builder(this, "default_channel_id")
                .SetSmallIcon(this.ApplicationInfo.Icon)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetAutoCancel(true);

            manager?.Notify(1, builder.Build());
        }
    }
}