using System;
using UnityEngine;
using System.Collections.Generic;
using TapSDK.Core;
using TapSDK.Login;
using System.Threading.Tasks;



namespace Invariable
{
    public partial class SdkManager : Singleton<SdkManager>
    {
        private readonly string tapTapId = "lipmkcrzigd2ymxbsr";
        private readonly string tapTapToken = "X9bqJ44ejsHetkqrrtRFgGpX3wU8stawQB3Aaaei";
        private TapTapSdkOptions tapTapSdkOptions = null;
        private TapTapAccount tapTapAccount = null;
        private Action<string> m_loginCallBack = null;



        public void InitTapTapSdkOptions()
        {
            if (tapTapSdkOptions != null)
            {
                return;
            }

            tapTapSdkOptions = new TapTapSdkOptions
            {
                clientId = tapTapId, // ID，开发者后台获取

                clientToken = tapTapToken, // 令牌，开发者后台获取

                region = TapTapRegionType.CN, // 地区，CN 为国内，Overseas 为海外

                preferredLanguage = TapTapLanguageType.zh_Hans, // 语言，默认为 Auto，默认情况下，国内为 zh_Hans，海外为 en

                enableLog = true, // 是否开启日志，Release 版本请设置为 false
            };

            TapTapSDK.Init(tapTapSdkOptions);

            // 当需要添加其他模块的初始化配置项，例如合规认证、成就等， 请使用如下 API
            TapTapSdkBaseOptions[] otherOptions = new TapTapSdkBaseOptions[]
            {

            };

            TapTapSDK.Init(tapTapSdkOptions, otherOptions);
        }

        public void Login(Action<string> loginCallBack)
        {
            m_loginCallBack = loginCallBack;
            LoginTapTap();
        }

        private void LoginTapTap()
        {
            try
            {
                AsyncLoginAccount();
            }
            catch (TaskCanceledException _)
            {
                Debug.Log("用户取消登录");
            }
            catch (Exception exception)
            {
                Debug.Log($"登录失败，出现异常：{exception}");
            }
        }

        private async void AsyncLoginAccount()
        {
            // 定义授权范围
            List<string> scopes = new List<string>() { TapTapLogin.TAP_LOGIN_SCOPE_PUBLIC_PROFILE };
            tapTapAccount = await TapTapLogin.Instance.LoginWithScopes(scopes.ToArray());
            m_loginCallBack?.Invoke(tapTapAccount.name);
        }

        public void LogoutAccount()
        {
            if (tapTapAccount == null)
            {
                return;
            }

            TapTapLogin.Instance.Logout();
        }
    }
}