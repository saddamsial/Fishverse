using UnityEngine;

public class Fishverse_Core : MonoBehaviour
{
    public static Fishverse_Core instance;

    public string api_key = "fish_mIbtvtlo6E";
    public string server = "https://mglabs.tinymagicians.com/fishverse/";
    public string dashboard_server = "https://api-fisher.thefishverse.com/rest-auth/login/";
    public string account_email = "";
    public string account_username = "";
    public string app_version = "";
    public string app_version_local = "0.13";
    public string avatar= "";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
