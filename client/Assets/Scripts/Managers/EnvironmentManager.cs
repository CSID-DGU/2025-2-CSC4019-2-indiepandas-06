using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor.Animations;
using Random = UnityEngine.Random;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif



public enum Particle {
	SmokeTiny,
	SmokeSmall,
	SmokeMedium,
	SmokeLarge,
	Dust,
	Leaf,

	Glow,
	Flash,
	Spark,
	Lightning,

	Bubble,
	Snow,
}

public enum Pattern {
	FlipRandom,
	LinearFall,
	HasGravity,
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Environment Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/Environment Manager")]
[RequireComponent(typeof(Light2D))]
public class EnvironmentManager : MonoSingleton<EnvironmentManager> {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(EnvironmentManager))]
	class EnvironmentManagerEditor : EditorExtensions {
		static int Index = 0;
		EnvironmentManager I => target as EnvironmentManager;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Main Light", EditorStyles.boldLabel);
			BeginHorizontal();
			PrefixLabel("Day Color Intensity");
			DayColor = ColorField(DayColor);
			DayIntensity = EditorGUILayout.FloatField(DayIntensity, GUILayout.Width(60f));
			EndHorizontal();
			BeginHorizontal();
			PrefixLabel("Night Color Intensity");
			NightColor = ColorField(NightColor);
			NightIntensity = EditorGUILayout.FloatField(NightIntensity, GUILayout.Width(60f));
			EndHorizontal();
			DayLength = FloatField("Day Length",  DayLength);
			TimeOfDay = FloatField("Time of Day", TimeOfDay);
			IntentLevel++;
			Simulate = Toggle("Simulate", Simulate);
			IntentLevel--;
			Space();

			LabelField("Particle Instance", EditorStyles.boldLabel);
			ParticleTemplate = ObjectField("Particle Template", ParticleTemplate);
			if (ParticleTemplate == null) {
				var message = string.Empty;
				message += "Particle Template is missing.\n";
				message += "Please assign a Particle Template here.";
				HelpBox(message, MessageType.Info);
				Space();
			} else {
				int num = Particles.Count;
				int den = Particles.Count + ParticlePool.Count;
				LabelField("Particle Pool", $"{num} / {den}");
				Space();
			}

			LabelField("Particle Entry Data", EditorStyles.boldLabel);
			LabelField("Particle Entry Count", $"{ParticleCount}");
			if (EntryData.Length != ParticleCount) {
				int length = Mathf.Min(EntryData.Length, ParticleCount);
				var entryData = new ParticleEntry[ParticleCount];
				for (int i = 0; i < length; i++) entryData[i] = EntryData[i];
				EntryData = entryData;
			}
			int element = 5;
			int minPage = 0;
			int maxPage = Mathf.Max(minPage, (ParticleCount - 1) / element);
			Index = Mathf.Clamp(Index, minPage, maxPage);
			int min = Index * element;
			int max = Mathf.Min((Index + 1) * element, ParticleCount);
			for (int i = min; i < max; i++) {
				BeginVertical(EditorStyles.helpBox);
				ref var data = ref EntryData[i];
				data.Controller = ObjectField($"{(Particle)i}", data.Controller);
				data.Pattern = FlagField<Pattern>("Pattern", data.Pattern);
				data.Duration = Slider("Duration", data.Duration, 0.1f, 5f);
				if (data.Duration < 0.1f) data.Duration = 1f;
				EndVertical();
			}
			BeginHorizontal();
			FlexibleSpace();
			BeginDisabledGroup(Index <= minPage);
			if (Button("〈", GUILayout.Width(24f))) Index--;
			EndDisabledGroup();
			var page = $"{Index + 1} / {maxPage + 1}";
			var center = new GUIStyle(EditorStyles.label) {
				alignment = TextAnchor.MiddleCenter,
			};
			LabelField(page, center, GUILayout.Width(48));
			BeginDisabledGroup(maxPage <= Index);
			if (Button("〉", GUILayout.Width(24f))) Index++;
			EndDisabledGroup();
			FlexibleSpace();
			EndHorizontal();
			End();
		}
	}
	#endif



	// Constants

	static readonly int ParticleCount = Enum.GetValues(typeof(Particle)).Length;

	class InstanceEntry {
		public Particle Particle;
		public SpriteRenderer Renderer;
		public Animator Animator;
		public float EndTime;
		public Vector3 Velocity;
	}

	[Serializable]
	struct ParticleEntry {
		public AnimatorController Controller;
		public uint Pattern;
		public float Duration;
		public bool GetPattern(Pattern pattern) => (Pattern & (1u << (int)pattern)) != 0u;
	}



	const float LinearFallSpeed = 0.1f;



	// Fields

	[SerializeField] Color m_DayColor = new(1.0f, 1.0f, 1.0f, 0f);
	[SerializeField] Color m_NightColor = new(0.1f, 0.1f, 0.3f, 0f);
	[SerializeField] float m_DayIntensity = 1.0f;
	[SerializeField] float m_NightIntensity = 0.5f;

	Light2D m_MainLight;
	[SerializeField] float m_DayLength = 300f;
	[SerializeField] float m_TimeOfDay = 0.5f;
	[SerializeField] bool m_Simulate = true;

	[SerializeField] GameObject m_ParticleTemplate;
	Dictionary<uint, InstanceEntry> m_Particles = new();
	Stack<InstanceEntry> m_ParticlePool = new();
	List<uint> m_IDBuffer = new();
	uint m_NextID;

	[SerializeField] ParticleEntry[] m_EntryData = new ParticleEntry[ParticleCount];



	// Properties

	static Light2D MainLight => Instance.m_MainLight ||
		Instance.TryGetComponent(out Instance.m_MainLight) ?
		Instance.m_MainLight : null;

	static float DayIntensity {
		get => Instance.m_DayIntensity;
		set => Instance.m_DayIntensity = value;
	}
	static float NightIntensity {
		get => Instance.m_NightIntensity;
		set => Instance.m_NightIntensity = value;
	}
	static Color DayColor {
		get => Instance.m_DayColor;
		set => Instance.m_DayColor = value;
	}
	static Color NightColor {
		get => Instance.m_NightColor;
		set => Instance.m_NightColor = value;
	}

	public static float DayLength {
		get => Instance.m_DayLength;
		set => Instance.m_DayLength = value;
	}
	public static float TimeOfDay {
		get => Instance.m_TimeOfDay;
		set {
			if (Instance.m_TimeOfDay != value) {
				Instance.m_TimeOfDay = value;
				float t = value % 1f;
				float blend = t switch {
					>= 0.3f and < 0.7f => 1f,
					>= 0.2f and < 0.3f => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, 0.3f, t)),
					>= 0.7f and < 0.8f => Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.7f, 0.8f, t)),
					_ => 0f,
				};
				MainLight.color = Color.Lerp(NightColor, DayColor, blend);
				MainLight.intensity = Mathf.Lerp(NightIntensity, DayIntensity, blend);
			}
		}
	}
	public static bool Simulate {
		get => Instance.m_Simulate;
		set => Instance.m_Simulate = value;
	}



	static GameObject ParticleTemplate {
		get => Instance.m_ParticleTemplate;
		set => Instance.m_ParticleTemplate = value;
	}
	static Dictionary<uint, InstanceEntry> Particles => Instance.m_Particles;
	static Stack<InstanceEntry> ParticlePool => Instance.m_ParticlePool;

	static List<uint> IDBuffer => Instance.m_IDBuffer;

	static uint NextID {
		get => Instance.m_NextID;
		set => Instance.m_NextID = value;
	}



	static ParticleEntry[] EntryData {
		get => Instance.m_EntryData;
		set => Instance.m_EntryData = value;
	}



	// Instance Methods

	static (uint, InstanceEntry) GetOrCreateInstance(Particle particle) {
		if (!ParticlePool.TryPop(out var instance)) {
			var template = Instantiate(ParticleTemplate);
			instance = new InstanceEntry {
				Animator = template.TryGetComponent(out Animator animator) ? animator : null,
				Renderer = template.TryGetComponent(out SpriteRenderer renderer) ? renderer : null,
			};
		}
		var data = EntryData[(int)particle];

		if (instance.Renderer && data.GetPattern(Pattern.FlipRandom)) {
			instance.Renderer.flipX = Random.value < 0.5f;
			instance.Renderer.flipY = Random.value < 0.5f;
		}
		/*
		add initialization patterns
		*/
		if (instance.Animator && data.Controller) {
			instance.Animator.runtimeAnimatorController = data.Controller;
		}
		#if UNITY_EDITOR
		instance.Animator.name = $"Particle {particle}";
		#endif
		instance.Particle = particle;
		instance.Animator.gameObject.SetActive(true);
		instance.EndTime = Time.time + data.Duration;
		instance.Velocity = default;
		while (++NextID == default || Particles.ContainsKey(NextID));
		Particles.Add(NextID, instance);
		return (NextID, instance);
	}

	static void UpdateInstances() {
		foreach (var (particleID, instance) in Particles) {
			var data = EntryData[(int)instance.Particle];

			if (data.GetPattern(Pattern.LinearFall)) {
				instance.Velocity.y -= LinearFallSpeed * Time.deltaTime;
			}
			if (data.GetPattern(Pattern.HasGravity)) {
				instance.Velocity += Physics.gravity * Time.deltaTime;
			}
			/*
			add simulation patterns
			*/
			if (instance.Velocity != Vector3.zero) {
				var position = instance.Animator.transform.localPosition;
				position += instance.Velocity * Time.deltaTime;
				instance.Animator.transform.localPosition = position;
			}
			if (instance.EndTime <= Time.time) IDBuffer.Add(particleID);
		}
		if (0 < IDBuffer.Count) {
			foreach (var particleID in IDBuffer) RemoveInstance(particleID);
			IDBuffer.Clear();
		}
	}

	static void RemoveInstance(uint particleID) {
		var instance = Particles[particleID];
		if (instance != null) {
			var transform_localPosition = ParticleTemplate.transform.localPosition;
			if (instance.Animator.transform.localPosition != transform_localPosition) {
				instance.Animator.transform.localPosition = transform_localPosition;
			}
			if (instance.Renderer) {
				instance.Renderer.sprite = null;
				instance.Renderer.flipX = false;
				instance.Renderer.flipY = false;
			}
			if (instance.Animator) {
				instance.Animator.runtimeAnimatorController = null;
			}
			instance.Animator.gameObject.SetActive(false);
			ParticlePool.Push(instance);
		}
		Particles.Remove(particleID);
	}



	// Particle Methods

	public static uint AddParticle(Particle particle, Vector3 position) {
		var (particleID, instance) = GetOrCreateInstance(particle);
		instance.Animator.transform.localPosition = position;
		return particleID;
	}

	public static void SetParticleVelocity(uint particleID, Vector3 velocity) {
		if (Particles.TryGetValue(particleID, out var instance)) {
			instance.Velocity = velocity;
		}
	}



	// Lifecycle

	void Update() {
		if (Simulate) TimeOfDay += Time.deltaTime / DayLength;
		UpdateInstances();

		// Test code
		if (InputManager.GetKeyDown(KeyAction.Jump)) {
			var particle = (Particle)Random.Range(0, 2);
			var position = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			var particleID = AddParticle(particle, position);
			SetParticleVelocity(particleID, position);
		}
	}
}
