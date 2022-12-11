using LibVLCSharp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JakesRemoteController : MonoBehaviour
{
    bool _menu_visible = false;
    
    GameObject _menuPanel = null;
    GameObject _og_menu = null;
    GameObject _app_menu = null;
    GameObject _my_popup = null;
    GameObject _menu_toggle_button = null;
    GameObject _options_button = null;
    GameObject _custom_popup = null;

    bool _og_menu_visible = true;
    bool _app_menu_visible = false;
    bool _popup_visible = false;
    bool _custom_popup_visible = false;

    MenuID _visible_menu_id;

    public UIStateBeforeCustomPopup stateBeforePopup;

    [SerializeField]
    public enum MenuID
    {
        OG_MENU,
        CONTROLLER_MENU,
        APP_MENU,
    };

    public class UIStateBeforeCustomPopup
    {
        public UIStateBeforeCustomPopup(MenuID _visible_menu_id) 
        {
            this.VisibleMenuID = _visible_menu_id;
        }
        public MenuID VisibleMenuID;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateReferences();

        // center things that i had spread out in Editor
        SetTransformX(_menuPanel, 0);
        SetTransformX(_app_menu, 0.0f);
        SetTransformX(_my_popup, 0.0f);
        SetTransformX(_custom_popup, 0.0f);

        HideAllMenus();
        // TODO: HideAllPopups
        HideLockedPopup();
        HideCustomPopup();
        ShowOGMenu();
    }

    void SetTransformX(GameObject o, float n)
    {
        o.transform.localPosition = new Vector3(n, o.transform.localPosition.y, o.transform.localPosition.z);
    }

    public static GameObject[] FindGameObjectsAll(string name)
    {
        try
        {
            List<GameObject> Found = new List<GameObject>();
            GameObject[] All = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject entry in All)
            {
                if (entry.name == name)
                {
                    Found.Add(entry);
                }
            }
            return Found.ToArray();

        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Error finding " + name + " " + e);
            return null;
        };
    }

    public static GameObject FindGameObjectsAllFirst(string name)
    {
        try
        {
            return FindGameObjectsAll(name)?.First();
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
        _menuPanel = FindGameObjectsAllFirst("MyControlPanel");
        _og_menu = FindGameObjectsAllFirst("BaseButtons");
        _app_menu = FindGameObjectsAllFirst("AppMenu");
        _my_popup = FindGameObjectsAllFirst("MyPopup");
        _menu_toggle_button = FindGameObjectsAllFirst("MenuToggleButton");
        _custom_popup = FindGameObjectsAllFirst("CustomPopup");
        _options_button = FindGameObjectsAllFirst("OptionsButton");
    }

    public void ShowOGMenu()
    {
        _og_menu.SetActive(true);
        _og_menu_visible = true;

        _menu_toggle_button.SetActive(true);
    }

    public void HideOGMenu()
    {
        _og_menu.SetActive(false);
        _og_menu_visible = false;
    }

    public void ShowAppMenu(){
        UpdateReferences();
        
        _app_menu.SetActive(true);
        _app_menu_visible = true;
        SetTransformX(_app_menu, 0.0f);

        _menu_toggle_button.SetActive(false);
    }

    

    public void HideAppMenu()
    {
        _app_menu.SetActive(false);
        _app_menu_visible = false;
    }

    public void ShowLockedPopup()
    {
        _my_popup.SetActive(true);
        _popup_visible = true;

        stateBeforePopup = new UIStateBeforeCustomPopup(_visible_menu_id);

        HideAllMenus();
    }

    public void RestoreStateBeforePopup()
    {
        if(stateBeforePopup == null)
        {
            return;
        }
        ShowMenuByID(stateBeforePopup.VisibleMenuID);
        stateBeforePopup = null;
    }

    public void HideLockedPopup()
    {
        _my_popup.SetActive(false);
        _popup_visible = false;

        RestoreStateBeforePopup();
    }

    /*public bool GetTrackpadVisible()
    {
        return OGMenuVisible();
    }*/

    /*public bool MenuIsHidden()
    {
        return !_menu_visible;
    }*/

    public bool OGMenuVisible()
    {
        return _og_menu_visible;
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    public void ShowControllerMenu()
    {
        _menu_visible = true;
        _menuPanel?.SetActive(true);

        _options_button.SetActive(true);
        _menu_toggle_button.SetActive(true);
    }

    public void HideControllerMenu()
    {
        _menu_visible = false;
        _menuPanel?.SetActive(false);
    }

    public void UIToggleControllerMenu()
    {
        if(_visible_menu_id == MenuID.CONTROLLER_MENU)
        {
            ShowMenuByID(MenuID.OG_MENU);
        }
        else
        {
            ShowMenuByID(MenuID.CONTROLLER_MENU);
        }
    }

    public void UIShowControllerMenu()
    {
        ShowMenuByID(MenuID.CONTROLLER_MENU);
    }
    public void UIShowOptionsMenu()
    {
        ShowMenuByID(MenuID.APP_MENU);
    }
        

    public void ShowMenuByID(MenuID id)
    {
        HideAllMenus();
        _visible_menu_id = id;
        _menu_toggle_button.SetActive(false);
        _options_button.SetActive(false);
        switch (id)
        {
            case MenuID.OG_MENU:
                ShowOGMenu();
                break;
            case MenuID.CONTROLLER_MENU:
                ShowControllerMenu();
                break;
            case MenuID.APP_MENU:
                ShowAppMenu();
                break;
        }
    }

    public void HideAllMenus()
    {
        HideOGMenu();
        HideControllerMenu();
        HideAppMenu();
    }

    public void HideMenuByID(MenuID id)
    {
        switch (id)
        {
            case MenuID.OG_MENU:
                HideOGMenu();
                break;
            case MenuID.CONTROLLER_MENU:
                HideControllerMenu();
                break;
            case MenuID.APP_MENU:
                HideAppMenu();
                break;
        }
    }

    public void ShowCustomPopup(string title, string body)
    {
        stateBeforePopup = new UIStateBeforeCustomPopup(_visible_menu_id);
        UpdateReferences();
        _popup_visible = true;
        _custom_popup.SetActive(true);
        _custom_popup.transform.position = new Vector3(
            _custom_popup.transform.position.x,
            _custom_popup.transform.position.y,
            _custom_popup.transform.position.z - 1.0f
        );
        GameObject.Find("CustomPopup/PopupInner/GameObject/Title").GetComponent<Text>().text = title;
        GameObject.Find("CustomPopup/PopupInner/GameObject/Body").GetComponent<Text>().text = body;
    }

    public void HideCustomPopup()
    {
        _popup_visible = false;
        _custom_popup.SetActive(false);
        RestoreStateBeforePopup();
    }

    // Flag UI as unlocked
    public void Unlock3DMode()
    {
        foreach (GameObject button in FindGameObjectsAll("Unlock3603D"))
        {
            button.GetComponent<Button>().interactable = false;
            button.transform.Find("Text").GetComponent<Text>().text = "Unlocked";
        }
    }
}
