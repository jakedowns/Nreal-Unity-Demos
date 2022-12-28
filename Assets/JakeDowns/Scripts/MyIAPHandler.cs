using Samples.Purchasing.Core.LocalReceiptValidation;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using Product = UnityEngine.Purchasing.Product;
//using static UnityEditor.ObjectChangeEventStream;

public class MyIAPHandler : MonoBehaviour, IStoreListener
{

    private IStoreController m_StoreController;
    private IExtensionProvider m_StoreExtensionProvider;
    CrossPlatformValidator m_Validator = null;
    public JakesRemoteController jakesRemoteController;
    public JakesSBSVLC jakesSBSVLC;
    bool m_UseAppleStoreKitTestCertificate = false;

    const string _3DModeProductID = "com.jakedowns.vlc3d.180_360_3d_mode";

    public MyIAPHandler()
    {
        
    }

    void Awake()
    {
        jakesRemoteController = GameObject.Find("VirtualController").GetComponent<JakesRemoteController>();
        jakesSBSVLC = GameObject.Find("SBSDisplay").GetComponent<JakesSBSVLC>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InitPurchasing();
    }

    // TODO: SetServiceDisconnectAtInitializeListener 
    // TODO: SetQueryProductDetailsFailedListener 
    // https://forum.unity.com/threads/unable-to-process-purchase-with-transaction-id.1366257/#post-8631087

    async void InitPurchasing()
    {
        Debug.Log("[IAP] Initializing");
        try
        {
            var options = new InitializationOptions()
                .SetEnvironmentName("production");

            await UnityServices.InitializeAsync(options);
        }
        catch (Exception exception)
        {
            // An error occurred during initialization.
            Debug.LogError("error initializing " + exception);
        }

        AnalyticsService.Instance.OptOut();
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(_3DModeProductID, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder);
    }

    static bool IsCurrentStoreSupportedByValidator()
    {
        //The CrossPlatform validator only supports the GooglePlayStore and Apple's App Stores.
        return IsGooglePlayStoreSelected(); // || IsAppleAppStoreSelected();
    }

    static bool IsGooglePlayStoreSelected()
    {
        var currentAppStore = StandardPurchasingModule.Instance().appStore;
        return currentAppStore == AppStore.GooglePlay;
    }

    void InitializeValidator()
    {
        if (IsCurrentStoreSupportedByValidator())
        {
#if !UNITY_EDITOR
                var appleTangleData = m_UseAppleStoreKitTestCertificate ? AppleStoreKitTestTangle.Data() : AppleTangle.Data();
                m_Validator = new CrossPlatformValidator(GooglePlayTangle.Data(), appleTangleData, Application.identifier);
#endif
        }
        else
        {
            /*jakesRemoteController.ShowCustomPopup(
                "Error Initializing IAP Validtor",
                $"IAP Validator is not compatible with the current app store:{StandardPurchasingModule.Instance().appStore}");
            Debug.LogError("Error Initializing IAP Validator for " + StandardPurchasingModule.Instance().appStore);*/
        }
    }

    // Update is called once per frame
//    void Update()
//    {
//
//    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.m_StoreController = controller;
        this.m_StoreExtensionProvider = extensions;

        Debug.Log("IAP Initialized!");

        Debug.Log("IAP Available items:");
        foreach (var item in controller.products.all)
        {
            /*if (item.availableToPurchase)
            {*/
                Debug.Log(string.Join(" - ",
                    new[]
                    {
                        "[IAP] available ? " + item.availableToPurchase.ToString(),
                        item.metadata.localizedTitle,
                        item.metadata.localizedDescription,
                        item.metadata.isoCurrencyCode,
                        item.metadata.localizedPrice.ToString(),
                        item.metadata.localizedPriceString,
                        item.transactionID,
                        item.receipt,
                        item.hasReceipt.ToString(),
                    }));
            /*}*/
        }

        // Retrieve the products from the store.
        UnityEngine.Purchasing.Product[] products = controller.products.all;
        foreach (UnityEngine.Purchasing.Product product in products)
        {
            Debug.Log("IAP controller.products.all: " + product.metadata.localizedTitle);
        }

        InitializeValidator();
        RestorePurchases(false);
        Debug.Log("IAP Has user unlocked 3D mode? " + HasReceiptFor3DMode().ToString());
        if (HasReceiptFor3DMode())
        {
            jakesRemoteController.Unlock3DMode();
            jakesSBSVLC.Unlock3DMode();
        }
    }

    public void Purchase3DMode()
    {
        if(this.m_StoreController is null){
            this.InitPurchasing();
            jakesRemoteController.ShowCustomPopup("Error. Store not ready", "Please try again");
            return;
        }
        this.m_StoreController.InitiatePurchase(_3DModeProductID);
    }

    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError("Error initializing IAP " + error);
    }

    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        //Retrieve the purchased product
        var product = args.purchasedProduct;

        var isPurchaseValid = IsPurchaseValid(product);

        if (isPurchaseValid)
        {
            //Add the purchased product to the players inventory
            Debug.Log("Valid receipt, unlocking content.");
            //Debug.Log("did it update the store tho? " + HasReceiptFor3DMode());
            if (HasReceiptFor3DMode())
            {
                UnlockContent(product);
            }
            else
            {
                jakesRemoteController.ShowCustomPopup(
                    "Error Completing Purchase",
                    "Please try again later or contact vlc-support@jakedowns.com"
                );
            }
        }
        else
        {
            Debug.Log("Invalid receipt, not unlocking content.");
            jakesRemoteController.ShowCustomPopup(
                "Error Completing Purchase",
                "Please try again later or contact vlc-support@jakedowns.com"
            );
        }

        //We return Complete, informing Unity IAP that the processing on our side is done and the transaction can be closed.
        return PurchaseProcessingResult.Complete;
    }

    public void UnlockContent(Product product)
    {
        switch (product.definition.id)
        {
            case _3DModeProductID:
                jakesRemoteController.HideUnlock3DSphereModePropmptPopup();
                jakesRemoteController.ShowCustomPopup("Thank You", "You now have unlimited 3D playback of 180 and 360 videos. Enjoy!");
                jakesRemoteController.Unlock3DMode();
                jakesSBSVLC.Unlock3DMode();
                break;
            default:
                jakesRemoteController.ShowCustomPopup("error", "unknown product id " + product.definition.id);
                break;
        }
    }

    public void OnPurchaseComplete(UnityEngine.Purchasing.Product product)
    {
        Debug.Log("Purchase Complete "+product.definition.id);
        //ListPurchases();
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError("Purchase failed: " + product.definition.id + " " + failureReason);
        jakesRemoteController.ShowCustomPopup("Error", "Purchase failed: " + product.definition.id + " " + failureReason);
        //throw new System.NotImplementedException();
    }

    bool IsPurchaseValid(Product product)
    {
        //If we the validator doesn't support the current store, we assume the purchase is valid
        if (IsCurrentStoreSupportedByValidator())
        {
            try
            {
                var result = m_Validator.Validate(product.receipt);
                //The validator returns parsed receipts.
                LogReceipts(result);
            }
            //If the purchase is deemed invalid, the validator throws an IAPSecurityException.
            catch (IAPSecurityException reason)
            {
                Debug.Log($"Invalid receipt: {reason}");
                return false;
            }
        }

        return true;
    }



    public void RestorePurchases(bool show_popup = false)
    {
        m_StoreExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(result =>
        {

            if (result)
            {
                Debug.Log($"Restore purchases succeeded. {result}");
                /*Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    { "restore_success", true },
                };*/
                //AnalyticsService.Instance.CustomData("myRestore", parameters);

                if(show_popup){
                    ShowRestorePopup();
                }
            }
            else
            {
                Debug.LogError("Restore purchases failed.");
                /*Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    { "restore_success", false },
                };*/
                //AnalyticsService.Instance.CustomData("myRestore", parameters);
                ShowRestoreFailedPopup();
            }

            //AnalyticsService.Instance.Flush();
        });

    }

    public void ShowRestorePopup()
    {
        if (HasReceiptFor3DMode())
        {
            jakesRemoteController.ShowCustomPopup("Restore Successful", "You have successfully restored your purchases. You now have unlimited 3D playback of 180 and 360 videos. Enjoy!");
        }
        else
        {
            jakesRemoteController.ShowCustomPopup("Restore Successful", "You have successfully restored your purchases. You do not have any purchases to restore.");
        }
    }

    public void ShowRestoreFailedPopup()
    {
        jakesRemoteController.ShowCustomPopup("Restore Failed", "We were unable to restore your purchases. Please try again later.");
    }

    static void LogReceipts(IEnumerable<IPurchaseReceipt> receipts)
    {
        Debug.Log("Receipt is valid. Contents:");
        foreach (var receipt in receipts)
        {
            LogReceipt(receipt);
        }
    }
    static void LogReceipt(IPurchaseReceipt receipt)
    {
        Debug.Log($"Product ID: {receipt.productID}\n" +
                  $"Purchase Date: {receipt.purchaseDate}\n" +
                  $"Transaction ID: {receipt.transactionID}");

        if (receipt is GooglePlayReceipt googleReceipt)
        {
            Debug.Log($"Purchase State: {googleReceipt.purchaseState}\n" +
                      $"Purchase Token: {googleReceipt.purchaseToken}");
        }

        /*if (receipt is AppleInAppPurchaseReceipt appleReceipt)
        {
            Debug.Log($"Original Transaction ID: {appleReceipt.originalTransactionIdentifier}\n" +
                      $"Subscription Expiration Date: {appleReceipt.subscriptionExpirationDate}\n" +
                      $"Cancellation Date: {appleReceipt.cancellationDate}\n" +
                      $"Quantity: {appleReceipt.quantity}");
        }*/
    }

    public bool HasReceiptFor3DMode()
    {
        foreach (UnityEngine.Purchasing.Product item in m_StoreController.products.all)
        {
            if (item.definition.id == _3DModeProductID && item.hasReceipt)
            {
                return true;
            }
        }
        return false;
    }
}
