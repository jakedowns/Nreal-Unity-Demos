using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
//using static UnityEditor.ObjectChangeEventStream;

public class MyIAPHandler : MonoBehaviour, IStoreListener
{

    private IStoreController m_StoreController;
    private IExtensionProvider m_StoreExtensionProvider;

    public MyIAPHandler()
    {
        
    }
    
    // Start is called before the first frame update
    async void Start()
    {
        try
        {
            var options = new InitializationOptions()
                .SetEnvironmentName("staging");

            await UnityServices.InitializeAsync(options);
        }
        catch (Exception exception)
        {
            // An error occurred during initialization.
            Debug.LogError("error initializing " + exception);
        }

        AnalyticsService.Instance.OptOut();
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct("180_360_3D_mode", ProductType.NonConsumable, new IDs
        {
            {"180_360_3D_mode", GooglePlay.Name},
            //{"180_360_3D_mode", MacAppStore.Name}
        });
        UnityPurchasing.Initialize(this, builder);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.m_StoreController = controller;
        this.m_StoreExtensionProvider = extensions;

        Debug.Log("IAP Init");
        ListPurchases();
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
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        Debug.Log("Purchase complete "+e.purchasedProduct.definition.id);
        ListPurchases();
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseComplete(UnityEngine.Purchasing.Product product)
    {
        Debug.Log("Purchase Complete "+product.definition.id);
        ListPurchases();
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError("Purchase failed: " + product.definition.id + " " + failureReason);
        //throw new System.NotImplementedException();
    }

    public void RestorePurchases()
    {
        m_StoreExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(result =>
        {

            if (result)
            {
                Debug.Log("Restore purchases succeeded.");
                /*Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    { "restore_success", true },
                };*/
                //AnalyticsService.Instance.CustomData("myRestore", parameters);
            }
            else
            {
                Debug.LogError("Restore purchases failed.");
                /*Dictionary<string, object> parameters = new Dictionary<string, object>()
                {
                    { "restore_success", false },
                };*/
                //AnalyticsService.Instance.CustomData("myRestore", parameters);
            }

            //AnalyticsService.Instance.Flush();
        });

    }

    public void ListPurchases()
    {
        if(m_StoreController?.products?.all is null)
        {
            Debug.LogWarning("m_StoreController never initialized");
            return;   
        }
        foreach (UnityEngine.Purchasing.Product item in m_StoreController?.products?.all)
        {
            if (item.hasReceipt)
            {
                Debug.Log("Has receipt for  " + item.receipt.ToString());
            }
            else
                Debug.LogWarning("No receipt for " + item.definition.id.ToString());
        }
    }

    public bool HasReceiptFor3DMode()
    {
        foreach (UnityEngine.Purchasing.Product item in m_StoreController.products.all)
        {
            if (item.definition.id == "180_360_3D_mode" && item.hasReceipt)
            {
                return true;
            }
        }
        return false;
    }
}
