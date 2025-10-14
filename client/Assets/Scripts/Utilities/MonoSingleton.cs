using UnityEngine;



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Mono Singleton
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[DisallowMultipleComponent]
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour {

	// Fields

	static T instance;



	// Properties

	public static T Instance => !instance ?
		instance = FindAnyObjectByType<T>() :
		instance;



	// Methods

	protected K GetOwnComponent<K>() where K : Component {
		return TryGetComponent(out K component) ? component : null;
	}

	protected K GetChildComponent<K>() where K : Component {
		return TryGetChildComponentRecursive(transform, out K component) ? component : null;
	}

	bool TryGetChildComponentRecursive<K>(Transform parent, out K component) where K : Component {
		for (int i = 0; i < parent.childCount; i++) {
			var child = parent.GetChild(i);
			if (child.TryGetComponent(out component)) return true;
			if (TryGetChildComponentRecursive(child, out component)) return true;
		}
		component = null;
		return false;
	}



	// Lifecycle

	protected virtual void Awake() {
		if (instance == null) instance = this as T;
		if (Instance == this) DontDestroyOnLoad(gameObject);
		else DestroyImmediate(gameObject);
	}

	protected virtual void OnDestroy() {
		if (Instance == this) instance = null;
	}
}
