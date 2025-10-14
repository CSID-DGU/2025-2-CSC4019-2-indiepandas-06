using UnityEngine;
using System;
using System.Collections.Generic;



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Hash Map
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Serializable]
public class HashMap<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver {

	// Fields

	[SerializeField] List<K> k = new();
	[SerializeField] List<V> v = new();



	// Methods

	public void OnBeforeSerialize() {
		k.Clear();
		v.Clear();
		foreach (var (k, v) in this) {
			this.k.Add(k);
			this.v.Add(v);
		}
	}

	public void OnAfterDeserialize() {
		Clear();
		for (int i = 0; i < k.Count; i++) Add(k[i], v[i]);
	}
}
