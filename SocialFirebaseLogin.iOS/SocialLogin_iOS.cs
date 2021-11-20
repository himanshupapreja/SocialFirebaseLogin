using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace SocialFirebaseLogin.iOS
{
    public class SocialLogin_iOS : ISocialLogin
    {
        static Action<SocialLoginUser, SocialLoginEnum, string> _onLoginComplete;
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