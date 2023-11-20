using LibVLCSharp;
using NRKernal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class JakesRemoteController : MonoBehaviour
{
    /*bool _menu_visible = false;*/

    JakesSBSVLC jakesSBSVLC;

    GameObject _menuPanel = null;
    GameObject _og_menu = null;
    GameObject _app_menu = null;
    // GameObject _unlock_3d_sphere_mode_prompt_popup = null;
    GameObject _menu_toggle_button = null;
    GameObject _options_button = null;
    GameObject _custom_popup = null;
    GameObject _aspect_popup;
    GameObject _lockScreenNotice = null;
    GameObject _display_popup = null;
    GameObject _format_popup = null;
    GameObject _whats_new_popup = null;
    GameObject _picture_settings_popup = null;

    bool _og_menu_visible = true;

    MenuID _visible_menu_id;

    public UIStateBeforeCustomPopup stateBeforePopup;

    [SerializeField]
    public enum MenuID
    {
        OG_MENU,
        CONTROLLER_MENU,
        APP_MENU,
    };

    [SerializeField]
    public enum PopupID
    {
        CUSTOM_AR_POPUP,
        // MODE_LOCKED,
        CUSTOM_POPUP,
        PICTURE_SETTINGS_POPUP,
        FILE_FORMAT_POPUP,
        DISPLAY_SETTINGS_POPUP,
        COLOR_POPUP,
        WHATS_NEW_POPUP
    }

    PopupID[] popupStack;

    public class UIStateBeforeCustomPopup
    {
        public UIStateBeforeCustomPopup(MenuID _visible_menu_id)
        {
            this.VisibleMenuID = _visible_menu_id;
        }
        public MenuID VisibleMenuID;
    }

    public void SetJakesSBSVLC(JakesSBSVLC instance)
    {
        jakesSBSVLC = instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateReferences();

        _lockScreenNotice = GameObject.Find("LockScreenNotice");

        // hide 6dof button if not supported
        if (NRDevice.Subsystem.GetDeviceType() != NRDeviceType.NrealLight)
        {
            GameObject.Find("ChangeTo6Dof").SetActive(false);
        }

        string versionName = Application.version;
        string versionCode = Application.buildGUID;
        GameObject.Find("AppMenu/AppMenuInner/Subtitle").GetComponent<Text>().text = $"{versionName} ({versionCode})";

        // center UI things that i had spread out in Editor
        CenterPopupLocations();

        // Center Menus/Objects
        CenterXY(_lockScreenNotice);
        CenterXY(_menuPanel);
        CenterXY(_app_menu);

        _lockScreenNotice.SetActive(false);

        HideAllMenus();
        HideAllPopups();

        ShowOGMenu();

        if (PlayerPrefs.GetInt("OnboardingSeen_0_0_5_g") == 1)
        {
            // The user has already seen the onboarding tutorial text
        }
        else
        {
            // The user has not yet seen the onboarding tutorial text
            PlayerPrefs.SetInt("OnboardingSeen_0_0_5_g", 1);
            ShowWhatsNewPopup();
        }
    }

    void CenterPopupLocations()
    {
        // Get the "Popups" game object, then loop over each of it's top-level children
        // and center them on the screen
        GameObject popups = GameObject.Find("Canvas/Popups");
        for (int i = 0; i < popups.transform.childCount; i++)
        {
            GameObject childGameObject = popups.transform.GetChild(i).gameObject;
            //Debug.Log("centering " + childGameObject.name);
            CenterXY(childGameObject);
        }
    }

    void CenterXY(GameObject o)
    {
        o.transform.localPosition = new Vector3(
            0.0f,
            0.0f,
            o.transform.localPosition.z
        );
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
        _menu_toggle_button = FindGameObjectsAllFirst("MenuToggleButton");

        _menuPanel = FindGameObjectsAllFirst("MyControlPanel");
        _og_menu = FindGameObjectsAllFirst("BaseButtons");
        _app_menu = FindGameObjectsAllFirst("AppMenu");

        // _unlock_3d_sphere_mode_prompt_popup = FindGameObjectsAllFirst("Unlock3DSphereModePopup");

        _custom_popup = FindGameObjectsAllFirst("CustomPopup");
        _aspect_popup = FindGameObjectsAllFirst("AspectRatioPopup");
        _options_button = FindGameObjectsAllFirst("OptionsButton");
        _display_popup = FindGameObjectsAllFirst("DisplayPopup");
        _format_popup = FindGameObjectsAllFirst("FormatPopup");
        _whats_new_popup = FindGameObjectsAllFirst("WhatsNewPopup");
        _picture_settings_popup = FindGameObjectsAllFirst("PictureSettingsPopup");
    }

    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
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

    public void ShowAppMenu() {
        UpdateReferences();

        _app_menu.SetActive(true);
        CenterXY(_app_menu);

        _menu_toggle_button.SetActive(false);
    }



    public void HideAppMenu()
    {
        _app_menu.SetActive(false);
    }

    // public void ShowUnlock3DSphereModePropmptPopup()
    // {
    //     _unlock_3d_sphere_mode_prompt_popup.SetActive(true);
    //
    //     stateBeforePopup = new UIStateBeforeCustomPopup(_visible_menu_id);
    //
    //     HideAllMenus();
    // }

    public void RestoreStateBeforePopup()
    {
        if (stateBeforePopup == null)
        {
            return;
        }
        ShowMenuByID(stateBeforePopup.VisibleMenuID);
        stateBeforePopup = null;
    }

    // public void HideUnlock3DSphereModePropmptPopup()
    // {
    //     _unlock_3d_sphere_mode_prompt_popup.SetActive(false);
    //
    //     RestoreStateBeforePopup();
    // }

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
        _menuPanel?.SetActive(true);

        _options_button.SetActive(true);
        _menu_toggle_button.SetActive(true);
    }

    public void HideControllerMenu()
    {
        _menuPanel?.SetActive(false);
    }

    public void UIToggleControllerMenu()
    {
        if (_visible_menu_id == MenuID.CONTROLLER_MENU)
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

    public void HideAllPopups()
    {
        // TODO: just loop
        // HideUnlock3DSphereModePropmptPopup();
        HideCustomPopup();
        HideCustomARPopup();
        HideDisplayPopup();
        HideFormatPopup();
        HideWhatsNewPopup();
        HidePopupByID(PopupID.PICTURE_SETTINGS_POPUP);
    }

    public void ShowPopupByID(PopupID popupID)
    {
        stateBeforePopup = new UIStateBeforeCustomPopup(_visible_menu_id);
        UpdateReferences();

        switch (popupID)
        {
            // case PopupID.MODE_LOCKED:
            //     ShowUnlock3DSphereModePropmptPopup();
            //     break;
            /*case PopupID.CUSTOM:
                ShowCustomPopup();
                break;*/
            case PopupID.CUSTOM_AR_POPUP:
                ShowCustomARPopup();
                break;
        }
    }

    public void HidePopupByID(PopupID popupID)
    {
        switch (popupID)
        {
            // case PopupID.MODE_LOCKED:
            //     HideUnlock3DSphereModePropmptPopup();
            //     break;
            /*case PopupID.CUSTOM:
                HideCustomPopup();
                break;*/
            case PopupID.CUSTOM_AR_POPUP:
                HideCustomARPopup();
                break;
            case PopupID.PICTURE_SETTINGS_POPUP:
                HidePictureSettingsPopup();
                break;
        }
        RestoreStateBeforePopup();
    }

    public void ShowCustomARPopup()
    {
        _aspect_popup.SetActive(true);

        UpdateCustomARPopupValuePreviewText();

        // split and parse float
        string[] split = jakesSBSVLC.GetCurrentAR().Split(':');
        float ar_width = float.Parse(split[0]);
        float ar_height = float.Parse(split[1]);

        float ar_combo = ar_width / ar_height;

        // set sliders to current value
        _aspect_popup.transform.Find("ARWidthBar").GetComponent<Slider>().value = ar_width;
        _aspect_popup.transform.Find("ARHeightBar").GetComponent<Slider>().value = ar_height;
        _aspect_popup.transform.Find("ARComboBar").GetComponent<Slider>().value = ar_combo;
    }

    public void UpdateCustomARPopupValuePreviewText()
    {
        GameObject.Find("ARValuePreview").GetComponent<Text>().text = jakesSBSVLC.GetCurrentAR();

        // split and parse float
        string[] split = jakesSBSVLC.GetCurrentAR().Split(':');
        float ar_width = float.Parse(split[0]);
        float ar_height = float.Parse(split[1]);

        float ar_combo = ar_width / ar_height;
        ar_combo = Mathf.Round(ar_combo * 100f) / 100f;

        GameObject.Find("ARValuePreviewDecimal").GetComponent<Text>().text = ar_combo.ToString();
    }

    public void ApplyCustomARPopup()
    {
        HidePopupByID(PopupID.CUSTOM_AR_POPUP);
        string requested_value = _aspect_popup.transform.Find("ARTextInput").GetComponent<InputField>().text;
        jakesSBSVLC.SetAspectRatio(requested_value);
    }

    public void HideCustomARPopup()
    {
        _aspect_popup.SetActive(false);
    }

    public void ShowCustomPopup(string title, string body)
    {
        stateBeforePopup = new UIStateBeforeCustomPopup(_visible_menu_id);
        UpdateReferences();
        _custom_popup.SetActive(true);
        _custom_popup.transform.position = new Vector3(
            _custom_popup.transform.position.x,
            _custom_popup.transform.position.y,
            _custom_popup.transform.position.z - 1.0f // TODO: make this dynamic based on popup stack index
        );
        GameObject.Find("CustomPopup/PopupInner/GameObject/Title").GetComponent<Text>().text = title;
        GameObject.Find("CustomPopup/PopupInner/GameObject/Body").GetComponent<Text>().text = body;
    }

    public void HideCustomPopup()
    {
        _custom_popup.SetActive(false);
        RestoreStateBeforePopup();
    }

    public void ShowDisplayPopup()
    {
        _display_popup.SetActive(true);
    }

    public void HideDisplayPopup()
    {
        _display_popup.SetActive(false);
    }

    public void ShowFormatPopup()
    {
        _format_popup.SetActive(true);
    }

    public void HideFormatPopup()
    {
        _format_popup.SetActive(false);
    }
    public void ShowWhatsNewPopup()
    {
        _whats_new_popup.SetActive(true);
    }

    public void HideWhatsNewPopup()
    {
       _whats_new_popup.SetActive(false);
    }

    public void ShowPictureSettingsPopup()
    {
        _picture_settings_popup.SetActive(true);
    }

    public void HidePictureSettingsPopup()
    {
        _picture_settings_popup.SetActive(false);
    }

    // Flag UI as unlocked
    // public void Unlock3DMode()
    // {
    //     foreach (GameObject button in FindGameObjectsAll("Unlock3603D"))
    //     {
    //         button.GetComponent<Button>().interactable = false;
    //         button.transform.Find("Text").GetComponent<Text>().text = "Unlocked";
    //     }
    // }
}
