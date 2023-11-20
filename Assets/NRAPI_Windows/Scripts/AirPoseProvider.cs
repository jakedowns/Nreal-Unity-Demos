using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using UnityEngine.Experimental.XR.Interaction;

public class AirPoseProvider : BasePoseProvider
{
    [DllImport("AirAPI_Windows", CallingConvention = CallingConvention.Cdecl)]
    public static extern int StartConnection();

    [DllImport("AirAPI_Windows", CallingConvention = CallingConvention.Cdecl)]
    public static extern int StopConnection();

    [DllImport("AirAPI_Windows", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetQuaternion();

    [DllImport("AirAPI_Windows", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetEuler();


    // Start is called before the first frame update
    void Start()
    {
        // Start the connection
        StartConnection();

    }

    // Update Pose
    public override bool TryGetPoseFromProvider(out Pose output)
    {
        IntPtr ptr = GetEuler();
        float[] arr = new float[3];
        Marshal.Copy(ptr, arr, 0, 3);
    
        Quaternion target = Quaternion.Euler(-arr[1] + 90.0f, -arr[2], -arr[0]);
        // Quaternion target = Quaternion.Euler(arr[1], -arr[2], -arr[0]);
        // Quaternion target = Quaternion.Euler(45, 0, 0);
        output = new Pose(new Vector3(0, 0, 0), target);
        return true;
    }
    
    // Use quaternion directly
    // public override bool TryGetPoseFromProvider(out Pose output)
    // {
    //     IntPtr ptr = GetQuaternion();
    //     float[] arr = new float[4];
    //     Marshal.Copy(ptr, arr, 0, 4);
    //
    //     Quaternion target = Quaternion(arr[1], arr[2], -arr[0] + 180.0f);
    //     output = new Pose(new Vector3(0, 0, 0), target);
    //     return true;
    // }
}