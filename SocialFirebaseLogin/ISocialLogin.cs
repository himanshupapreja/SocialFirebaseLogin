using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SocialFirebaseLogin
{
    public interface ISocialLogin
    {
        Task NativeGoogleSignin(Action<SocialLoginUser, SocialLoginEnum, string> OnLoginComplete, SocialLoginEnum socialLoginType);
        Task NativeSocialSignout(SocialLoginEnum socialLoginType);
    }
}
