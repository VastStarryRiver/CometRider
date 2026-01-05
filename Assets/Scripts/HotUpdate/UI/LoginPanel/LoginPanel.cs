using UnityEngine;
using Invariable;



namespace HotUpdate
{
    public class LoginPanel : UIPanel
    {
        private void Awake()
        {
            Utils.PlayAnimation(gameObject, null, "Play", WrapMode.Once, () =>
            {
                Utils.SetImage(gameObject, "parent/Img_State1", "Atlas02/02_rwtx5");
                Utils.SetImage(gameObject, "parent/Img_State2", "Atlas02/02_rwtx6");

                Utils.SetGray(gameObject, "parent/Img_State2");

                Utils.GetetTextByKey(8, (text) =>
                {
                    Utils.SetText(gameObject, "parent/Text_Name", text);
                }, "#1BB25F", "£¿£¿£¿"); // Ãû×Ö£º<color={0}>{1}</color>

                Utils.AddClickListener(gameObject, "parent/Btn_Login", Login);

                Utils.AddClickListener(gameObject, "parent/Img_State1", () =>
                {
                    LanguageManager.Instance.SetLanguageKey("Chinese");
                });

                Utils.AddClickListener(gameObject, "parent/Img_State2", () =>
                {
                    LanguageManager.Instance.SetLanguageKey("English");
                });
            });
        }

        private void Start()
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_StartGame");
        }



        private void Login()
        {
            SdkManager.Instance.Login((name) =>
            {
                Utils.SetText(gameObject, "parent/Text_Name", name);
            });
        }
    }
}