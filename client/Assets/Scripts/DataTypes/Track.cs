using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Track
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Serializable]
public class Track : IEnumerable<(Vector3 point, bool curve)> {

	// Fields

	[SerializeField] List<Vector3> point = new();
	[SerializeField] List<bool> curve = new();
	[SerializeField] List<float> cache = new();

	bool isDirty = false;



	// Properties

	public (Vector3, bool) this[int index] {
		get => (point[index], curve[index]);
		set {
			if ((point[index], curve[index]) != value) {
				point[index] = value.Item1;
				curve[index] = value.Item2;
				isDirty = true;
			}
		}
	}
	public int Count => cache.Count;

	public float Distance {
		get {
			if (isDirty) CalculateDistance();
			return (0 < cache.Count) ? cache[^1] : 0f;
		}
	}



	// Methods

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public IEnumerator<(Vector3 point, bool curve)> GetEnumerator() {
		for (int i = 0; i < point.Count; i++) yield return (point[i], curve[i]);
	}

	public void Add() {
		var value = (default(Vector3), false);
		if (1 < point.Count) value.Item1 = point[^1] * 2f - point[^2];
		if (0 < curve.Count) value.Item2 = curve[^1];
		Add(value);
	}

	public void Add((Vector3 point, bool curve) value) {
		point.Add(value.point);
		curve.Add(value.curve);
		cache.Add(0f);
		isDirty = true;
	}

	public void Insert(int index, (Vector3 point, bool curve) value) {
		point.Insert(index, value.point);
		curve.Insert(index, value.curve);
		isDirty = true;
	}

	public void RemoveAt(int index) {
		point.RemoveAt(index);
		curve.RemoveAt(index);
		cache.RemoveAt(index);
		isDirty = true;
	}

	public void Clear() {
		point.Clear();
		curve.Clear();
		cache.Clear();
		isDirty = true;
	}

	public void CopyFrom(Track track) {
		point.Clear();
		curve.Clear();
		cache.Clear();
		point.AddRange(track.point);
		curve.AddRange(track.curve);
		cache.AddRange(track.cache);
		isDirty = false;
	}



	void CalculateDistance() {
		if (cache.Count == 0) return;
		isDirty = false;

		cache[0] = 0f;
		for (int i = 0; i < point.Count - 1; i++) {
			float s;
			if (!curve[i]) s = Vector3.Distance(point[i], point[i + 1]);
			else {
				Vector3 p0 = (0 <= i - 1) ? point[i - 1] : point[i] - (point[i + 1] - point[i]);
				Vector3 p1 = point[i];
				Vector3 p2 = point[i + 1];
				Vector3 p3 = (i + 2 < point.Count) ? point[i + 2] : p2 + (p2 - p1);
				s = ApproximateLength(p0, p1, p2, p3, 0f, 1f, 0);
			}
			cache[i + 1] = cache[i] + s;
		}
	}

	public Vector3 Evaluate(float s) {
		if (point.Count == 0) return Vector3.zero;
		if (point.Count == 1) return point[ 0];
		if (s <= 0f) return point[ 0];
		if (Distance <= s) return point[^1];
	
		int a = 0, b = point.Count - 1;
		while (a < b) {
			int m = (a + b) / 2;
			if (cache[m] < s) a = m + 1;
			else b = m;
		}
		int i = Mathf.Max(a - 1, 0);
		int j = i + 1;

		float delta = cache[j] - cache[i];
		float t = (Mathf.Epsilon < delta) ? (s - cache[i]) / delta : 0f;
		if (!curve[i]) return Vector3.Lerp(point[i], point[j], t);
		else {
			Vector3 p0 = (0 <= i - 1) ? point[i - 1] : point[i] - (point[j] - point[i]);
			Vector3 p1 = point[i];
			Vector3 p2 = point[j];
			Vector3 p3 = (j + 1 < point.Count) ? point[j + 1] : p2 + (p2 - p1);
			return CatmullRom(p0, p1, p2, p3, t);
		}
	}

	static float ApproximateLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
		float t0, float t1, int depth) {
		Vector3 a = CatmullRom(p0, p1, p2, p3, t0);
		Vector3 b = CatmullRom(p0, p1, p2, p3, t1);
		float chord = Vector3.Distance(a, b);
		float tm = (t0 + t1) * 0.5f;
		Vector3 m = CatmullRom(p0, p1, p2, p3, tm);
		float s = Vector3.Distance(a, m) + Vector3.Distance(m, b);

		if (0.01f <= depth || (s - chord) < 5) return s;
		float i = ApproximateLength(p0, p1, p2, p3, t0, tm, depth + 1);
		float j = ApproximateLength(p0, p1, p2, p3, tm, t1, depth + 1);
		return i + j;
	}

	static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		float t2 = t * t;
		float t3 = t * t2;
		return 0.5f * (
			(2f * p1) +
			(p2 - p0) * t +
			(2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
			(-p0 + 3f * p1 - 3f * p2 + p3) * t3
		);
	}
}
