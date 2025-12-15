using UnityEngine;
using UnityEngine.UIElements;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using static ElementEditorExtensions;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Audio Manager | Play Music
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Serializable, NodeMenu("Audio Manager/Play Music")]
public sealed class PlayMusicEvent : EventBase, ISerializer {

	// Node

	#if UNITY_EDITOR
	public sealed class PlayMusicEventNode : EventNodeBase {
		PlayMusicEvent I => target as PlayMusicEvent;

		public PlayMusicEventNode() : base() {
			mainContainer.style.width = Node2U;
		}

		public override void ConstructData() {
			var audio  = TextEnumField("Audio", I.Audio, value => I.Audio = value);
			var volume = FloatField("Volume", I.Volume, value => I.Volume = value);
			mainContainer.Add(audio);
			mainContainer.Add(volume);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			CreatePort(Direction.Output, PortType.DataID);
		}
	}
	#endif



	// Fields

	#if UNITY_EDITOR
	[SerializeField] string m_AudioName;
	#endif
	[SerializeField] Audio m_Audio;
	[SerializeField] float m_Volume = 1f;

	uint m_AudioID;



	// Properties

	public Audio Audio {
		get => m_Audio;
		set => m_Audio = value;
	}
	public float Volume {
		get => m_Volume;
		set => m_Volume = value;
	}

	ref uint AudioID {
		get => ref m_AudioID;
	}



	// Methods

	#if UNITY_EDITOR
	public void OnBeforeSerialize() {
		m_AudioName = m_Audio.ToString();
	}

	public void OnAfterDeserialize() {
		if (Enum.TryParse(m_AudioName, out Audio audio)) m_Audio = audio;
	}
	#endif



	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is PlayMusicEvent playMusicEvent) {
			Audio  = playMusicEvent.Audio;
			Volume = playMusicEvent.Volume;
		}
	}

	public override void Start() {
		AudioID = default;
	}

	public override void End() {
		if (AudioID == default) {
			AudioID = AudioManager.PlayMusic(Audio, Volume);
		}
	}

	protected override void GetDataID(ref uint audioID) {
		End();
		if (audioID != default) audioID = AudioID;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Audio Manager | Play Sound FX
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Serializable, NodeMenu("Audio Manager/Play Sound FX")]
public sealed class PlaySoundFXEvent : EventBase, ISerializer {

	// Node

	#if UNITY_EDITOR
	public sealed class PlaySoundFXEventNode : EventNodeBase {
		PlaySoundFXEvent I => target as PlaySoundFXEvent;

		public PlaySoundFXEventNode() : base() {
			mainContainer.style.width = Node2U;
		}

		public override void ConstructData() {
			var audio  = TextEnumField("Audio", I.Audio, value => I.Audio = value);
			var volume = FloatField("Volume", I.Volume, value => I.Volume = value);
			mainContainer.Add(audio);
			mainContainer.Add(volume);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			CreatePort(Direction.Output, PortType.DataID);
		}
	}
	#endif



	// Fields

	#if UNITY_EDITOR
	[SerializeField] string m_AudioName;
	#endif
	[SerializeField] Audio m_Audio;
	[SerializeField] float m_Volume = 1f;

	uint m_AudioID;



	// Properties

	public Audio Audio {
		get => m_Audio;
		set => m_Audio = value;
	}
	public float Volume {
		get => m_Volume;
		set => m_Volume = value;
	}

	ref uint AudioID {
		get => ref m_AudioID;
	}



	// Methods

	#if UNITY_EDITOR
	public void OnBeforeSerialize() {
		m_AudioName = m_Audio.ToString();
	}

	public void OnAfterDeserialize() {
		if (Enum.TryParse(m_AudioName, out Audio audio)) m_Audio = audio;
	}
	#endif



	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is PlaySoundFXEvent playSoundFXEvent) {
			Audio  = playSoundFXEvent.Audio;
			Volume = playSoundFXEvent.Volume;
		}
	}

	public override void Start() {
		AudioID = default;
	}

	public override void End() {
		if (AudioID == default) {
			AudioID = AudioManager.PlaySoundFX(Audio, Volume);
		}
	}

	protected override void GetDataID(ref uint audioID) {
		End();
		if (audioID != default) audioID = AudioID;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Audio Manager | Play Point Sound FX
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Serializable, NodeMenu("Audio Manager/Play Point Sound FX")]
public sealed class PlayPointSoundFXEvent : EventBase, ISerializer {

	// Node

	#if UNITY_EDITOR
	public sealed class PlayPointSoundFXEventNode : EventNodeBase {
		PlayPointSoundFXEvent I => target as PlayPointSoundFXEvent;

		public PlayPointSoundFXEventNode() : base() {
			mainContainer.style.width = Node2U;
		}

		public override void ConstructData() {
			var audio    = TextEnumField("Audio", I.Audio, value => I.Audio = value);
			var anchor   = ObjectField("Anchor", I.Anchor, value => I.Anchor = value);
			var position = Vector3Field("Position", I._Position, value => I._Position = value);
			var volume   = FloatField("Volume", I.Volume, value => I.Volume = value);
			var spread   = FloatField("Spread", I.Spread, value => I.Spread = value);
			mainContainer.Add(audio);
			mainContainer.Add(anchor);
			mainContainer.Add(position);
			mainContainer.Add(volume);
			mainContainer.Add(spread);

			var element = new VisualElement();
			element.style.flexDirection = FlexDirection.Row;
			mainContainer.Add(element);
			element.Add(Label("Distance"));
			var child0 = Label("Min");
			child0.style.width = 26f;
			element.Add(child0);
			var child1 = new FloatField() { value = I.MinDistance };
			child1.ElementAt(0).style.minWidth = (Node2U - Node2ULabel - 20f) * 0.5f - 26f;
			child1.ElementAt(0).style.maxWidth = (Node2U - Node2ULabel - 20f) * 0.5f - 26f;
			child1.RegisterValueChangedCallback(callback => I.MinDistance = callback.newValue);
			element.Add(child1);
			var child2 = Label("Max");
			child2.style.width = 26f;
			element.Add(child2);
			var child3 = new FloatField() { value = I.MaxDistance };
			child3.ElementAt(0).style.minWidth = (Node2U - Node2ULabel - 20f) * 0.5f - 26f;
			child3.ElementAt(0).style.maxWidth = (Node2U - Node2ULabel - 20f) * 0.5f - 26f;
			child3.RegisterValueChangedCallback(callback => I.MaxDistance = callback.newValue);
			element.Add(child3);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			CreatePort(Direction.Output, PortType.DataID);
		}
	}
	#endif



	// Fields

	#if UNITY_EDITOR
	[SerializeField] string m_AudioName;
	#endif
	[SerializeField] Audio m_Audio;
	[SerializeField] GameObject m_Anchor;
	[SerializeField] Vector3 m_Position;
	[SerializeField] float m_Volume = 1f;
	[SerializeField] float m_Spread = 0f;
	[SerializeField] float m_MinDistance = -1f;
	[SerializeField] float m_MaxDistance = -1f;

	uint m_AudioID;



	// Properties

	public Audio Audio {
		get => m_Audio;
		set => m_Audio = value;
	}
	public GameObject Anchor {
		get => m_Anchor;
		set => m_Anchor = value;
	}
	public Vector3 _Position {
		get => m_Position;
		set => m_Position = value;
	}
	public float Volume {
		get => m_Volume;
		set => m_Volume = value;
	}
	public float Spread {
		get => m_Spread;
		set => m_Spread = value;
	}
	public float MinDistance {
		get => m_MinDistance;
		set => m_MinDistance = value;
	}
	public float MaxDistance {
		get => m_MaxDistance;
		set => m_MaxDistance = value;
	}

	ref uint AudioID {
		get => ref m_AudioID;
	}



	// Methods

	#if UNITY_EDITOR
	public void OnBeforeSerialize() {
		m_AudioName = m_Audio.ToString();
	}

	public void OnAfterDeserialize() {
		if (Enum.TryParse(m_AudioName, out Audio audio)) m_Audio = audio;
	}
	#endif



	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is PlayPointSoundFXEvent playPointSoundFXEvent) {
			Audio       = playPointSoundFXEvent.Audio;
			Volume      = playPointSoundFXEvent.Volume;
			Anchor      = playPointSoundFXEvent.Anchor;
			_Position    = playPointSoundFXEvent._Position;
			Spread      = playPointSoundFXEvent.Spread;
			MinDistance = playPointSoundFXEvent.MinDistance;
			MaxDistance = playPointSoundFXEvent.MaxDistance;
		}
	}

	public override void Start() {
		AudioID = default;
	}

	public override void End() {
		if (AudioID == default) {
			if (Anchor) {
				var position = Anchor.transform.TransformPoint(_Position);
				AudioID = AudioManager.PlayPointSoundFX(
					Audio, position, Volume, Spread, MinDistance, MaxDistance);
			} else {
				AudioID = AudioManager.PlayPointSoundFX(
					Audio, _Position, Volume, Spread, MinDistance, MaxDistance);
			}
		}
	}

	protected override void GetDataID(ref uint audioID) {
		End();
		if (audioID != default) audioID = AudioID;
	}



	#if UNITY_EDITOR
	public override void DrawGizmos() {
		if (Anchor) {
			var position = Anchor.transform.TransformPoint(_Position);
			Gizmos.DrawIcon(position, "AudioSource Gizmo", true, Gizmos.color);
		} else {
			Gizmos.DrawIcon(_Position, "AudioSource Gizmo", true, Gizmos.color);
		}
	}
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Audio Manager | Play Blend Sound FX
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Serializable, NodeMenu("Audio Manager/Play Blend Sound FX")]
public sealed class PlayBlendSoundFXEvent : EventBase, ISerializer {

	// Node

	#if UNITY_EDITOR
	public sealed class PlayBlendSoundFXNode : EventNodeBase {
		PlayBlendSoundFXEvent I => target as PlayBlendSoundFXEvent;

		public PlayBlendSoundFXNode() : base() {
			mainContainer.style.width = Node2U;
		}

		public override void ConstructData() {
			var audio    = TextEnumField("Audio", I.Audio, value => I.Audio = value);
			var anchor   = ObjectField("Anchor", I.Anchor, value => I.Anchor = value);
			var position = Vector3Field("Position", I._Position, value => I._Position = value);
			var volume   = FloatField("Volume", I.Volume, value => I.Volume = value);
			var blend    = Slider("Blend", I.Blend, 0f, 1f, value => I.Blend = value);
			mainContainer.Add(audio);
			mainContainer.Add(anchor);
			mainContainer.Add(position);
			mainContainer.Add(volume);
			mainContainer.Add(blend);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			CreatePort(Direction.Output, PortType.DataID);
		}
	}
	#endif



	// Fields

	#if UNITY_EDITOR
	[SerializeField] string m_AudioName;
	#endif
	[SerializeField] Audio m_Audio;
	[SerializeField] GameObject m_Anchor;
	[SerializeField] Vector3 m_Position;
	[SerializeField] float m_Volume = 1f;
	[SerializeField] float m_Blend = 0.5f;

	uint m_AudioID;



	// Properties

	public Audio Audio {
		get => m_Audio;
		set => m_Audio = value;
	}
	public GameObject Anchor {
		get => m_Anchor;
		set => m_Anchor = value;
	}
	public Vector3 _Position {
		get => m_Position;
		set => m_Position = value;
	}
	public float Volume {
		get => m_Volume;
		set => m_Volume = value;
	}
	public float Blend {
		get => m_Blend;
		set => m_Blend = value;
	}

	ref uint AudioID {
		get => ref m_AudioID;
	}



	// Methods

	#if UNITY_EDITOR
	public void OnBeforeSerialize() {
		m_AudioName = m_Audio.ToString();
	}

	public void OnAfterDeserialize() {
		if (Enum.TryParse(m_AudioName, out Audio audio)) m_Audio = audio;
	}
	#endif



	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is PlayBlendSoundFXEvent playBlendSoundFXEvent) {
			Audio    = playBlendSoundFXEvent.Audio;
			Volume   = playBlendSoundFXEvent.Volume;
			Anchor   = playBlendSoundFXEvent.Anchor;
			_Position = playBlendSoundFXEvent._Position;
			Blend    = playBlendSoundFXEvent.Blend;
		}
	}

	public override void Start() {
		AudioID = default;
	}

	public override void End() {
		if (AudioID == default) {
			if (Anchor) {
				var position = Anchor.transform.TransformPoint(_Position);
				AudioID = AudioManager.PlayBlendSoundFX(Audio, position, Volume, Blend);
			} else {
				AudioID = AudioManager.PlayBlendSoundFX(Audio, _Position, Volume, Blend);
			}
		}
	}

	protected override void GetDataID(ref uint audioID) {
		End();
		if (audioID != default) audioID = AudioID;
	}



	#if UNITY_EDITOR
	public override void DrawGizmos() {
		if (Anchor) {
			var position = Anchor.transform.TransformPoint(_Position);
			Gizmos.DrawIcon(position, "AudioSource Gizmo", true, Gizmos.color);
		} else {
			Gizmos.DrawIcon(_Position, "AudioSource Gizmo", true, Gizmos.color);
		}
	}
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Audio Manager | Set Audio Position
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Serializable, NodeMenu("Audio Manager/Set Audio Position")]
public sealed class SetAudioPositionEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public sealed class SetAudioPositionEventNode : EventNodeBase {
		SetAudioPositionEvent I => target as SetAudioPositionEvent;

		public SetAudioPositionEventNode() : base() {
			mainContainer.style.width = Node2U;
		}

		public override void ConstructData() {
			var anchor   = ObjectField("Anchor", I.Anchor, value => I.Anchor = value);
			var position = Vector3Field("Position", I._Position, value => I._Position = value);
			mainContainer.Add(anchor);
			mainContainer.Add(position);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Input, PortType.DataID);
			CreatePort(Direction.Output);
			CreatePort(Direction.Output, PortType.DataID);
		}
	}
	#endif



	// Fields

	[SerializeField] GameObject m_Anchor;
	[SerializeField] Vector3 m_Position;

	uint m_AudioID;



	// Properties

	public GameObject Anchor {
		get => m_Anchor;
		set => m_Anchor = value;
	}
	public Vector3 _Position {
		get => m_Position;
		set => m_Position = value;
	}

	ref uint AudioID {
		get => ref m_AudioID;
	}



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetAudioPositionEvent setAudioPositionEvent) {
			Anchor   = setAudioPositionEvent.Anchor;
			_Position = setAudioPositionEvent._Position;
		}
	}

	public override void Start() {
		AudioID = default;
	}

	public override void End() {
		if (AudioID == default) base.GetDataID(ref AudioID);
		if (AudioID != default) {
			if (Anchor) {
				var position = Anchor.transform.TransformPoint(_Position);
				AudioManager.SetAudioPosition(AudioID, position);
			} else {
				AudioManager.SetAudioPosition(AudioID, _Position);
			}
		}
	}

	protected override void GetDataID(ref uint audioID) {
		if (audioID == default) base.GetDataID(ref audioID);
		if (audioID != default) audioID = AudioID;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Audio Manager | Set Audio Volume
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Serializable, NodeMenu("Audio Manager/Set Audio Volume")]
public sealed class SetAudioVolumeEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	class SetAudioVolumeEventNode : EventNodeBase {
		SetAudioVolumeEvent I => target as SetAudioVolumeEvent;

		public SetAudioVolumeEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var volume = Slider(I.Volume, 0f, 1f, value => I.Volume = value);
			mainContainer.Add(volume);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Input, PortType.DataID);
			CreatePort(Direction.Output);
			CreatePort(Direction.Output, PortType.DataID);
		}
	}
	#endif



	// Fields

	[SerializeField] float m_Volume = 1f;

	uint m_AudioID;



	// Properties

	public float Volume {
		get => m_Volume;
		set => m_Volume = value;
	}

	ref uint AudioID {
		get => ref m_AudioID;
	}



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetAudioVolumeEvent setAudioVolumeEvent) {
			Volume = setAudioVolumeEvent.Volume;
		}
	}

	public override void Start() {
		AudioID = default;
	}

	public override void End() {
		if (AudioID == default) base.GetDataID(ref AudioID);
		if (AudioID != default) AudioManager.SetAudioVolume(AudioID, Volume);
	}

	protected override void GetDataID(ref uint audioID) {
		if (audioID == default) base.GetDataID(ref audioID);
		if (audioID != default) audioID = AudioID;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Audio Manager | Stop Audio
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Serializable, NodeMenu("Audio Manager/Stop Audio")]
public sealed class StopAudioEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public sealed class StopAudioEventNode : EventNodeBase {
		StopAudioEvent I => target as StopAudioEvent;

		public StopAudioEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Input, PortType.DataID);
			CreatePort(Direction.Output);
		}
	}
	#endif



	// Fields

	uint m_AudioID;



	// Properties

	ref uint AudioID {
		get => ref m_AudioID;
	}



	// Methods

	public override void Start() {
		AudioID = default;
	}

	public override void End() {
		if (AudioID == default) base.GetDataID(ref AudioID);
		if (AudioID != default) AudioManager.StopAudio(AudioID);
	}
}
