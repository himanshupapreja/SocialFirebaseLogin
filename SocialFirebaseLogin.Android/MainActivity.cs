using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Gms.Auth.Api.SignIn;
using Android.Content;
using System.Threading.Tasks;
using Android.Gms.Common.Apis;
using Xamarin.Forms;
using Firebase.Auth;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using System.Collections.Generic;
using Firebase;
using System.Linq;
using Org.Json;

namespace SocialFirebaseLogin.Droid
{
    [Activity(Label = "SocialFirebaseLogin", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, ISocialLogin, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback
    {
        static GoogleSignInClient googleSignInClient;
        static MainActivity activity;
        static Action<SocialLoginUser, SocialLoginEnum, string> _onLoginComplete;
        static ICallbackManager callbackManager;
        static LoginManager LoginManager;
        static FirebaseAuth fbAuth;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            DependencyService.Register<ISocialLogin, MainActivity>();
            FirebaseApp.InitializeApp(this);

            /* Google Login data */
            GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn).RequestProfile().Build();
            googleSignInClient = GoogleSignIn.GetClient(this, gso);
            Intent signInIntent = googleSignInClient.SignInIntent;

            activity = this;

            /* Facebook Login data */
            fbAuth = FirebaseAuth.Instance;
            callbackManager = CallbackManagerFactory.Create();
            LoginManager = LoginManager.Instance;



            LoadApplication(new App());
        }

        public async Task NativeSocialSignin(Action<SocialLoginUser, SocialLoginEnum, string> OnLoginComplete, SocialLoginEnum socialLoginType)
        {
            _onLoginComplete = OnLoginComplete;
            if (socialLoginType.Equals(SocialLoginEnum.Google))
            {
                await googleSignInClient.SignOutAsync();
                Intent signInIntent = googleSignInClient.SignInIntent;
                activity.StartActivityForResult(signInIntent, SocialLoginEnum.Google.GetHashCode());
            }
            else if (socialLoginType.Equals(SocialLoginEnum.Facebook))
            {
                fbAuth.SignOut();

                try
                {
                    LoginManager.RegisterCallback(callbackManager, activity);
                    LoginManager.LogInWithReadPermissions(activity, new List<string> { "email", "public_profile" });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("RegisterCallback exception" + ex.Message);
                }
            }
        }

        public async Task NativeSocialSignout(SocialLoginEnum socialLoginType)
        {
            if (socialLoginType.Equals(SocialLoginEnum.Google))
            {
                await googleSignInClient.SignOutAsync(); 
            }
            else if (socialLoginType.Equals(SocialLoginEnum.Facebook))
            {
                fbAuth.SignOut(); 
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        protected async override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == SocialLoginEnum.Google.GetHashCode())
            {
                try
                {
                    GoogleSignInAccount task = await GoogleSignIn.GetSignedInAccountFromIntentAsync(data);
                    handleGoogleSignInResult(task, SocialLoginEnum.Google);
                }
                catch (ApiException e)
                {
                    System.Diagnostics.Debug.WriteLine("signInResult:failed code=" + e.Message);
                    _onLoginComplete?.Invoke(null, SocialLoginEnum.Google, "signInResult:failed code=" + e.Message);
                }
            }
            else
            {
                /* Facebook Login data */
                callbackManager.OnActivityResult(requestCode, resultCode.GetHashCode(), data);
            }
        }

        private void handleGoogleSignInResult(GoogleSignInAccount completedTask, SocialLoginEnum socialLogin)
        {
            try
            {
                GoogleSignInAccount accountt = completedTask;
                _onLoginComplete?.Invoke(new SocialLoginUser()
                {
                    DisplayName = accountt.DisplayName,
                    Email = accountt.Email,
                    PhotoUrl = new Uri((accountt.PhotoUrl != null ? $"{accountt.PhotoUrl}" : $"https://autisticdating.net/imgs/profile-placeholder.jpg"))
                }, socialLogin, string.Empty);
                // Signed in successfully, show authenticated UI.
            } 
            catch (ApiException e) 
            {
                // The ApiException status code indicates the detailed failure reason.
                // Please refer to the GoogleSignInStatusCodes class reference for more information.
                System.Diagnostics.Debug.WriteLine("signInResult:failed code=" + e.Message);
                _onLoginComplete?.Invoke(null, socialLogin, "signInResult:failed code=" + e.Message);
            } 
            catch (Exception e) 
            {
                // The ApiException status code indicates the detailed failure reason.
                // Please refer to the GoogleSignInStatusCodes class reference for more information.
                System.Diagnostics.Debug.WriteLine("signInResult:failed code=" + e.Message);
            }
        }

        private async void handleFacebookAccessToken(AccessToken token, SocialLoginEnum socialLogin)
        {
            if (token == null)
            {
                _onLoginComplete?.Invoke(null, socialLogin, "Facebook Login Failed.");
                return;
            }
            else
            {
                var credentials = FacebookAuthProvider.GetCredential(token.Token);
                var result = await fbAuth.SignInWithCredentialAsync(credentials);
                if(result != null)
                {
                    var fbUser = result.User.ProviderData?.LastOrDefault();
                    if (fbUser != null)
                    {
                        _onLoginComplete?.Invoke(new SocialLoginUser()
                        {
                            DisplayName = fbUser.DisplayName,
                            Email = fbUser.Email,
                            PhotoUrl = new Uri((fbUser.PhotoUrl != null ? $"{fbUser.PhotoUrl}" : $"https://autisticdating.net/imgs/profile-placeholder.jpg"))
                        }, socialLogin, string.Empty);
                    }
                    else
                    {
                        _onLoginComplete?.Invoke(new SocialLoginUser()
                        {
                            DisplayName = result.User.DisplayName,
                            Email = result.User.Email,
                            PhotoUrl = new Uri((result.User.PhotoUrl != null ? $"{result.User.PhotoUrl}" : $"https://autisticdating.net/imgs/profile-placeholder.jpg"))
                        }, socialLogin, string.Empty);
                    }
                }
                else
                {
                    _onLoginComplete?.Invoke(null, socialLogin, "Facebook Login Failed.");
                }
            }
        }

        public void OnCancel()
        {
            _onLoginComplete?.Invoke(null, SocialLoginEnum.Facebook, "Facebook Login cancelled.");
        }

        public void OnError(FacebookException error)
        {
            _onLoginComplete?.Invoke(null, SocialLoginEnum.Facebook, "Facebook Error::--" + error.ToString());
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            if(result != null)
            {
                var loginResult = result as LoginResult;

                var request = GraphRequest.NewMeRequest(loginResult.AccessToken, this);
                var bundle = new Android.OS.Bundle();
                bundle.PutString("fields", "id, first_name, email, last_name, picture.width(500).height(500)");
                request.Parameters = bundle;
                request.ExecuteAsync();

                //handleFacebookAccessToken(loginResult.AccessToken,SocialLoginEnum.Facebook);
            }
        }

        public void OnCompleted(JSONObject p0, GraphResponse response)
        {
            var id = string.Empty;
            var first_name = string.Empty;
            var email = string.Empty;
            var last_name = string.Empty;
            var pictureUrl = string.Empty;

            if (p0.Has("id"))
                id = p0.GetString("id");

            if (p0.Has("first_name"))
                first_name = p0.GetString("first_name");

            if (p0.Has("email"))
                email = p0.GetString("email");

            if (p0.Has("last_name"))
                last_name = p0.GetString("last_name");

            if (p0.Has("picture"))
            {
                var p2 = p0.GetJSONObject("picture");
                if (p2.Has("data"))
                {
                    var p3 = p2.GetJSONObject("data");
                    if (p3.Has("url"))
                    {
                        pictureUrl = p3.GetString("url");
                    }
                }
            }


            _onLoginComplete?.Invoke(new SocialLoginUser()
            {
                DisplayName = first_name +" "+last_name,
                Email = email,
                PhotoUrl = new Uri((pictureUrl != null ? $"{pictureUrl}" : $"https://autisticdating.net/imgs/profile-placeholder.jpg"))
            }, SocialLoginEnum.Facebook, string.Empty);

        }
    }
}