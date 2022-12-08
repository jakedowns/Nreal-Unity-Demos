package com.jakedowns.VLC3D;
import com.unity3d.player.UnityPlayerActivity;

import android.content.Context;
import android.os.Bundle;
import android.util.Log;
import com.jakedowns.BrightnessHelper;

public class VLC3DActivity extends UnityPlayerActivity {

//    public Context context;
    protected void onCreate(Bundle savedInstanceState) {
        // Calls UnityPlayerActivity.onCreate()
        super.onCreate(savedInstanceState);
        // Prints debug message to Logcat
        Log.d("OverrideActivity", "onCreate called!");

//        context = getApplicationContext();
        //Log.d("VLC3DActivity", "getBrightness " + BrightnessHelper.getBrightness(context));
    }

    public int getBrightness(){
        return BrightnessHelper.getBrightness(getApplicationContext());
    }
    public void setBrightness(int level){
        BrightnessHelper.setBrightness(getApplicationContext(),level);
    }

    /*public void onBackPressed()
    {
        // Instead of calling UnityPlayerActivity.onBackPressed(), this example ignores the back button event
        //super.onBackPressed();
        Log.d("VLC3DActivity", "set brightness");
        BrightnessHelper.setBrightness(getApplicationContext(), 128);
    }*/


}