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
	public float price;
	public int quantity;
}

[Serializable]
public struct FeatureEntry {
	public Sprite icon;
	public string name;
	public string text;
	public UnityEvent action;
	public float price;
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
			I.BuyScrollview  = ObjectField("Buy Scrollview",  I.BuyScrollview);
			I.SellScrollview = ObjectField("Sell Scrollview", I.SellScrollview);
			I.BuyButton      = ObjectField("Buy Button",      I.BuyButton);
			I.SellButton     = ObjectField("Sell Button",     I.SellButton);
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

	[SerializeField] GameObject m_BuyScrollview;
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



	public List<ItemEntry> ItemList {
		get => m_ItemList;
		set => m_ItemList = value;
	}
	public List<FeatureEntry> FeatureList {
		get => m_FeatureList;
		set => m_FeatureList = value;
	}

	GameObject BuyScrollview {
		get => m_BuyScrollview;
		set => m_BuyScrollview = value;
	}
	GameObject SellScrollview {
		get => m_SellScrollview;
		set => m_SellScrollview = value;
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
		get => BuyScrollview.activeSelf;
		set {
			BuyScrollview.SetActive(value);
			SellScrollview.SetActive(!value);
			BuyButton.SetActive(value);
			SellButton.SetActive(!value);
			Input = default;
			if (value) {
				RefreshListPanel();
				if (0 < ItemButtonList.Count) ItemButtonList[0].onClick.Invoke();
				else RefreshDetailPanel(null, null, null, 0, 0);
			} else {
				RefreshDetailPanel(null, null, null, 0, 0);
			}
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



	void RefreshDetailPanel(string name, string text, Sprite icon, float price, int quantity) {
		DetailPanelIconImage.sprite = icon;
		DetailPanelNameText.text = name;
		DetailPanelTextText.text = text;
		DetailPanelDataText.text = (0 < quantity) ? $"{quantity}개 남음" : "품절";
	}

	void RefreshListPanel() {
		int count = 0;
		Button GetInstance() {
			if (ItemButtonList.Count == count) ItemButtonList.Add(GetOrCreateInstance());
			return ItemButtonList[count++];
		}
		const int Up = 0, Down = 1, Left = 2, Right = 3;
		void SetNavigation(Selectable selectable, Selectable target, int direction) {
			var navigation = selectable.navigation;
			switch (direction) {
				case Up:    navigation.selectOnUp    = target; break;
				case Down:  navigation.selectOnDown  = target; break;
				case Left:  navigation.selectOnLeft  = target; break;
				case Right: navigation.selectOnRight = target; break;
			}
			selectable.navigation = navigation;
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

		if (ItemList != null) for (int i = 0; i < ItemList.Count; i++) {
			int index = i;
			var instance = GetInstance();
			var transform = (RectTransform)instance.transform;
			var position = transform.anchoredPosition;
			position.y = -transform.rect.height * index;
			transform.anchoredPosition = position;
			instance.navigation = new Navigation { mode = Navigation.Mode.Explicit };
			if (0 < index) {
				SetNavigation(instance, ItemButtonList[index - 1], Up);
				SetNavigation(ItemButtonList[index - 1], instance, Down);
			}
			var entry = ItemList[index];
			var name = entry.item.ItemName;
			var text = "아이템 설명";
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
				match = match && 0 < entry.quantity;
				if (match) {
					entry.quantity--;
					ItemList[index] = entry;
					OnItemPurchased(entry.item);
					RefreshDetailPanel(name, text, icon, entry.price, entry.quantity);
					RefreshListPanel();
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
			instance.navigation = new Navigation { mode = Navigation.Mode.Explicit };
			if (0 < index) {
				SetNavigation(instance, ItemButtonList[index - 1], Up);
				SetNavigation(ItemButtonList[index - 1], instance, Down);
			}
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
				match = match && 0 < entry.quantity;
				if (match) {
					entry.quantity--;
					FeatureList[index] = entry;
					entry.action?.Invoke();
					RefreshDetailPanel(name, text, icon, entry.price, entry.quantity);
					RefreshListPanel();
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



	public void PurchaseSelectedItem() {
		if (Input.button != null) {
			Input = (Input.button, Time.time);
			Input.button.onClick.Invoke();
		}
	}

	void OnItemPurchased(ItemData item) {
		Debug.Log($"아이템 {item.ItemName} 구매");
	}



	// Screen Methods

	public override void Show() {
		base.Show();
		IsBuying = true;
	}
}
