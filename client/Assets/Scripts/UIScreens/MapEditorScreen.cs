using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Map Editor Screen
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI Screen/Map Editor Screen")]
public sealed class MapEditorScreen : ScreenBase {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(MapEditorScreen))]
	class MapEditorScreenEditor : EditorExtensions {
		MapEditorScreen I => target as MapEditorScreen;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Selected", EditorStyles.boldLabel);
			I.DefaultSelected = ObjectField("Default Selected", I.DefaultSelected);
			Space();

			LabelField("Map Editor", EditorStyles.boldLabel);
			I.Anchor               = ObjectField("Anchor",          I.Anchor);
			I.PrefabButtonTemplate = ObjectField("Prefab Template", I.PrefabButtonTemplate);
			Space();

			End();
		}
	}
	#endif



	// Constants

	const float Margin = 4f;

	enum Drag : byte {
		None,
		Item,
		Camera,
	}



	// Fields

	[SerializeField] GameObject m_Anchor;
	[SerializeField] Button m_PrefabButtonTemplate;
	List<Button> m_PrefabButtonList = new();
	Stack<Button> m_PrefabButtonPool = new();
	Dictionary<GameObject, GameObject> m_ButtonPrefabPair = new();

	Vector2 m_PointPosition;
	GameObject m_PointedPrefab;
	Drag m_DragMode;



	// Properties

	public override bool IsPrimary => false;
	public override bool IsOverlay => true;
	public override BackgroundMode BackgroundMode => BackgroundMode.Scene;



	GameObject Anchor {
		get => m_Anchor;
		set => m_Anchor = value;
	}
	Button PrefabButtonTemplate {
		get => m_PrefabButtonTemplate;
		set => m_PrefabButtonTemplate = value;
	}
	List<Button> PrefabButtonList {
		get => m_PrefabButtonList;
	}
	Stack<Button> PrefabButtonPool {
		get => m_PrefabButtonPool;
	}
	Dictionary<GameObject, GameObject> InstancePrefabPair {
		get => m_ButtonPrefabPair;
	}



	Vector2 PointPosition {
		get => m_PointPosition;
		set => m_PointPosition = value;
	}
	GameObject PointedPrefab {
		get => m_PointedPrefab;
		set => m_PointedPrefab = value;
	}
	bool IsPointingUI {
		get => EventSystem.current.IsPointerOverGameObject();
	}

	Drag DragMode {
		get => m_DragMode;
		set => m_DragMode = value;
	}
	bool IsDraggingItem   => DragMode == Drag.Item;
	bool IsDraggingCamera => DragMode == Drag.Camera;



	// Methods

	Button GetOrCreateInstance() {
		Button instance;
		while (PrefabButtonPool.TryPop(out instance) && instance == null);
		if (instance == null) instance = Instantiate(PrefabButtonTemplate, Anchor.transform);
		instance.gameObject.SetActive(true);
		return instance;
	}

	void RemoveInstance(Button instance) {
		instance.gameObject.SetActive(false);
		PrefabButtonPool.Push(instance);
	}



	void Refresh() {
		int count = 0;
		Button GetInstance() {
			if (PrefabButtonList.Count == count) PrefabButtonList.Add(GetOrCreateInstance());
			return PrefabButtonList[count++];
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
			return button.TryGetComponent(out image);
		}

		InstancePrefabPair.Clear();
		var prefabs = Resources.LoadAll<GameObject>("").Where(prefab => {
			return !prefab.name.StartsWith("Debug");
		}).ToArray();
		for (int i = 0; i < prefabs.Length; i++) {
			int index = i;
			var instance = GetInstance();
			var transform = (RectTransform)instance.transform;
			var position = transform.anchoredPosition;
			position.x = Margin + (transform.rect.width + Margin) * index;
			transform.anchoredPosition = position;
			instance.navigation = new Navigation { mode = Navigation.Mode.Explicit };
			if (0 < index) {
				SetNavigation(instance, PrefabButtonList[index - 1], Left);
				SetNavigation(PrefabButtonList[index - 1], instance, Right);
			}
			instance.name = prefabs[index].name;
			if (TryGetIconImage(instance, out var image)) {
				if (prefabs[index].TryGetComponent(out SpriteRenderer renderer)) {
					image.sprite = renderer.sprite;
					image.preserveAspect = true;
				}
			}
			InstancePrefabPair[instance.gameObject] = prefabs[index];
		}

		while (count < PrefabButtonList.Count) {
			RemoveInstance(PrefabButtonList[count]);
			PrefabButtonList.RemoveAt(count);
		}
		var anchorTransform = (RectTransform)Anchor.transform;
		var buttonTransform = (RectTransform)PrefabButtonTemplate.transform;
		var sizeDelta = buttonTransform.sizeDelta;
		sizeDelta.x = Margin + (buttonTransform.rect.width + Margin) * count;
		anchorTransform.sizeDelta = sizeDelta;
		DefaultSelected = (0 < count) ? PrefabButtonList[0] : null;
	}



	// Screen Methods

	public override void Show() {
		base.Show();
		Refresh();
	}



	// Lifecycle

	void LateUpdate() {
		if (InputManager.GetKeyDown(KeyAction.Click)) {
			PointPosition = InputManager.PointPosition;
			if (IsPointingUI) {
				var eventData = new PointerEventData(EventSystem.current) { position = PointPosition };
				var results = new List<RaycastResult>();
				EventSystem.current.RaycastAll(eventData, results);
				bool match = true;
				match &= 0 < results.Count;
				match &= InstancePrefabPair.TryGetValue(results[0].gameObject, out var prefab);
				if (match) {
					PointedPrefab = prefab;
					DragMode = Drag.Item;
				}
			} else DragMode = Drag.Camera;
		}
		if (IsDraggingItem) {
			var position = CameraManager.ScreenToWorldPoint(InputManager.PointPosition);
			position.z = 0f;
			if (InputManager.GetKeyUp(KeyAction.Click) && !IsPointingUI) {
				Instantiate(PointedPrefab, position, Quaternion.identity);
			}
		}
		if (IsDraggingCamera) {
			if (InputManager.GetKey(KeyAction.Click) && PointPosition != InputManager.PointPosition) {
				var prev = CameraManager.ScreenToWorldPoint(PointPosition);
				var next = CameraManager.ScreenToWorldPoint(InputManager.PointPosition);
				var deltaPosition = next - prev;
				CameraManager.Position -= deltaPosition;
				PointPosition = InputManager.PointPosition;
			}
		}
		if (!InputManager.GetKey(KeyAction.Click)) DragMode = Drag.None;
	}
}
