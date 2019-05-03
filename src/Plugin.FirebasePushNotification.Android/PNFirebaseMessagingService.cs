using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Firebase.Messaging;

namespace Plugin.FirebasePushNotification
{
	[Service]
	[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
	public class PNFirebaseMessagingService : FirebaseMessagingService
	{
		internal static readonly string CHANNEL_ID = "eo_notification_channel";
		internal static readonly string groupKey_ID = "eo_notification_group";
		internal static readonly int NOTIFICATION_ID = 100;

		public static Type PendingActivityType;
		public static int NotificationIconResource = 0;

		public override void OnMessageReceived(RemoteMessage message)
		{
			var parameters = new Dictionary<string, object>();
			var notification = message.GetNotification();
			if (notification != null)
			{
				if (!string.IsNullOrEmpty(notification.Body))
					parameters.Add("body", notification.Body);

				if (!string.IsNullOrEmpty(notification.BodyLocalizationKey))
					parameters.Add("body_loc_key", notification.BodyLocalizationKey);

				var bodyLocArgs = notification.GetBodyLocalizationArgs();
				if (bodyLocArgs != null && bodyLocArgs.Any())
					parameters.Add("body_loc_args", bodyLocArgs);

				if (!string.IsNullOrEmpty(notification.Title))
					parameters.Add("title", notification.Title);

				if (!string.IsNullOrEmpty(notification.TitleLocalizationKey))
					parameters.Add("title_loc_key", notification.TitleLocalizationKey);

				var titleLocArgs = notification.GetTitleLocalizationArgs();
				if (titleLocArgs != null && titleLocArgs.Any())
					parameters.Add("title_loc_args", titleLocArgs);

				if (!string.IsNullOrEmpty(notification.Tag))
					parameters.Add("tag", notification.Tag);

				if (!string.IsNullOrEmpty(notification.Sound))
					parameters.Add("sound", notification.Sound);

				if (!string.IsNullOrEmpty(notification.Icon))
					parameters.Add("icon", notification.Icon);

				if (notification.Link != null)
					parameters.Add("link_path", notification.Link.Path);

				if (!string.IsNullOrEmpty(notification.ClickAction))
					parameters.Add("click_action", notification.ClickAction);

				if (!string.IsNullOrEmpty(notification.Color))
					parameters.Add("color", notification.Color);
			}

			foreach (var d in message.Data)
			{
				if (!parameters.ContainsKey(d.Key))
					parameters.Add(d.Key, d.Value);
			}

			Android.Util.Log.Debug("EOPUNE", "FirebaseService: Message reading completed.");

			// TODO : Add pending intent.
			if (PendingActivityType != null)
			{
				var intent = new Intent(this, PendingActivityType);
				intent.AddFlags(ActivityFlags.ClearTop);
				foreach (var key in parameters.Keys)
				{
					intent.PutExtra(key, parameters[key].ToString());
				}

				Android.Util.Log.Debug("EOPUNE", "FirebaseService: Creating Intent.");

				Android.Util.Log.Debug("EOPUNE", $"FirebaseService: Title: {notification.Title}.");
				Android.Util.Log.Debug("EOPUNE", $"FirebaseService: Body: {notification.Body}.");

				var pendingIntent = PendingIntent.GetActivity(this,
													 NOTIFICATION_ID,
													 intent,
													 PendingIntentFlags.OneShot);

				var notificationBuilder = new NotificationCompat.Builder(this, CHANNEL_ID)
										  .SetSmallIcon(NotificationIconResource)
										  .SetContentTitle(notification.Title)
										  .SetContentText(notification.Body)
										  .SetAutoCancel(true)
										  .SetContentIntent(pendingIntent);

				var notificationManager = NotificationManagerCompat.From(this);
				notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());

				Android.Util.Log.Debug("EOPUNE", "FirebaseService: Notification build success.");
			}

			FirebasePushNotificationManager.RegisterData(parameters);
			CrossFirebasePushNotification.Current.NotificationHandler?.OnReceived(parameters);
		}

		void ScheduleJob()
		{
			// [START dispatch_job]
			/*FirebaseJobDispatcher dispatcher = new FirebaseJobDispatcher(new GooglePlayDriver(this));
            Job myJob = dispatcher.newJobBuilder()
                    .setService(MyJobService.class)
                .setTag("my-job-tag")
                .build();
        dispatcher.schedule(myJob);*/
			// [END dispatch_job]
		}

	}

}