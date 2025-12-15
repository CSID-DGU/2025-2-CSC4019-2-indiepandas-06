using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Shop Owner
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class ShopOwner : MonoBehaviour {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(ShopOwner))]
	class ShopOwnerEditor : EditorExtensions {
		ShopOwner I => target as ShopOwner;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Items", EditorStyles.boldLabel);
			BeginVertical(EditorStyles.helpBox);
			for (int i = 0; i < I.ItemList.Count; i++) {
				BeginHorizontal();
				var entry = I.ItemList[i];
				PrefixLabel(entry.item ? entry.item.ItemName : "Empty");
				entry.item = ObjectField(entry.item);
				entry.price = EditorGUILayout.IntField(entry.price, GUILayout.Width(50));
				entry.quantity = EditorGUILayout.IntField(entry.quantity, GUILayout.Width(50));
				if (Button("-", GUILayout.Width(20))) {
					I.ItemList.RemoveAt(i);
					break;
				}
				EndHorizontal();
				I.ItemList[i] = entry;
			}
			if (Button("Add Item")) I.ItemList.Add(new ItemEntry {
				quantity = 1,
			});
			EndVertical();
			Space();
			LabelField("Features", EditorStyles.boldLabel);
			BeginVertical(EditorStyles.helpBox);
			for (int i = 0; i < I.FeatureList.Count; i++) {
				BeginHorizontal();
				var entry = I.FeatureList[i];
				PrefixLabel(string.IsNullOrEmpty(entry.name) ? "Empty" : entry.name);
				entry.icon = ObjectField(entry.icon);
				entry.price = EditorGUILayout.IntField(entry.price, GUILayout.Width(50));
				entry.quantity = EditorGUILayout.IntField(entry.quantity, GUILayout.Width(50));
				if (Button("-", GUILayout.Width(20))) {
					I.FeatureList.RemoveAt(i);
					break;
				}
				EndHorizontal();
				IntentLevel++;
				entry.name = TextField("Feature Name", entry.name);
				entry.text = TextField("Feature Description", entry.text);
				if (i < 8) switch (i) {
					case 0: I.E0 = entry.action; PropertyField("E0"); entry.action = I.E0; break;
					case 1: I.E1 = entry.action; PropertyField("E1"); entry.action = I.E1; break;
					case 2: I.E2 = entry.action; PropertyField("E2"); entry.action = I.E2; break;
					case 3: I.E3 = entry.action; PropertyField("E3"); entry.action = I.E3; break;
					case 4: I.E4 = entry.action; PropertyField("E4"); entry.action = I.E4; break;
					case 5: I.E5 = entry.action; PropertyField("E5"); entry.action = I.E5; break;
					case 6: I.E6 = entry.action; PropertyField("E6"); entry.action = I.E6; break;
					case 7: I.E7 = entry.action; PropertyField("E7"); entry.action = I.E7; break;
				}
				IntentLevel--;
				Space();
				I.FeatureList[i] = entry;
			}
			if (Button("Add Feature")) I.FeatureList.Add(new FeatureEntry {
				action = new UnityEvent(),
				quantity = 1,
			});
			EndVertical();
			Space();

			End();
		}
	}
	#endif



	// Fields

	[SerializeField] List<ItemEntry> m_ItemList = new();
	[SerializeField] List<FeatureEntry> m_FeatureList = new();

	#if UNITY_EDITOR
	[SerializeField] UnityEvent E0;
	[SerializeField] UnityEvent E1;
	[SerializeField] UnityEvent E2;
	[SerializeField] UnityEvent E3;
	[SerializeField] UnityEvent E4;
	[SerializeField] UnityEvent E5;
	[SerializeField] UnityEvent E6;
	[SerializeField] UnityEvent E7;
	#endif



	// Properties

	public InteractionType InteractionType => InteractionType.Trade;

	public bool IsInteractable => true;



	List<ItemEntry> ItemList {
		get => m_ItemList;
	}
	List<FeatureEntry> FeatureList {
		get => m_FeatureList;
	}



	// Methods

	public void OpenShop() {
		UIManager.ShopItemList = ItemList;
		UIManager.ShopFeatureList = FeatureList;
		UIManager.OpenScreen(Screen.Shop);
	}
}
