using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Firebase.Messaging;

namespace PracticoMobileApp.Platforms.Android
{
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseMessagingService : Firebase.Messaging.FirebaseMessagingService
    {
        private const string TAG = "FirebaseMessaging";

        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
            Log.Debug(TAG, "Token recibido: " + token);
            // Aquí puedes guardar el token en tu servidor o BD
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            Log.Debug(TAG, "Mensaje recibido");

            if (message?.GetNotification() != null)
            {
                string title = message.GetNotification().Title ?? "Notificación";
                string body = message.GetNotification().Body ?? "";

                Log.Debug(TAG, $"Título: {title}, Body: {body}");
                SendNotificationToAndroid(title, body);
            }

            // También procesa datos del mensaje si existen
            if (message?.Data != null && message.Data.Count > 0)
            {
                Log.Debug(TAG, "Datos recibidos: " + string.Join(", ", message.Data));
            }
        }

        private void SendNotificationToAndroid(string title, string body)
        {
            try
            {
                NotificationManager manager = (NotificationManager)GetSystemService(Context.NotificationService);

                // Crear canal de notificación para Android 8+ (API 26+)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    NotificationChannel channel = new NotificationChannel(
                        "default_channel_id",
                        "Notificaciones",
                        NotificationImportance.Default)
                    {
                        Description = "Canal de notificaciones general"
                    };
                    manager?.CreateNotificationChannel(channel);
                }

                Notification.Builder builder = new Notification.Builder(this, "default_channel_id")
                    .SetSmallIcon(ApplicationInfo.Icon)
                    .SetContentTitle(title)
                    .SetContentText(body)
                    .SetAutoCancel(true)
                    .SetPriority((int)NotificationPriority.High)
                    .SetDefaults(NotificationDefaults.All);

                manager?.Notify(1, builder.Build());
                Log.Debug(TAG, "Notificación enviada correctamente");
            }
            catch (Exception ex)
            {
                Log.Error(TAG, "Error al enviar notificación: " + ex.Message);
            }
        }
    }
}