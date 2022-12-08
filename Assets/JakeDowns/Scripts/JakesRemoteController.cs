using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JakesRemoteController : MonoBehaviour
{
    bool _menu_visible = false;
    
    GameObject _menuPanel = null;
    GameObject _og_menu = null;
    
    // Start is called before the first frame update
    void Start()
    {
        _menuPanel = GameObject.Find("MyControlPanel");
        _og_menu = GameObject.Find("BaseControllerPanel/Buttons");
        HideMenu();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            _menuPanel = GameObject.Find("MyControlPanel");
            _og_menu = GameObject.Find("BaseControllerPanel/Buttons");
        }
    }

    public bool MenuIsHidden()
    {
        return !_menu_visible;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShowMenu()
    {
        _menu_visible = true;
        _menuPanel?.SetActive(true);
        _og_menu?.SetActive(false);
    }

    void HideMenu()
    {
        _menu_visible = false;
        _menuPanel?.SetActive(false);
        _og_menu?.SetActive(true);
    }

    public void ToggleMenu()
    {
        Debug.Log("toggle menu " + !_menu_visible);
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
