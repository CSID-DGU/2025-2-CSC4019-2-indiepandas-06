using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using NavMeshPlus.Components;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Navigation Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/Navigation Manager")]
[RequireComponent(typeof(NavMeshSurface))]
public sealed class NavigationManager : MonoSingleton<NavigationManager> {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(NavigationManager))]
	class NavigationManagerEditor : EditorExtensions {
		NavigationManager I => target as NavigationManager;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Navigation", EditorStyles.boldLabel);
			BeginHorizontal();
			PrefixLabel("Bake NavMesh");
			if (Button("Clear All")) ClearAll();
			if (Button("Bake All")) BakeAll();
			EndHorizontal();

			End();
		}
	}
	#endif



	// Constants

	const float SampleDistance = 1.0f;
	const float SampleMaxRange = 5.0f;



	// Fields

	NavMeshPath m_Path;



	// Properties

	static NavMeshSurface[] Surfaces {
		get => Instance.GetComponents<NavMeshSurface>();
	}
	static NavMeshPath Path {
		get => Instance.m_Path;
		set => Instance.m_Path = value;
	}



	// Methods

	public static void ClearAll() {
		foreach (var surface in Surfaces) surface.RemoveData();
	}

	public static void BakeAll() {
		foreach (var surface in Surfaces) surface.BuildNavMesh();
	}



	public static bool TryGetPath(Vector3 source, Vector3 target, Queue<Vector3> path) {
		Path ??= new NavMeshPath();
		Path.ClearCorners();
		if (!NavMesh.CalculatePath(source, target, NavMesh.AllAreas, Path)) return false;
		if (Path.status != NavMeshPathStatus.PathComplete) return false;

		path.Clear();
		for (int i = 0; i < Path.corners.Length - 1; i++) {
			float distance = Vector3.Distance(Path.corners[i], Path.corners[i + 1]);
			for (float s = 0; s < distance; s += SampleDistance) {
				Vector3 point = Vector3.Lerp(Path.corners[i], Path.corners[i + 1], s / distance);
				if (NavMesh.SamplePosition(point, out var hit, SampleMaxRange, NavMesh.AllAreas)) {
					path.Enqueue(hit.position);
				}
			}
		}
		return true;
	}
}
