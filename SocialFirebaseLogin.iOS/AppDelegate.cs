using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Google.SignIn;
using FirebaseCore = Firebase.Core;
using Firebase.Auth;
using UIKit;
using System.Threading.Tasks;

namespace SocialFirebaseLogin.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, ISocialLogin
    {

        static Action<SocialLoginUser, SocialLoginEnum, string> _onLoginComplete;
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            FirebaseCore.App.Configure();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            //return base.OpenUrl(app, url, options);
            return SignIn.SharedInstance.HandleUrl(url);
        }


        public Task NativeSocialSignin(Action<SocialLoginUser, SocialLoginEnum, string> OnLoginComplete, SocialLoginEnum socialLoginType)
        {
            _onLoginComplete = OnLoginComplete;
            if (socialLoginType.Equals(SocialLoginEnum.Google))
            {
                Google.SignIn.SignIn.SharedInstance.SignInUser();
            }
            else if (socialLoginType.Equals(SocialLoginEnum.Facebook))
            {
            }

            return Task.CompletedTask;
        }

        public Task NativeSocialSignout(SocialLoginEnum socialLoginType)
        {
            if (socialLoginType.Equals(SocialLoginEnum.Google))
            {
            }
            else if (socialLoginType.Equals(SocialLoginEnum.Facebook))
            {
            }

            return Task.CompletedTask;
        }
    }
}