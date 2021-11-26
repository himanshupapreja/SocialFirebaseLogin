using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Google.SignIn;
using FirebaseCore = Firebase.Core;
using Firebase.Auth;
using UIKit;
using System.Threading.Tasks;
using Facebook.ShareKit;
using Facebook.GamingServicesKit;
using Facebook.CoreKit;
using Facebook.LoginKit;
using Xamarin.Forms;

namespace SocialFirebaseLogin.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, ISocialLogin, ISignInDelegate
    {

        static Action<SocialLoginUser, SocialLoginEnum, string> _onLoginComplete;
        static AppDelegate appDelegate;
        static UIViewController NewUIViewController =  new UIViewController();
        static Firebase.Auth.Auth fbAuth;
        static LoginManager loginManager;
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
            appDelegate = this;

            DependencyService.Register<ISocialLogin, AppDelegate>();
            fbAuth = Auth.DefaultInstance;

            loginManager = new LoginManager();
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
            try
            {
                _onLoginComplete = OnLoginComplete;
                if (socialLoginType.Equals(SocialLoginEnum.Google))
                {
                    SignIn.SharedInstance.SignOutUser();
                    var clientId = FirebaseCore.App.DefaultInstance.Options.ClientId;
                    SignIn.SharedInstance.ClientId = clientId;
                    SignIn.SharedInstance.Delegate = this;
                    SignIn.SharedInstance.PresentingViewController = appDelegate.Window.RootViewController;
                    SignIn.SharedInstance.SignInUser();
                }
                else if (socialLoginType.Equals(SocialLoginEnum.Facebook))
                {
                    NSError fbError = null;
                    fbAuth.SignOut(out fbError);
                    loginManager.LogOut();
                    LoginConfiguration loginConfiguration = new LoginConfiguration(new string[] { "email", "public_profile" }, LoginTracking.Enabled);
                    loginManager.LogIn(appDelegate.Window.RootViewController, loginConfiguration, async (result, error) =>
                    {
                        if (result == null || result.IsCancelled)
                        {
                            if (result.IsCancelled)
                                _onLoginComplete?.Invoke(null, SocialLoginEnum.Facebook, "User Cancelled!");
                            else if (error != null)
                                _onLoginComplete?.Invoke(null, SocialLoginEnum.Facebook, error.LocalizedDescription);
                        }
                        else
                        {
                            var credentials = FacebookAuthProvider.GetCredential(result.Token.TokenString);
                            var facebookResultData = await Auth.DefaultInstance.SignInWithCredentialAsync(credentials);
                            if (facebookResultData != null && facebookResultData.User != null)
                            {
                                _onLoginComplete?.Invoke(new SocialLoginUser()
                                {
                                    DisplayName = facebookResultData.User.ProviderData.FirstOrDefault().DisplayName,
                                    Email = facebookResultData.User.ProviderData.FirstOrDefault().Email,
                                    PhotoUrl = new Uri((facebookResultData.User.ProviderData.FirstOrDefault().PhotoUrl != null ? $"{facebookResultData.User.ProviderData.FirstOrDefault().PhotoUrl}" : $"https://autisticdating.net/imgs/profile-placeholder.jpg"))
                                }, SocialLoginEnum.Facebook, null);
                            }
                            else
                            {
                                _onLoginComplete?.Invoke(null, SocialLoginEnum.Facebook, "User login Failed");
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("NativeSocialSignIn :: -- ", ex);
            }

            return Task.CompletedTask;
        }

        public Task NativeSocialSignout(SocialLoginEnum socialLoginType)
        {
            try
            {
                if (socialLoginType.Equals(SocialLoginEnum.Google))
                {
                    SignIn.SharedInstance.SignOutUser();
                }
                else if (socialLoginType.Equals(SocialLoginEnum.Facebook))
                {
                    NSError fbError = null;
                    fbAuth.SignOut(out fbError);
                    loginManager.LogOut();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("NativeSocialSignout :: -- ", ex);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Google ISignInDelegate Method
        /// </summary>
        /// <param name="signIn"></param>
        /// <param name="user"></param>
        /// <param name="error"></param>
        public void DidSignIn(SignIn signIn, GoogleUser user, NSError error)
        {
            if (user == null)
            {
                _onLoginComplete.Invoke(null,SocialLoginEnum.Google,error.ToString());
            }
            else
            {
                var image = user.Profile.GetImageUrl(500);
                _onLoginComplete.Invoke(new SocialLoginUser()
                {
                    DisplayName = user.Profile.Name,
                    Email = user.Profile.Email,
                    PhotoUrl = new Uri(image != null ? $"image" : $"https://autisticdating.net/imgs/profile-placeholder.jpg")
                }, SocialLoginEnum.Google, error.ToString());
            }
        }


        /// <summary>
        /// Facebook ILoginButtonDelegate Methodes
        /// </summary>
        /// <param name="loginButton"></param>
        /// <param name="result"></param>
        /// <param name="error"></param>
        /// <exception cref="NotImplementedException"></exception>
        //public void DidComplete(LoginButton loginButton, LoginManagerLoginResult result, NSError error)
        //{
        //}

        //public void DidLogOut(LoginButton loginButton)
        //{
        //}
    }
}