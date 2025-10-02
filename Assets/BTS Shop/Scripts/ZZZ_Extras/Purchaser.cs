using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;

// Placing the Purchaser class in the CompleteProject namespace allows it to interact with ScoreManager, 
// one of the existing Survival Shooter scripts.
namespace com.burningthumb
{
	// Deriving the Purchaser class from IDetailedStoreListener enables it to receive messages from Unity Purchasing.
	public class Purchaser : MonoBehaviour, IDetailedStoreListener
	{
		private static IStoreController m_StoreController;          // The Unity Purchasing system.
		private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

		private static GameObject s_spinner;
		private static GameObject s_appStoreStatusPanel;
		private static Button s_buyButton;

		public string m_className = "BTS_Purchaser";

		// Display some status
		public Button m_hideStatusButton;
		public Button m_buyButton;
		public GameObject m_appStoreStatusPanel;
		public Text m_appStoreStatusText;
		public GameObject m_spinner;
		public float m_panelCloseTime = 7.5f;


		// Product identifiers for all products capable of being purchased: 
		// "convenience" general identifiers for use with Purchasing, and their store-specific identifier 
		// counterparts for use with and outside of Unity Purchasing. Define store-specific identifiers 
		// also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)

		public string m_suffix_macos = "";
		public string m_suffix_ios = ".ios";
		public string m_suffix_tvos = ".ios";
		public string m_suffix_windows = "";
		public string m_suffix_android = "";

		// General product identifiers for the consumable, non-consumable, and subscription products.
		// Use these handles in the code to reference which product to purchase. Also use these values 
		// when defining the Product Identifiers on the store. Except, for illustration purposes, the 
		// kProductIDSubscription - it has custom Apple and Google identifiers. We declare their store-
		// specific mapping to Unity Purchasing's AddProduct, below.
		public string kProductIDConsumablePrefix = "com.burningthumb.coin.100";

		public static string kProductIDNonConsumable = "nonconsumable";
		public static string kProductIDSubscription = "subscription";

		// Apple App Store-specific product identifier for the subscription product.
		private static string kProductNameAppleSubscription = "com.unity3d.subscription.new";

		// Google Play Store-specific product identifier subscription product.
		private static string kProductNameGooglePlaySubscription = "com.unity3d.subscription.original";

		public string ClassName
		{
			get
			{
				return m_className;
			}
		}

		public string ProductIDConsumable
		{
			get
			{

				string l_suffix;

				switch (Application.platform)
				{
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.OSXPlayer:
						l_suffix = m_suffix_macos;
						break;

					case RuntimePlatform.WindowsEditor:
					case RuntimePlatform.WindowsPlayer:
						l_suffix = m_suffix_windows;
						break;

					case RuntimePlatform.IPhonePlayer:
						l_suffix = m_suffix_ios;
						break;

					case RuntimePlatform.tvOS:
						l_suffix = m_suffix_tvos;
						break;

					case RuntimePlatform.Android:
						l_suffix = m_suffix_android;
						break;

					default:
						l_suffix = "";
						break;

				}

				return kProductIDConsumablePrefix + l_suffix;

			}
		}

		public void ShowStatusPanel()
		{
			if (null != m_appStoreStatusPanel)
			{
				m_appStoreStatusPanel.SetActive(true);
				s_appStoreStatusPanel = m_appStoreStatusPanel;
			}

			if (null != m_spinner)
			{
				m_spinner.SetActive(true);
				s_spinner = m_spinner;
			}

			if (null != m_hideStatusButton)
			{
				s_buyButton = m_buyButton;
				m_hideStatusButton.Select();
			}
		}

		public void HideStatusPanel(float a_time)
		{
//			Debug.Log(this);

			if (null != this)
			{ 
				StopAllCoroutines();
				StartCoroutine(HideStatusPanelCoroutine(a_time));
			}
			else
			{
				HideStatusPanelNow();
			}
		}

		void HideStatusPanelNow()
		{
			if (null != m_spinner)
			{
				m_spinner.SetActive(false);
			}
			else if (null != s_spinner)
			{
				s_spinner.SetActive(false);
			}

			s_spinner = null;

			if (null != m_appStoreStatusPanel)
			{
				m_appStoreStatusPanel.SetActive(false);
			}
			else if (null != s_appStoreStatusPanel)
			{
				s_appStoreStatusPanel.SetActive(false);
			}

			s_appStoreStatusPanel = null;

			if (null != m_buyButton)
			{
				m_buyButton.Select();
			}
			else if (null != s_buyButton)
			{
				s_buyButton.Select();
			}

			s_buyButton = null;
		}

		IEnumerator HideStatusPanelCoroutine(float a_time)
		{
			if (null != m_spinner)
			{
				m_spinner.SetActive(false);
			}

			yield return new WaitForSeconds(a_time);

			if (null != m_appStoreStatusPanel)
			{
				m_appStoreStatusPanel.SetActive(false);
			}

			if (null != m_buyButton)
			{
				m_buyButton.Select();
			}
		}

		public void ShowStatus(string a_string)
		{
			if (null != m_appStoreStatusText)
			{
				m_appStoreStatusText.text = a_string;
			}
		}

		void Start()
		{
			// Disable the App Store Status Panel - just in case it was enabled ... it should not be
			if (null != m_appStoreStatusPanel)
			{
				m_appStoreStatusPanel.SetActive(false);
			}

			// If we haven't set up the Unity Purchasing reference
			if (m_StoreController == null)
			{
				// Begin to configure our connection to Purchasing
				InitializePurchasing();
			}
		}

		public void InitializePurchasing()
		{
			// If we have already connected to Purchasing ...
			if (IsInitialized())
			{
				// ... we are done here.
				return;
			}

			// Create a builder, first passing in a suite of Unity provided stores.
			var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

			// Add a product to sell / restore by way of its identifier, associating the general identifier
			// with its store-specific identifiers.
			builder.AddProduct(ProductIDConsumable, ProductType.Consumable);

			// Continue adding the non-consumable product.
			// builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable);

			// And finish adding the subscription product. Notice this uses store-specific IDs, illustrating
			// if the Product ID was configured differently between Apple and Google stores. Also note that
			// one uses the general kProductIDSubscription handle inside the game - the store-specific IDs 
			// must only be referenced here. 
			//builder.AddProduct(kProductIDSubscription, ProductType.Subscription, new IDs(){
			//    { kProductNameAppleSubscription, AppleAppStore.Name },
			//    { kProductNameGooglePlaySubscription, GooglePlay.Name },
			//});

			// Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
			// and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
			UnityPurchasing.Initialize(this, builder);
		}


		private bool IsInitialized()
		{
			// Only say we are initialized if both the Purchasing references are set.
			return m_StoreController != null && m_StoreExtensionProvider != null;
		}


		public void BuyConsumable()
		{
			// Buy the consumable product using its general identifier. Expect a response either 
			// through ProcessPurchase or OnPurchaseFailed asynchronously.

			// Enable the App Store Status Panel
			ShowStatusPanel();

			BuyProductID(ProductIDConsumable);
		}


		public void BuyNonConsumable()
		{
			// Buy the non-consumable product using its general identifier. Expect a response either 
			// through ProcessPurchase or OnPurchaseFailed asynchronously.

			ShowStatusPanel();

			BuyProductID(kProductIDNonConsumable);
		}


		public void BuySubscription()
		{
			// Buy the subscription product using its the general identifier. Expect a response either 
			// through ProcessPurchase or OnPurchaseFailed asynchronously.
			// Notice how we use the general product identifier in spite of this ID being mapped to
			// custom store-specific identifiers above.

			ShowStatusPanel();

			BuyProductID(kProductIDSubscription);
		}


		void BuyProductID(string productId)
		{
			// If Purchasing has been initialized ...
			if (IsInitialized())
			{
				// ... look up the Product reference with the general product identifier and the Purchasing 
				// system's products collection.
				Product product = m_StoreController.products.WithID(productId);

				// If the look up found a product for this device's store and that product is ready to be sold ... 
				if (product != null && product.availableToPurchase)
				{
					ShowStatus("Waiting for the App Store purchase flow to complete ...");

					Debug.Log($"{ClassName}: Purchasing product asychronously: '{product.definition.id}'");

					// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
					// asynchronously.
					m_StoreController.InitiatePurchase(product);
				}
				// Otherwise ...
				else
				{
					ShowStatus("The App store reported there are no products available for purchase.");

					HideStatusPanel(m_panelCloseTime);

					// ... report the product look-up failure situation  
					Debug.Log($"{ClassName}: BuyProductID: FAILED. Not purchasing product, either is not found or is not available for purchase");
				}
			}
			// Otherwise ...
			else
			{
				ShowStatus("The App store could not be initialized. Are you connected to the Internet?");

				HideStatusPanel(m_panelCloseTime);

				// ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
				// retrying initiailization.
				Debug.Log($"{ClassName}: BuyProductID: FAILED. App store not initialized (is you network working?).");
			}
		}


		// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
		// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
		public void RestorePurchases()
		{
			// If Purchasing has not yet been set up ...
			if (!IsInitialized())
			{
				ShowStatus("The App store could not be initialized. Are you connected to the Internet?");
				HideStatusPanel(m_panelCloseTime);

				// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
				Debug.Log($"{ClassName}: RestorePurchases: FAILED. App store not initialized (is you network working?).");
				return;
			}

			// If we are running on an Apple device ... 
			if (Application.platform == RuntimePlatform.IPhonePlayer ||
				Application.platform == RuntimePlatform.OSXPlayer)
			{
				// ... begin restoring purchases
				ShowStatus($"Restore Purchases started. Waiting for the App store ...");

				Debug.Log($"{ClassName}: RestorePurchases: STARTED. Waiting for the App store ...");

				// Fetch the Apple store-specific subsystem.
				var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();

				// Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
				// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
				apple.RestoreTransactions((result, message) =>
				{
					// The first phase of restoration. If no more responses are received on ProcessPurchase then 
					// no purchases are available to be restored.

					ShowStatus("Restore Purchases continuing ...");

					Debug.Log($"{ClassName}: RestorePurchases continuing: {result}. If no further messages, no purchases available to restore.");
				});
			}
			// Otherwise ...
			else
			{
				// We are not running on an Apple device. No work is necessary to restore purchases.
				ShowStatus($"Restore Purchases failed. Restore is not supported on this platform ({Application.platform})");

				Debug.Log($"{ClassName}: RestorePurchases FAIL. Not supported on this platform. Current = {Application.platform}");
			}
		}


		//  
		// --- IStoreListener
		//

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			// Purchasing has succeeded initializing. Collect our Purchasing references.
			ShowStatus("The App store was successfully initialized.");

			Debug.Log($"{ClassName}: OnInitialized: PASS");

			// Overall Purchasing system, configured with products for this application.
			m_StoreController = controller;
			// Store specific subsystem, for accessing device-specific store features.
			m_StoreExtensionProvider = extensions;
		}


		public void OnInitializeFailed(InitializationFailureReason error)
		{
			// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
			ShowStatus($"Purchasing could not be initialized. Initialization Failure Reason: {error}");

			Debug.Log($"{ClassName}: OnInitializeFailed InitializationFailureReason: {error}");
		}


		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			// A consumable product has been purchased by this user.
			if (String.Equals(args.purchasedProduct.definition.id, ProductIDConsumable, StringComparison.Ordinal))
			{
				ShowStatus($"The product was successfully purchased");

				Debug.Log($"{ClassName}: ProcessPurchase: PASS. Product: '{args.purchasedProduct.definition.id}'");

				// The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
				BTSShop.SharedInstance.Buy100Coins();

				//                ScoreManager.score += 100;
			}
			// Or ... a non-consumable product has been purchased by this user.
			else if (String.Equals(args.purchasedProduct.definition.id, kProductIDNonConsumable, StringComparison.Ordinal))
			{
				ShowStatus($"The product was successfully purchased");

				Debug.Log($"{ClassName}: Process Purchase: PASS. Product: '{args.purchasedProduct.definition.id}'");
				// TODO: The non-consumable item has been successfully purchased, grant this item to the player.
			}
			// Or ... a subscription product has been purchased by this user.
			else if (String.Equals(args.purchasedProduct.definition.id, kProductIDSubscription, StringComparison.Ordinal))
			{
				ShowStatus($"The product was successfully purchased");

				Debug.Log($"{ClassName}: ProcessPurchase: PASS. Product: '{args.purchasedProduct.definition.id}'");
				// TODO: The subscription item has been successfully purchased, grant this to the player.
			}
			// Or ... an unknown product has been purchased by this user. Fill in additional products here....
			else
			{
				ShowStatus($"The product was not successfully purchased. Did you cancel the purchase?");

				Debug.Log($"{ClassName}: ProcessPurchase: FAIL. Unrecognized product: '{args.purchasedProduct.definition.id}'");
			}

			HideStatusPanel(m_panelCloseTime / 2.0f); // Success is displayed for less time than failure

			// Return a flag indicating whether this product has completely been received, or if the application needs 
			// to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
			// saving purchased products to the cloud, and when that save is delayed. 
			return PurchaseProcessingResult.Complete;
		}

		void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
		{
			// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
			// this reason with the user to guide their troubleshooting actions.
			ShowStatus($"The product was not successfully purchased, Purchase Failure Description: {failureDescription.message}");

			HideStatusPanel(m_panelCloseTime);

			Debug.Log($"{ClassName}: OnPurchaseFailed: FAIL. Product: '{product.metadata.localizedTitle}', PurchaseFailureDescription: {failureDescription.message}");
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
		{
			// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
			// this reason with the user to guide their troubleshooting actions.
			ShowStatus($"The productwas not successfully purchased, Purchase Failure Reason: {failureReason}");

			HideStatusPanel(m_panelCloseTime);

			Debug.Log($"{ClassName}: OnPurchaseFailed: FAIL. Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
		}

		void IStoreListener.OnInitializeFailed(InitializationFailureReason failureReason, string message)
		{
			ShowStatus($"The App store could not be initialized. Message: '{message}', Purchase Failure Reason: {failureReason}");

			HideStatusPanel(m_panelCloseTime);

			Debug.Log($"{ClassName}: OnInitializeFailed: FAIL. Message: '{message}', PurchaseFailureReason: {failureReason}");
		}


	}
}