package com.jakedowns.VLC3D;
import com.unity3d.player.UnityPlayerActivity;

import android.content.Context;
import android.os.Bundle;
import android.util.Log;
import com.jakedowns.BrightnessHelper;

public class VLC3DActivity extends UnityPlayerActivity {

    protected void onCreate(Bundle savedInstanceState) {
        // Calls UnityPlayerActivity.onCreate()
        super.onCreate(savedInstanceState);
        // Prints debug message to Logcat
        Log.d("OverrideActivity", "onCreate called!");

        Context context = getApplicationContext();
        Log.d("VLC3DActivity", "getBrightness " + BrightnessHelper.getBrightness(context));
    }

    public void onBackPressed()
    {
        // Instead of calling UnityPlayerActivity.onBackPressed(), this example ignores the back button event
        //super.onBackPressed();

        BrightnessHelper.setBrightness(getApplicationContext(), 128);
    }


}