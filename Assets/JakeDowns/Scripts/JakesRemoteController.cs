using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JakesRemoteController : MonoBehaviour
{
    bool _menu_visible = false;
    
    GameObject _menuPanel = null;
    GameObject _trigger = null;
    
    // Start is called before the first frame update
    void Start()
    {
        _menuPanel = GameObject.Find("MySubMenu");
        _trigger = GameObject.Find("Buttons/MyTrigger");
        HideMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShowMenu()
    {
        _menu_visible = true;
        _menuPanel.SetActive(true);
        _trigger.SetActive(false);
    }

    void HideMenu()
    {
        _menu_visible = false;
        _menuPanel.SetActive(false);
        _trigger.SetActive(true);
    }

    public void ToggleMenu()
    {
        if (_menu_visible)
        {
            HideMenu();
        }
        else
        {
            ShowMenu();
        }
    }
}
