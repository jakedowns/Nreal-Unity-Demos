using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class IntentHandler : MonoBehaviour
{
    public JakesSBSVLC jakesSBSVLC;

    // Start is called before the first frame update
    void Start()
    {
        OnIntent();
    }

    // OnApplicationFocus
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            OnIntent();
        }
        else
        {
            //Debug.Log("Application lost focus");
        }
    }

    // OnIntent
    void OnIntent()
    {
        if (Application.isEditor)
        {
            return;
        }
        
        AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");
        Debug.Log("On Intent" + intent.Call<string>("getAction"));

        string result = intent.Call<string>("getDataString");

        if (result != null)
        {
            result = UnityWebRequest.UnEscapeURL(result);
            Debug.Log("On Intent" + result);
            jakesSBSVLC.Open(result);
        }

        AndroidJavaObject extras = intent.Call<AndroidJavaObject>("getExtras");
        if (extras != null)
        {
            string data = extras.Call<string>("getString", "data");
            Debug.Log("Data: " + data);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
