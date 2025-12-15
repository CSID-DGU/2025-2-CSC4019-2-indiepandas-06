using UnityEngine;
using UnityEngine.UI;
using System;



public enum BackgroundMode {
	Scene,
	SceneBlur,
	StaticBackground,
	PreserveWithBlur,
	Preserve,
}

public enum InputPolicy {
	PlayerOnly,
	UIOnly,
	Both,
}


// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Screen Base
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[DisallowMultipleComponent]
public abstract class ScreenBase : MonoBehaviour {

	// Fields

	[SerializeField] Selectable m_DefaultSelected;
	Selectable m_CurrentSelected;



	// Properties

	public virtual bool IsPrimary => false;
	public virtual bool IsOverlay => true;
	public virtual BackgroundMode BackgroundMode => BackgroundMode.Preserve;
	public virtual InputPolicy InputPolicy => InputPolicy.UIOnly;
	/*
	Is Primary: 참일 경우 이 스크린이 열릴 때 다른 스크린들을 전부 닫음
	Is Overlay: 참일 경우 이 스크린이 열린 상태에서 다른 스크린이 열리면 이 스크린은 가려짐
	Background Mode: 스크린이 사용할 배경
		- Scene: 카메라가 렌더링한 장면
		- Static Background: UIManager의 Static Background 이미지
		- Preserve With Blur: 현재 화면 유지, 단 Scene 모드일 경우 블러 처리
		- Preserve: 현재 화면 유지

		Scene <-> Static Background 전환 시 이전 배경 스크린들은 완전히 가려짐
		Preserve일 경우 이전 스크린의 배경 유지

		예시: Game Screen 위에 Menu Screen을 열 경우
			Scene -> Preserve With Blur
			Scene 배경 유지, 게임 화면만 블러처리

		예시: Game Screen 위에 Options Screen을 열 경우
			Scene -> Static Background
			Game Screen의 Overlay 여부에 관계없이 가려짐
	*/



	public Selectable DefaultSelected {
		get => m_DefaultSelected;
		set => m_DefaultSelected = value;
	}
	public Selectable CurrentSelected {
		get => m_CurrentSelected;
		set => m_CurrentSelected = value;
	}



	// Methods

	public virtual void Show() {
		var selected = CurrentSelected ? CurrentSelected : DefaultSelected;
		UIManager.Selected = InputManager.IsPointerMode ? null : selected;
		transform.SetAsLastSibling();
		gameObject.SetActive(true);
	}

	public virtual void Hide() {
		UIManager.Selected = null;
		gameObject.SetActive(false);
	}

	public virtual void Back() {
		UIManager.CloseScreen(this);
	}



	// Lifecycle

	protected virtual void Update() {
		if (UIManager.CurrentScreen == this.ToScreen()) {
			if (InputManager.GetKeyUp(KeyAction.Cancel)) Back();
			if (InputManager.Navigate != Vector2.zero) {
				if (UIManager.Selected == null) UIManager.Selected = DefaultSelected;
				if (UIManager.Selected) EnsureSelectedVisible();
				CurrentSelected = UIManager.Selected;
			}
		}
	}

	static void EnsureSelectedVisible() {
		var transform = UIManager.Selected.transform;
		var component = GetParentComponentRecursive(transform, typeof(ScrollRect));
		if (component is ScrollRect scrollRect) {
			var selectedTransform = (RectTransform)transform;
			var viewportTransform = scrollRect.viewport;
			var viewRect = viewportTransform.rect;
			var contentTransform = scrollRect.content;
			var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
				viewportTransform, selectedTransform);
			float xDelta = 0f;
			if (scrollRect.horizontal) {
				if (bounds.min.x < viewRect.xMin) xDelta = viewRect.xMin - bounds.min.x;
				if (bounds.max.x > viewRect.xMax) xDelta = viewRect.xMax - bounds.max.x;
			}
			float yDelta = 0f;
			if (scrollRect.vertical) {
				if (bounds.min.y < viewRect.yMin) yDelta = viewRect.yMin - bounds.min.y;
				if (bounds.max.y > viewRect.yMax) yDelta = viewRect.yMax - bounds.max.y;
			}
			if (xDelta != 0f || yDelta != 0f) {
				contentTransform.anchoredPosition += new Vector2(xDelta, yDelta);
			}
		}
	}

	static Component GetParentComponentRecursive(Transform child, Type type) {
		if (child.TryGetComponent(type, out var component)) return component;
		if (child.parent) return GetParentComponentRecursive(child.parent, type);
		return null;
	}
}
