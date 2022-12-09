using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Purchasing;
//using static UnityEditor.PlayerSettings;
using Product = UnityEngine.Purchasing.Product;

public class MyIAPManager : IStoreListener
{

    private IStoreController controller;
    private IExtensionProvider extensions;

    public MyIAPManager()
    {
        /*var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct("180_360_3D_mode", ProductType.Consumable, new IDs
        {
            {"180_360_3D_mode", GooglePlay.Name},
            //{"100_gold_coins_mac", MacAppStore.Name}
        });

        UnityPurchasing.Initialize(this, builder);*/
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.extensions = extensions;
    }

    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
    }

    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError("Purchase failed: " + product.definition.id + " " + failureReason);
        //throw new System.NotImplementedException();
    }
}