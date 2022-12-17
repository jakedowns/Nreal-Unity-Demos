package com.jakedowns.VLC3D;
import com.unity3d.player.UnityPlayerActivity;

import android.Manifest;
import android.app.Activity;
import android.content.ContentResolver;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.ParcelFileDescriptor;
import android.os.PowerManager;
import android.util.Log;
import com.jakedowns.BrightnessHelper;
import android.widget.Toast;

import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import java.util.Arrays;
import java.util.Objects;
import java.io.File;

public class VLC3DActivity extends UnityPlayerActivity {

    Toast toast;
    String TAG = "VLC3D";
    Context context;
    private static final int MY_PERMISSIONS_REQUEST_WAKE_LOCK = 1;
    PowerManager powerManager;
    PowerManager.WakeLock wakeLock;

//    public Context context;
    @Override
    protected void onCreate(Bundle savedInstanceState) {

        if (ContextCompat.checkSelfPermission(this, Manifest.permission.WAKE_LOCK)
                != PackageManager.PERMISSION_GRANTED) {
            Log.d(TAG, "WAKE LOCK REQUESTING...");
            // Permission is not granted, request it
            ActivityCompat.requestPermissions(this,
                    new String[]{Manifest.permission.WAKE_LOCK},
                    MY_PERMISSIONS_REQUEST_WAKE_LOCK);

        }else{
            // Permission has already been granted, acquire the wake lock
            Log.d(TAG, "WAKE LOCK GRANTED");
            powerManager = (PowerManager) getSystemService(POWER_SERVICE);
            wakeLock = powerManager.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK, "MyApp::WakeLockTag");
            wakeLock.acquire();
        }

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            registerActivityLifecycleCallbacks(new MyActivityLifecycleCallbacks());
        }


        // Calls UnityPlayerActivity.onCreate()
        super.onCreate(savedInstanceState);
        // Prints debug message to Logcat
        Log.d("OverrideActivity", "onCreate called!");

        // Parse the intent
        String filepath = parsePathFromIntent(getIntent());
        /*if (intent.action == Intent.ACTION_VIEW) {
            parseIntentExtras(intent.extras)
        }*/

        if (filepath == null) {
            Log.e(TAG, "No file given");
            //showToast("error processing file");
            //finishWithResult(RESULT_CANCELED)
            return;
        }

//        player.initialize(applicationContext.filesDir.path)
//        player.addObserver(this)
//        player.playFile(filepath)

        Log.d(TAG, "got filepath " + filepath);

        context = getApplicationContext();
        Log.d(TAG, "getBrightness " + BrightnessHelper.getBrightness(context));
    }

    @Override
    public void onRequestPermissionsResult(int requestCode,
                                           String[] permissions, int[] grantResults) {
        if (requestCode == MY_PERMISSIONS_REQUEST_WAKE_LOCK) {
            // If request is cancelled, the result arrays are empty.
            if (grantResults.length > 0
                    && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                // Permission was granted, you can acquire the wake lock
                PowerManager powerManager = (PowerManager) getSystemService(POWER_SERVICE);
                PowerManager.WakeLock wakeLock = powerManager.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK, "MyApp::WakeLockTag");
                wakeLock.acquire(120*60*1000L /*120 minutes*/);
                Log.e(TAG, "WAKE LOCK ACQUIRED");
            } else {
                // Permission was denied, you can't acquire the wake lock
                Log.e(TAG, "WAKE LOCK DENIED");
            }
        }
    }

    public int getBrightness(){
        return BrightnessHelper.getBrightness(getApplicationContext());
    }
    public void setBrightness(int level){
        BrightnessHelper.setBrightness(getApplicationContext(),level);
    }

    private String parsePathFromIntent(Intent intent)
    {
        String filepath = null;

        if (Objects.equals(intent.getAction(), Intent.ACTION_VIEW)) {
            filepath = resolveUri(intent.getData());
        }else if(Objects.equals(intent.getAction(), Intent.ACTION_SEND)) {
            String extra_text = intent.getStringExtra(Intent.EXTRA_TEXT);
            if(extra_text != null) {
                Uri uri = Uri.parse(extra_text.trim());
                if (uri.isHierarchical() && !uri.isRelative()) {
                    filepath = resolveUri(uri);
                } else {
                    filepath = null;
                }
            }
        } else {
            filepath = intent.getStringExtra("filepath");
        }
        return filepath;
    }

    private String resolveUri(Uri data)
    {
        String[] stringables = {"http", "https", "rtmp", "rtmps", "rtp", "rtsp", "mms", "mmst", "mmsh", "tcp", "udp"};
        String filepath;
        String scheme = data.getScheme();
        if(Objects.equals(scheme, "file")) {
            filepath = data.getPath();
        }else if(Objects.equals(scheme, "content")) {
            filepath = openContentFd(data);
        }else if(Arrays.asList(stringables).contains(scheme)) {
            filepath = data.toString();
        }else {
            filepath = null;
        }

        if (filepath == null)
            Log.e(TAG, "unknown scheme: ${data.scheme}");

        return filepath;
    }

    private String openContentFd(Uri uri) {
        ContentResolver resolver = getApplicationContext().getContentResolver();
        Log.v(TAG, "Resolving content URI: $uri");
        int fd;
        try {
            ParcelFileDescriptor desc = resolver.openFileDescriptor(uri, "r");
            fd = desc.detachFd();
        } catch(Exception e) {
            Log.e(TAG, "Failed to open content fd: $e");
            return null;
        }
        // Find out real file path and see if we can read it directly
        try {
            String path = new File("/proc/self/fd/${fd}").getCanonicalPath();
            if (!path.startsWith("/proc") && new File(path).canRead()) {
                Log.v(TAG, "Found real file path: $path");
                ParcelFileDescriptor.adoptFd(fd).close(); // we don't need that anymore
                return path;
            }
        } catch(Exception e) {
            Log.e(TAG, "error opening $e");
        }
        // Else, pass the fd to mpv
        return "fdclose://${fd}";
    }

    /*
    private void parseIntentExtras(Bundle extras) {
        //onloadCommands.clear()
        if (extras == null)
            return;

        // Refer to http://mpv-android.github.io/mpv-android/intent.html
        /*if (extras.getByte("decode_mode") == 2.toByte())
        onloadCommands.add(arrayOf("set", "file-local-options/hwdec", "no"))
        if (extras.containsKey("subs")) {
            val subList = extras.getParcelableArray("subs")?.mapNotNull { it as? Uri } ?: emptyList()
            val subsToEnable = extras.getParcelableArray("subs.enable")?.mapNotNull { it as? Uri } ?: emptyList()

            for (suburi in subList) {
                val subfile = resolveUri(suburi) ?: continue
                        val flag = if (subsToEnable.filter { it.compareTo(suburi) == 0 }.any()) "select" else "auto"

                Log.v(TAG, "Adding subtitles from intent extras: $subfile")
                onloadCommands.add(arrayOf("sub-add", subfile, flag))
            }
        }
        if (extras.getInt("position", 0) > 0) {
            val pos = extras.getInt("position", 0) / 1000f
            onloadCommands.add(arrayOf("set", "start", pos.toString()))
        }*--/
    }
    */

    private void showToast(String msg) {
        toast.setText(msg);
        toast.show();
    }


    public void onBackPressed()
    {
        showToast("back pressed");
        
        // Instead of calling UnityPlayerActivity.onBackPressed(), this example ignores the back button event
        super.onBackPressed();
        
        

        Log.d("VLC3DActivity", "set brightness");
        BrightnessHelper.setBrightness(getApplicationContext(), 0);

    }


}