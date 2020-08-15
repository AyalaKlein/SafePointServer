using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FirebaseCloudMessaging
{
    public class FirebaseHandler
    {
        private static FirebaseApp _firebaseApp = null;

        public static FirebaseApp FirebaseApp
        {
            get
            {
                if (_firebaseApp == null)
                {
                    _firebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key.json"))
                    });
                }

                return _firebaseApp;
            }
        }
    }
}
