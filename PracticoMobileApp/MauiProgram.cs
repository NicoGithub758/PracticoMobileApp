using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents; // <--- AGREGAR ESTO
using Plugin.Firebase.CloudMessaging;

#if ANDROID
using Plugin.Firebase.Core.Platforms.Android; // <--- AGREGAR ESTO
#endif

namespace PracticoMobileApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureLifecycleEvents(events => {
#if ANDROID
                    events.AddAndroid(android => android.OnCreate((activity, state) => {
                        // Opción alternativa si la anterior falla
                        Plugin.Firebase.Core.Platforms.Android.CrossFirebase.Initialize(activity, () => activity);
                    }));
#endif
                });

            builder.Services.AddSingleton(_ => CrossFirebaseCloudMessaging.Current);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}