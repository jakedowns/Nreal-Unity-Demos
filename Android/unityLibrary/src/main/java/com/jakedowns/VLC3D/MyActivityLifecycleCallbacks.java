package com.jakedowns.VLC3D;

import static android.content.Context.POWER_SERVICE;

import android.app.Activity;
import android.app.Application;
import android.os.Bundle;
import android.os.PowerManager;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

public class MyActivityLifecycleCallbacks implements Application.ActivityLifecycleCallbacks {
    private PowerManager powerManager;
    private PowerManager.WakeLock wakeLock;

    @Override
    public void onActivityPaused(Activity activity) {
        // Release the wake lock when the activity is paused
        if (wakeLock != null && wakeLock.isHeld()) {
            wakeLock.release();
        }
    }

    /**
     * Called when the Activity calls {@link Activity#onStop super.onStop()}.
     *
     * @param activity
     */
    @Override
    public void onActivityStopped(@NonNull Activity activity) {

    }

    /**
     * Called when the Activity calls
     * {@link Activity#onSaveInstanceState super.onSaveInstanceState()}.
     *
     * @param activity
     * @param outState
     */
    @Override
    public void onActivitySaveInstanceState(@NonNull Activity activity, @NonNull Bundle outState) {

    }

    /**
     * Called when the Activity calls {@link Activity#onDestroy super.onDestroy()}.
     *
     * @param activity
     */
    @Override
    public void onActivityDestroyed(@NonNull Activity activity) {

    }

    /**
     * Called when the Activity calls {@link Activity#onCreate super.onCreate()}.
     *
     * @param activity
     * @param savedInstanceState
     */
    @Override
    public void onActivityCreated(@NonNull Activity activity, @Nullable Bundle savedInstanceState) {

    }

    /**
     * Called when the Activity calls {@link Activity#onStart super.onStart()}.
     *
     * @param activity
     */
    @Override
    public void onActivityStarted(@NonNull Activity activity) {
        aquireWakeLock(activity);
    }

    @Override
    public void onActivityResumed(Activity activity) {
        aquireWakeLock(activity);
    }

    private void aquireWakeLock(Activity activity) {
        // Acquire the wake lock when the activity is resumed
        if (powerManager == null) {
            powerManager = (PowerManager) activity.getSystemService(POWER_SERVICE);
        }
        if (wakeLock == null) {
            wakeLock = powerManager.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK, "MyApp::WakeLockTag");
        }
        if (!wakeLock.isHeld()) {
            wakeLock.acquire();
        }
    }
}

