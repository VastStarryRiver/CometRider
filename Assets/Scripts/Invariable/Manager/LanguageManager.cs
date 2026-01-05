using UnityEngine;
using System.Globalization;



namespace Invariable
{
    public class LanguageManager : Singleton<LanguageManager>
    {
        public string LanguageKey
        {
            get
            {
                string key = PlayerPrefs.GetString(Application.productName + "_LanguageKey", "");

                if (string.IsNullOrEmpty(key))
                {
                    string type = CultureInfo.CurrentUICulture.Name;

                    switch (type)
                    {
                        case "zh-CN":
                            return "Chinese";
                        case "en-US":
                            return "English";
                        default:
                            return "Chinese";
                    }
                }

                return key;
            }
        }

        public void SetLanguageKey(string key, bool isRestart = true)
        {
            PlayerPrefs.SetString(Application.productName + "_LanguageKey", key);

            if (isRestart)
            {
                Utils.RestartGame();
            }
        }
    }
}