using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif



[Serializable]
public struct ItemEntry {
	public ItemData item;
	public int price;
	public int quantity;
}

[Serializable]
public struct FeatureEntry {
	public Sprite icon;
	public string name;
	public string text;
	public UnityEvent action;
	public int price;
	public int quantity;
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Shop Screen
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI Screen/Shop Screen")]
public sealed class ShopScreen : ScreenBase {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(ShopScreen))]
	class ShopScreenEditor : EditorExtensions {
		ShopScreen I => target as ShopScreen;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Selected", EditorStyles.boldLabel);
			I.DefaultSelected = ObjectField("Default Selected", I.DefaultSelected);
			Space();

			LabelField("Shop", EditorStyles.boldLabel);
			I.Scrollview  = ObjectField("Scrollview",  I.Scrollview);
			I.BuyButton   = ObjectField("Buy Button",  I.BuyButton);
			I.SellButton  = ObjectField("Sell Button", I.SellButton);
			Space();
			I.DetailPanelIconImage = ObjectField("Detail Panel Icon Image", I.DetailPanelIconImage);
			I.DetailPanelNameText  = ObjectField("Detail Panel Name Text",  I.DetailPanelNameText);
			I.DetailPanelTextText  = ObjectField("Detail Panel Text Text",  I.DetailPanelTextText);
			I.DetailPanelDataText  = ObjectField("Detail Panel Data Text",  I.DetailPanelDataText);
			Space();
			I.Anchor             = ObjectField("Anchor",               I.Anchor);
			I.ItemButtonTemplate = ObjectField("Item Button Template", I.ItemButtonTemplate);
			Space();

			End();
		}
	}
	#endif



	// Constants

	const float DoubleClickThreshold = 0.5f;
	const float TextIconMargin = 4f;



	// Fields

	List<ItemEntry> m_ItemList;
	List<FeatureEntry> m_FeatureList;

	[SerializeField] GameObject m_Scrollview;
	[SerializeField] GameObject m_SellScrollview;
	[SerializeField] GameObject m_BuyButton;
	[SerializeField] GameObject m_SellButton;

	[SerializeField] Image m_DetailPanelIconImage;
	[SerializeField] TextMeshProUGUI m_DetailPanelNameText;
	[SerializeField] TextMeshProUGUI m_DetailPanelTextText;
	[SerializeField] TextMeshProUGUI m_DetailPanelDataText;

	[SerializeField] GameObject m_Anchor;
	[SerializeField] Button m_ItemButtomTemplate;
	List<Button> m_ItemButtomList = new();
	Stack<Button> m_ItemButtomPool = new();

	(Button button, float time) m_Input;



	// Properties

	public override bool IsPrimary => false;
	public override bool IsOverlay => true;
	public override BackgroundMode BackgroundMode => BackgroundMode.SceneBlur;



	/*
	Shop Screen 호출 전 ItemList 및 FeatureList 등록 필요
	*/

	public List<ItemEntry> ItemList {
		get => m_ItemList;
		set => m_ItemList = value;
	}
	public List<FeatureEntry> FeatureList {
		get => m_FeatureList;
		set => m_FeatureList = value;
	}

	GameObject Scrollview {
		get => m_Scrollview;
		set => m_Scrollview = value;
	}
	GameObject BuyButton {
		get => m_BuyButton;
		set => m_BuyButton = value;
	}
	GameObject SellButton {
		get => m_SellButton;
		set => m_SellButton = value;
	}
	public bool IsBuying {
		get => BuyButton.activeSelf;
		set {
			if (value) {
				BuyButton.SetActive(true);
				SellButton.SetActive(false);
				RefreshListPanelForBuy();
			} else {
				BuyButton.SetActive(false);
				SellButton.SetActive(true);
				RefreshListPanelForSell();
			}
			if (0 < ItemButtonList.Count) ItemButtonList[0].onClick.Invoke();
			else RefreshDetailPanel(null, null, null, 0, 0);
			Input = default;
		}
	}

	Image DetailPanelIconImage {
		get => m_DetailPanelIconImage;
		set => m_DetailPanelIconImage = value;
	}
	TextMeshProUGUI DetailPanelNameText {
		get => m_DetailPanelNameText;
		set => m_DetailPanelNameText = value;
	}
	TextMeshProUGUI DetailPanelTextText {
		get => m_DetailPanelTextText;
		set => m_DetailPanelTextText = value;
	}
	TextMeshProUGUI DetailPanelDataText {
		get => m_DetailPanelDataText;
		set => m_DetailPanelDataText = value;
	}

	GameObject Anchor {
		get => m_Anchor;
		set => m_Anchor = value;
	}
	Button ItemButtonTemplate {
		get => m_ItemButtomTemplate;
		set => m_ItemButtomTemplate = value;
	}
	List<Button> ItemButtonList {
		get => m_ItemButtomList;
	}
	Stack<Button> ItemButtonPool {
		get => m_ItemButtomPool;
	}

	(Button button, float time) Input {
		get => m_Input;
		set => m_Input = value;
	}



	// Methods

	Button GetOrCreateInstance() {
		Button instance;
		while (ItemButtonPool.TryPop(out instance) && instance == null);
		if (instance == null) instance = Instantiate(ItemButtonTemplate, Anchor.transform);
		instance.gameObject.SetActive(true);
		return instance;
	}

	void RemoveInstance(Button instance) {
		instance.gameObject.SetActive(false);
		ItemButtonPool.Push(instance);
	}

	bool TryGetIconImage(Button button, out Image image) {
		return button.transform.GetChild(0).TryGetComponent(out image);
	}
	bool TryGetNameText(Button button, out TextMeshProUGUI text) {
		return button.transform.GetChild(1).TryGetComponent(out text);
	}
	bool TryGetDataText(Button button, out TextMeshProUGUI text) {
		return button.transform.GetChild(2).TryGetComponent(out text);
	}
	bool TryGetCurrencyImage(Button button, out Image image) {
		return button.transform.GetChild(3).TryGetComponent(out image);
	}
	void RefreshLayout(TextMeshProUGUI text, Image icon) {
		var textTransform = (RectTransform)text.transform;
		var iconTransform = (RectTransform)icon.transform;
		float textWidth = text.preferredWidth;
		float iconWidth = iconTransform.rect.width;
		float textPivot = textTransform.anchoredPosition.x - (textWidth * 1.0f);
		float iconPositionX = textPivot - TextIconMargin - (iconWidth * 0.0f);
		float iconPositionY = iconTransform.anchoredPosition.y;
		iconTransform.anchoredPosition = new(iconPositionX, iconPositionY);
	}



	/*
	구매 목록 표시 함수
	*/

	void RefreshListPanelForBuy() {
		int count = 0;
		Button GetInstance() {
			if (ItemButtonList.Count == count) ItemButtonList.Add(GetOrCreateInstance());
			return ItemButtonList[count++];
		}

		if (ItemList != null) for (int i = 0; i < ItemList.Count; i++) {
			int index = i;
			var instance = GetInstance();
			var transform = (RectTransform)instance.transform;
			var position = transform.anchoredPosition;
			position.y = -transform.rect.height * index;
			transform.anchoredPosition = position;
			var entry = ItemList[index];
			var name = entry.item.ItemName;
			var text = entry.item.ItemDescription;
			var icon = entry.item.ItemIconSprite;
			if (TryGetIconImage(instance, out var iconImage)) iconImage.sprite = icon;
			if (TryGetNameText(instance, out var nameText)) nameText.text = name;
			if (TryGetDataText(instance, out var dataText)) {
				dataText.text = $"{entry.price}";
				if (TryGetCurrencyImage(instance, out var currencyImage)) {
					RefreshLayout(dataText, currencyImage);
				}
			}
			instance.onClick.RemoveAllListeners();
			instance.onClick.AddListener(() => {
				bool match = true;
				match = match && Input.button == instance;
				match = match && Time.time - Input.time < DoubleClickThreshold;
				match = match && entry.price < GameManager.IntValue["Gem"];
				match = match && 0 < entry.quantity;
				if (match) {
					GameManager.IntValue["Gem"] -= entry.price;
					entry.quantity--;
					ItemList[index] = entry;
					AudioManager.PlaySoundFX(Audio.Shop, 0.3f);
					/*
					구매 성공 처리
					- ItemData -> ItemBase 변환 추가 필요
					*/
					OnBuyCompleted(entry.item);
					RefreshDetailPanel(name, text, icon, entry.price, entry.quantity);
					RefreshListPanelForBuy();
					Input = default;
				} else {
					RefreshDetailPanel(name, text, icon, entry.price, entry.quantity);
					Input = (instance, Time.time);
				}
			});
		}

		if (FeatureList != null) for (int i = 0; i < FeatureList.Count; i++) {
			int index = i;
			var instance = GetInstance();
			var transform = (RectTransform)instance.transform;
			var position = transform.anchoredPosition;
			position.y = -transform.rect.height * (index + (ItemList?.Count ?? 0));
			transform.anchoredPosition = position;
			var entry = FeatureList[index];
			var name = entry.name;
			var text = entry.text;
			var icon = entry.icon;
			if (TryGetIconImage(instance, out var iconImage)) iconImage.sprite = icon;
			if (TryGetNameText(instance, out var nameText)) nameText.text = name;
			if (TryGetDataText(instance, out var dataText)) {
				dataText.text = $"{entry.price}";
				if (TryGetCurrencyImage(instance, out var currencyImage)) {
					RefreshLayout(dataText, currencyImage);
				}
			}
			instance.onClick.RemoveAllListeners();
			instance.onClick.AddListener(() => {
				bool match = true;
				match = match && Input.button == instance;
				match = match && Time.time - Input.time < DoubleClickThreshold;
				match = match && entry.price < GameManager.IntValue["Gem"];
				match = match && 0 < entry.quantity;
				if (match) {
					GameManager.IntValue["Gem"] -= entry.price;
					entry.quantity--;
					FeatureList[index] = entry;
					AudioManager.PlaySoundFX(Audio.Shop, 0.3f);
					entry.action?.Invoke();
					RefreshDetailPanel(name, text, icon, entry.price, entry.quantity);
					RefreshListPanelForBuy();
					Input = default;
				} else {
					RefreshDetailPanel(name, text, icon, entry.price, entry.quantity);
					Input = (instance, Time.time);
				}
			});
		}

		while (count < ItemButtonList.Count) {
			RemoveInstance(ItemButtonList[count]);
			ItemButtonList.RemoveAt(count);
		}
		var anchorTransform = (RectTransform)Anchor.transform;
		var buttonTransform = (RectTransform)ItemButtonTemplate.transform;
		var sizeDelta = anchorTransform.sizeDelta;
		sizeDelta.y = buttonTransform.rect.height * count;
		anchorTransform.sizeDelta = sizeDelta;
		DefaultSelected = (0 < count) ? ItemButtonList[0] : null;
	}

	/*
	판매 목록 표시 함수
	*/

	void RefreshListPanelForSell() {
		int count = 0;
		Button GetInstance() {
			if (ItemButtonList.Count == count) ItemButtonList.Add(GetOrCreateInstance());
			return ItemButtonList[count++];
		}

		int itemTypeCount = Enum.GetValues(typeof(ItemType)).Length;
		for (int i = 0; i < itemTypeCount; i++) {
			var itemType = (ItemType)i;
			var inventory = InventoryManager.Instance.GetInventory(itemType);
			foreach (var item in inventory.Items) {
				/*
				판매 불가 아이템 필터 추가 필요
				예) if (!item.ItemData.IsSellable) continue;
				*/
				int index = count;
				var instance = GetInstance();
				var transform = (RectTransform)instance.transform;
				var position = transform.anchoredPosition;
				position.y = -transform.rect.height * index;
				transform.anchoredPosition = position;
				var name = item.ItemData.ItemName;
				var text = item.ItemData.ItemDescription;
				var icon = item.ItemData.ItemIconSprite;
				if (TryGetIconImage(instance, out var iconImage)) iconImage.sprite = icon;
				if (TryGetNameText(instance, out var nameText)) nameText.text = name;
				/*
				판매가 설정 필요
				예) int price = item.ItemData.ItemSellPrice;
				*/
				int price = 1;
				if (TryGetDataText(instance, out var dataText)) {
					dataText.text = $"{price}";
					if (TryGetCurrencyImage(instance, out var currencyImage)) {
						RefreshLayout(dataText, currencyImage);
					}
				}
				instance.onClick.RemoveAllListeners();
				instance.onClick.AddListener(() => {
					bool match = true;
					match = match && Input.button == instance;
					match = match && Time.time - Input.time < DoubleClickThreshold;
					if (match) {
						AudioManager.PlaySoundFX(Audio.Shop, 0.3f);
						OnSellCompleted(item);
						RefreshDetailPanel(name, text, icon, price, item.Count);
						RefreshListPanelForSell();
						Input = default;
					} else {
						RefreshDetailPanel(name, text, icon, price, item.Count);
						Input = (instance, Time.time);
					}
				});
			}
		}

		while (count < ItemButtonList.Count) {
			RemoveInstance(ItemButtonList[count]);
			ItemButtonList.RemoveAt(count);
		}
		var anchorTransform = (RectTransform)Anchor.transform;
		var buttonTransform = (RectTransform)ItemButtonTemplate.transform;
		var sizeDelta = anchorTransform.sizeDelta;
		sizeDelta.y = buttonTransform.rect.height * count;
		anchorTransform.sizeDelta = sizeDelta;
		DefaultSelected = (0 < count) ? ItemButtonList[0] : null;
	}



	void RefreshDetailPanel(string name, string text, Sprite icon, int price, int quantity) {
		DetailPanelIconImage.sprite = icon;
		DetailPanelNameText.text = name;
		DetailPanelTextText.text = text;
		DetailPanelDataText.text = (0 < quantity) ? $"{quantity}개 남음" : "품절";
	}

	public void BuySelectedItem() {
		if (Input.button != null) {
			Input = (Input.button, Time.time);
			Input.button.onClick.Invoke();
		}
	}

	public void SellSelectedItem() {
		if (Input.button != null) {
			Input = (Input.button, Time.time);
			Input.button.onClick.Invoke();
		}
	}

	/*
	아이템 구매/판매 완료 콜백 함수
	*/

	void OnBuyCompleted(ItemData itemData) {
		/*
		인벤토리 내 아이템 +1 로직 추가 필요
		*/
		var itemBase = ItemDatabase.Instance.CreateItem(itemData, 1);
		InventoryManager.Instance.AddItem(itemBase);
	}

	void OnSellCompleted(ItemBase itemBase) {
		/*
		인벤토리 내 아이템 -1 로직 추가 필요
		*/
		InventoryManager.Instance.RemoveItem(itemBase);
	}



	// Screen Methods

	public override void Show() {
		base.Show();
		IsBuying = true;
	}
}
