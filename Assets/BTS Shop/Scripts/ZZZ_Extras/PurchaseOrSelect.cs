using System.Collections;
using System.Collections.Generic;
using com.burningthumb;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.burningthumb
{
    public class PurchaseOrSelect : MonoBehaviour
    {
	    public Button m_button;

	    public Sprite m_openSprite;
	    public Sprite m_purchaseSprite;

		public Text m_buttonText;

		public string m_openString = "Start Game";
	    public string m_purchaseString = "Buy Game";

	    public BTSShop m_BTSShop;
	    public string m_shopKey;

	    public Image m_costImage;
	    public Text m_costText;

	    public Color m_purchaseColor;
	    public Color m_ownedColor;
	    public string m_ownedString = "Purchased";

		public GameObject[] m_disableIfOwned;

	    public UnityEvent m_buyCoin;
	    public UnityEvent m_purchase;
	    public UnityEvent m_select;

	    public BTSShop BTSShopInstance
		{

		    get
			{
				if (null == m_BTSShop)
				{
					m_BTSShop = FindObjectOfType<BTSShop>();
				}

			    return m_BTSShop;
		    }

		    set {
			    m_BTSShop = value;
		    }
	    }

	    public Button ButtonInstance {

		    get
			{
			    return m_button;
		    }

	    }

		
		public Text ButtonText
		{

		    get
			{
			    return m_buttonText;
		    }
	    }

	    public string ShopKey {

		    get {
			    return m_shopKey;
		    }

		    set {
			    m_shopKey = value;
			    ShowUI();
		    }
	    }

	    public Text CostText {

		    get
			{
			    return m_costText;
		    }
	    }

	    public Image CostImage {

		    get
			{
			    return m_costImage;
		    }
	    }

	    public bool Owned {

		    get
			{
			    return BTSShopInstance.IsOwned( m_shopKey);
		    }
	    }

	    public int Cost
		{

		    get
			{
			    return BTSShopInstance.Cost( m_shopKey);
		    }
	    }


	    public void HandleButtoClick()
	    {
		    if (Owned) {
			    m_select.Invoke();
		    }
		    else
			{
			    int l_coins = BTSShopInstance.Coins;

			    if ((0 == l_coins) || (l_coins < Cost))
			    { 
				    m_buyCoin.Invoke();
			    }
			    else {
				    m_purchase.Invoke();
			    }
		    }
		
		
	    }

	    public void ShowUI()
	    {
		    if (Owned)
			{
				if (ButtonInstance && m_openSprite)
				{ 
					ButtonInstance.image.sprite = m_openSprite;
				}

				if (ButtonText && (m_openString.Length > 0))
				{ 
					ButtonText.text = m_openString;
				}

			    CostImage.color = m_ownedColor;
			    CostText.text = m_ownedString;

				foreach (GameObject l_disable in m_disableIfOwned)
				{
					l_disable.SetActive(false);
				}
		    }
		    else
			{
				if (ButtonInstance && m_purchaseSprite)
				{ 
					ButtonInstance.image.sprite = m_purchaseSprite;
				}

				if (ButtonText && (m_purchaseString.Length > 0))
				{ 
					ButtonText.text = m_purchaseString;
				}

			    CostImage.color = m_purchaseColor;
			    CostText.text = Cost + " Coins";
		    }
	    }

	        public void BuyProduct()
	        {

		    if (BTSShopInstance.Coins >= BTSShopInstance.Cost(ShopKey))
		    { 
			    BTSShopInstance.Coins -= BTSShopInstance.Cost(ShopKey);

			    BTSShopInstance.SetLocalAndRemoteStringValue(ShopKey, BTSShopInstance.OwnedValue(ShopKey) );

			    ShowUI();
		    }
	        }

        // Start is called before the first frame update
        void Start()
        {
	    // SecurePlayerPrefs.DeleteAll();
	    ShowUI();
        
        }

    }
}
