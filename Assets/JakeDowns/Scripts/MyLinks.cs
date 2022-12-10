using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyLinks : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenPatreon()
    {
        Application.OpenURL("http://patreon.com/jakedowns");
    }

    public void OpenKofi()
    {
        Application.OpenURL("http://ko-fi.com/jakedowns");
    }

    public void OpenGithub()
    {
        // TODO: github.com/jakedowns/VLC3D
        Application.OpenURL("http://github.com/jakedowns/Nreal-Unity-Demos");
    }
}
