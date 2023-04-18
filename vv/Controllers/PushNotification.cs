using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace Sch_WCFApplication
{
    public class PushNotification
    {
        public void SendNotification(string DeviceToken, string title, string msg)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var defaultApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fcm.json")),
                });
            }

            var message = new Message()
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = msg
                },
                Token = DeviceToken,
            };
            var messaging = FirebaseMessaging.DefaultInstance;
            var result = messaging.SendAsync(message);
        }

    }
}