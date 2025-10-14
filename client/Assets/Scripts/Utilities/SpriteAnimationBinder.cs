using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Sprite Animation Binder
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Component/Sprite Animation Binder")]
[RequireComponent(typeof(Image), typeof(SpriteRenderer), typeof(Animator))]
public class SpriteAnimationBinder : MonoBehaviour {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(SpriteAnimationBinder))]
	class SpriteAnimationBinderEditor : EditorExtensions {
		SpriteAnimationBinder I => target as SpriteAnimationBinder;
		public override void OnInspectorGUI() {
			Begin("Sprite Animation Binder");

			End();
		}
	}
	#endif



	// Fields

	SpriteRenderer m_Renderer; 
	Image m_Image;



	// Properties

	SpriteRenderer Renderer => m_Renderer || TryGetComponent(out m_Renderer) ? m_Renderer : null;
	Image Image => m_Image || TryGetComponent(out m_Image) ? m_Image : null;



	// Lifecycle

	void LateUpdate() {
		var sprite = Renderer.sprite;
		if (sprite) {
			Image.sprite = sprite;
			Image.SetNativeSize();
			Image.rectTransform.pivot = new Vector2(
				Image.sprite.pivot.x / Image.sprite.rect.width,
				Image.sprite.pivot.y / Image.sprite.rect.height
			);
		}
	}
}
