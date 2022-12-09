using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JakesRemoteController : MonoBehaviour
{
    bool _menu_visible = false;
    
    GameObject _menuPanel = null;
    GameObject _og_menu = null;
    GameObject _app_menu = null;
    GameObject _my_popup = null;
    GameObject _menu_toggle_button = null;

    bool _og_menu_visible = true;
    bool _app_menu_visible = false;
    bool _popup_visible = false;
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateReferences();

        // center things that i had spread out in Editor
        SetTransformX(_menuPanel, 0);
        SetTransformX(_app_menu, 0.0f);
        SetTransformX(_my_popup, 0.0f);
        
        HideMenu();
        HidePopup();
        HideAppMenu();
    }

    void SetTransformX(GameObject o, float n)
    {
        o.transform.localPosition = new Vector3(n, o.transform.localPosition.y, o.transform.localPosition.z);
    }

    public static GameObject FindGameObjectsAll(string name)
    {
        try
        {
            return Resources.FindObjectsOfTypeAll<GameObject>().First(x => x.name == name);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Error finding " + name + " " + e);
            return null;
        };
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {

            UpdateReferences();
        }
    }

    public void UpdateReferences()
    {
        _menuPanel = FindGameObjectsAll("MyControlPanel");
        _og_menu = FindGameObjectsAll("BaseButtons");
        _app_menu = FindGameObjectsAll("AppMenu");
        _my_popup = FindGameObjectsAll("MyPopup");
        _menu_toggle_button = FindGameObjectsAll("MenuToggleButton");
    }

    public void ShowAppMenu(){
        Debug.Log("show app menu");
        UpdateReferences();
        
        _app_menu.SetActive(true);
        _app_menu_visible = true;
        SetTransformX(_app_menu, 0.0f);

        _menuPanel.SetActive(false);
        _og_menu.SetActive(false);

        _menu_visible = false;
        _og_menu_visible = false;

        _menu_toggle_button.SetActive(false);
    }

    public void HideAppMenu()
    {
        _app_menu.SetActive(false);
        _app_menu_visible = false;

        _menu_toggle_button.SetActive(true);

        // show the controller menu
        ShowMenu();
    }

    public void ShowPopup()
    {
        _my_popup.SetActive(true);
        _popup_visible = true;

        // Hide our controller menu and the og menu
        _og_menu?.SetActive(false);
        HideMenu();
    }

    public void HidePopup()
    {
        _my_popup.SetActive(false);
        _popup_visible = false;

        // show our controller menu
        ShowMenu();
    }

    public bool GetTrackpadVisible()
    {
        return !_menu_visible && !_app_menu_visible;
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
        _og_menu_visible = false;
    }

    void HideMenu()
    {
        _menu_visible = false;
        _menuPanel?.SetActive(false);

        // TODO: decouple out of this method into HideOGMenu/ShowOGMenu
        _og_menu?.SetActive(true);
        _og_menu_visible = true;
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
