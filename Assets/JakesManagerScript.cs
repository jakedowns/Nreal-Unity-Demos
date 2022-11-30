using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JakesManagerScript : MonoBehaviour
{
    bool tracking_enabled = true;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnToggleTrackingClicked()
    {
        // log
        Debug.Log("OnToggleClicked");

        // toggle tracking
        tracking_enabled = !tracking_enabled;

        // get the Scene Picker Popup
        GameObject statusText = GameObject.Find("Scene Picker Popup/TrackingStatusText");
        Text tc = statusText?.GetComponent<Text>();
        string disabled_or_enabled = tracking_enabled ? "Enabled" : "Disabled";
        if (tc)
        {
            tc.text = "Tracking: " + disabled_or_enabled;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
