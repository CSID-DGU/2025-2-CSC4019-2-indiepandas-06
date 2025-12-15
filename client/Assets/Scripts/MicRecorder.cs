using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading.Tasks;
using TMPro;



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Microphone Recorder
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI/Mic Recorder")]
[RequireComponent(typeof(Image), typeof(Button))]
public class MicRecorder : MonoBehaviour {

	// Constants

	const float AudioClipLength = 9f;



	// Fields

	Button m_Button;

	[Header("Components")]
	[SerializeField] MicSelector m_MicSelector;
	string m_Mic;
	AudioClip m_Clip;

	[Header("Events")]
	[SerializeField] UnityEvent m_OnStartRecord = new();
	[SerializeField] UnityEvent<AudioClip> m_OnRecorded = new();



	// Properties

	public Button Button => !m_Button ?
		m_Button = GetComponent<Button>() :
		m_Button;

	MicSelector MicSelector => m_MicSelector;

	string Mic {
		get => m_Mic;
		set => m_Mic = value;
	}
	public AudioClip Clip {
		get => m_Clip;
		set => m_Clip = value;
	}

	public UnityEvent OnStartRecord {
		get => m_OnStartRecord;
	}
	public UnityEvent<AudioClip> OnRecorded {
		get => m_OnRecorded;
	}
	public bool IsRecording {
		get => Microphone.IsRecording(Mic);
	}



	// Methods

	public void StartRecord() {
		Button.onClick.RemoveListener(StartRecord);
		Button.onClick.AddListener(StopRecord);
		Mic = MicSelector.GetSelected();
		Clip = Microphone.Start(Mic, false, (int)AudioClipLength, 44100);
		if (Clip) {
			OnStartRecord.Invoke();
			_ = StopRecordAsync();
		}
	}

	async Task StopRecordAsync() {
		float startpoint = Time.realtimeSinceStartup;
		while (IsRecording) {
			float elapsed = Time.realtimeSinceStartup - startpoint;
			if ((int)AudioClipLength < elapsed + 0.1f) {
				StopRecord();
				break;
			}
			await Task.Yield();
		}
	}



	public void StopRecord() {
		Button.onClick.RemoveListener(StopRecord);
		Button.onClick.AddListener(StartRecord);
		Microphone.End(Mic);
		OnRecorded.Invoke(Clip);
	}



	// Lifecycle

	void Start() {
		Button.onClick.AddListener(StartRecord);
	}
}
