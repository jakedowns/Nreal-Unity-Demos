package com.jakedowns;

//import android.app.Activity;
import android.app.AlertDialog;
//import android.app.FragmentManager;
import android.content.ContentResolver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import android.provider.Settings;
//import android.support.v7.app.AlertDialog;
//import android.widget.Toast;

public class BrightnessHelper {

    public static void setBrightness(Context context, int brightness){

        if (Settings.System.canWrite(context)) {

            //Write code to feature for eg. set brightness or vibrate device
            ContentResolver cResolver = context.getContentResolver();  Settings.System.putInt(cResolver,  Settings.System.SCREEN_BRIGHTNESS,brightness);
        }
        else {
            showBrightnessPermissionDialog(context);
        }

    }

    public static int getBrightness(Context context) {
        ContentResolver cResolver = context.getContentResolver();
        try {
            return Settings.System.getInt(cResolver,  Settings.System.SCREEN_BRIGHTNESS);
        } catch (Settings.SettingNotFoundException e) {
            return 0;
        }
    }

    private  static  void showBrightnessPermissionDialog(final Context context) {

        final AlertDialog.Builder builder = new AlertDialog.Builder(context);
        builder.setCancelable(true);
        final AlertDialog alert = builder.create();
        builder.setMessage("Please give the permission to change brightness. \n Thanks ")
                .setCancelable(false)
                .setPositiveButton("OK", (dialog, id) -> {
                    Intent intent = new Intent(Settings.ACTION_MANAGE_WRITE_SETTINGS);
                    intent.setData(Uri.parse("package:" + context.getPackageName()));
                    // intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                    context.startActivity(intent);
                    alert.dismiss();
                });
        alert.show();
    }

    /*
    private boolean checkSystemWritePermission(Activity activity) {
        boolean retVal = true;
        if (Build.VERSION.SDK_INT >= activity.Build.VERSION_CODES.M) {
            retVal = Settings.System.canWrite(activity.getApplicationContext());
           // Log.d(TAG, "Can Write Settings: " + retVal);
            if(retVal){
                Toast.makeText(activity, "Write allowed :-)", Toast.LENGTH_LONG).show();
            }else{
                Toast.makeText(this, "Write not allowed :-(", Toast.LENGTH_LONG).show();
                FragmentManager fm = getFragmentManager();
                PopupWritePermission dialogFragment = new PopupWritePermission();
                dialogFragment.show(fm, getString(R.string.popup_writesettings_title));
            }
        }
        return retVal;
    }
    */
}