using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class JakeRotator : MonoBehaviour
{
    public bool paused = false;
    [SerializeField]
    public Vector3 frequencies;
    [SerializeField]
    public Vector3 amplitudes;

    public bool xCos = false;
    public bool yCos = false;
    public bool zCos = false;

    public Vector3 startingRotationEuler;

    public bool xContinuous = false;
    public bool yContinuous = false;
    public bool zContinuous = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(this.paused){
            return;
        }
        var r = transform.localRotation;
        var angle2 = new Vector3();
        var methodX = xCos ? "Cos" : "Sin";
        var methodY = yCos ? "Cos" : "Sin";
        var methodZ = zCos ? "Cos" : "Sin";

        var t = new Mathf();

        var x = Time.time * frequencies.y;
        var y = Time.time * frequencies.y;
        var z = Time.time * frequencies.z;

        if(xContinuous){
            // TODO: calculate continious Angle give frequency and amplitude
            angle2.x += Time.time / frequencies.x * amplitudes.x;
        }else{
            // ping-pong
            float xMod = (float) typeof(Mathf).GetMethod(methodX).Invoke(t, new object[] { x });
            angle2.x = amplitudes.x * xMod;
        }

        if(yContinuous){
            // TODO: *-1 if yCos checked? (invert)
            angle2.y += Time.time / frequencies.y * amplitudes.y;
        }else{
            // ping-pong
            float yMod = (float) typeof(Mathf).GetMethod(methodY).Invoke(t, new object[] { y });
            angle2.y = amplitudes.y * yMod;
        }

        if(zContinuous){
            angle2.z += Time.time / frequencies.z * amplitudes.z;
        }else{
            // ping-pong
            float zMod = (float) typeof(Mathf).GetMethod(methodZ).Invoke(t, new object[] { z });
            angle2.z = amplitudes.z * zMod;
        }

        transform.localRotation = Quaternion.Euler(angle2);
    }
}
