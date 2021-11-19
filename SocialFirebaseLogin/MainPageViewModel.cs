using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SocialFirebaseLogin
{
    public class MainPageViewModel : BaseViewModel
    {
        private bool _isFacebookLogedIn;

        public bool IsFacebookLogedIn
        {
            get { return _isFacebookLogedIn; }
            set { SetProperty(ref _isFacebookLogedIn, value); }
        }
        private bool _isGoogleLogedIn;

        public bool IsGoogleLogedIn
        {
            get { return _isGoogleLogedIn; }
            set { SetProperty(ref _isGoogleLogedIn, value); }
        }

        private string _googleButtonText;

        public string GoogleButtonText
        {
            get { return _googleButtonText; }
            set { SetProperty(ref _googleButtonText, value); }
        }

        private string _facebookButtonText;

        public string FacebookButtonText
        {
            get { return _facebookButtonText; }
            set { SetProperty(ref _facebookButtonText, value); }
        }
        

        private SocialLoginUser _googleUser;

        public SocialLoginUser GoogleUser
        {
            get { return _googleUser; }
            set { SetProperty(ref _googleUser, value); }
        }

        private SocialLoginUser _facebookUser;

        public SocialLoginUser FacebookUser
        {
            get { return _facebookUser; }
            set { SetProperty(ref _facebookUser, value); }
        }

        public Command LoginCommand { get; }

        private ISocialLogin socialLogin;


        public MainPageViewModel()
        {
            GoogleButtonText = "Google Login";
            FacebookButtonText = "Facebook Login";
            socialLogin = DependencyService.Get<ISocialLogin>();
            LoginCommand = new Command<string>(SocialLogin);
        }

        private void SocialLogin(string obj)
        {
            if (obj != null)
            {
                if (obj.Equals("Google Login"))
                {
                    socialLogin.NativeGoogleSignin(OnLoginComplete,SocialLoginEnum.Google);
                }
                else if (obj.Equals("Google Logout"))
                {
                    socialLogin.NativeSocialSignout(SocialLoginEnum.Google);
                    GoogleButtonText = "Google Login";
                    IsGoogleLogedIn = false;
                }

                if (obj.Equals("Facebook Login"))
                {
                    socialLogin.NativeGoogleSignin(OnLoginComplete, SocialLoginEnum.Facebook);
                }
                else if (obj.Equals("Facebook Logout"))
                {
                    socialLogin.NativeSocialSignout(SocialLoginEnum.Facebook);
                    FacebookButtonText = "Facebook Login";
                    IsFacebookLogedIn = false;
                }
            }
        }


        private void OnLoginComplete(SocialLoginUser socialLoginUser, SocialLoginEnum socialLoginType, string message)
        {
            if (socialLoginUser != null)
            {
                if (socialLoginType.Equals(SocialLoginEnum.Google))
                {
                    GoogleUser = socialLoginUser;
                    GoogleButtonText = "Google Logout";
                    IsGoogleLogedIn = true; 
                }
                else if (socialLoginType.Equals(SocialLoginEnum.Facebook))
                {
                    FacebookUser = socialLoginUser;
                    FacebookButtonText = "Facebook Logout";
                    IsFacebookLogedIn = true; 
                }
            }
            else
            {
                App.Current.MainPage.DisplayAlert("Error", message, "Ok");
            }
        }
    }

    public class SocialLoginUser
    {

        public string DisplayName
        {
            get;set;
        }

        public string Email
        {
            get;set;
        }
        public Uri PhotoUrl
        {
            get;set;
        }
    }
}
