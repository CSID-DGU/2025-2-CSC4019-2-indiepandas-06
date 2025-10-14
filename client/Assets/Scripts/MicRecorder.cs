using UnityEngine;
using UnityEngine.UI;
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
	const float ServerTimeout = 5f;



	// Fields

	[Header("Resources")]
	[SerializeField] Sprite m_SpriteStart;
	[SerializeField] Sprite m_SpriteStop;
	Image m_Image;
	Button m_Button;

	[Header("Components")]
	[SerializeField] MicSelector m_MicSelector;
	[SerializeField] TextMeshProUGUI m_Text;
	string m_Mic;
	AudioClip m_Clip;



	// Properties

	Sprite SpriteStart => m_SpriteStart;
	Sprite SpriteStop => m_SpriteStop;

	public Image Image => m_Image || TryGetComponent(out m_Image) ? m_Image : null;
	public Button Button => m_Button || TryGetComponent(out m_Button) ? m_Button : null;
	public Sprite Sprite {
		get => Image.sprite;
		set => Image.sprite = value;
	}

	MicSelector MicSelector => m_MicSelector;

	string Text {
		get => m_Text.text;
		set => m_Text.text = value;
	}
	string Mic {
		get => m_Mic;
		set => m_Mic = value;
	}
	AudioClip Clip {
		get => m_Clip;
		set => m_Clip = value;
	}



	// Methods

	public void StartRecord() {
		if (Sprite != SpriteStart) return;
		Sprite = SpriteStop;
		Button.onClick.RemoveListener(StartRecord);
		Button.onClick.AddListener(StopRecord);
		Mic = MicSelector.GetSelected();
		Clip = Microphone.Start(Mic, false, (int)AudioClipLength, 44100);
		if (!Clip) Text = $"Recording failed : {Mic} Unavailable";
		else _ = StopRecordAsync();
	}

	async Task StopRecordAsync() {
		float startpoint = Time.realtimeSinceStartup;
		while (Microphone.IsRecording(Mic)) {
			float elapsed = Time.realtimeSinceStartup - startpoint;
			Text = (elapsed % 3f) switch {
				< 1.0f => $"(Recording.)  ",
				< 2.0f => $"(Recording..) ",
				< 3.0f => $"(Recording...)",
				_ => $"(Recording)",
			};
			if ((int)AudioClipLength < elapsed + 0.1f) {
				StopRecord();
				break;
			}
			await Task.Yield();
		}
	}



	public void StopRecord() {
		if (Sprite != SpriteStop) return;
		Sprite = SpriteStart;
		Button.onClick.RemoveListener(StopRecord);
		Button.onClick.AddListener(StartRecord);
		Microphone.End(Mic);
		_ = ConvertDataAsync();
	}

	async Task ConvertDataAsync() {
		Button.enabled = false;

		/* Transmit clip to server */

		float startpoint = Time.realtimeSinceStartup;
		while (true) {
			float elapsed = Time.realtimeSinceStartup - startpoint;
			Text = (elapsed % 3f) switch {
				< 1.0f => $"(Converting.)",
				< 2.0f => $"(Converting..)",
				< 3.0f => $"(Converting...)",
				_ => $"(Converting)",
			};
			await Task.Yield();

			/* Receive text from server if arrived */

			if (ServerTimeout < elapsed) {
				Text = $"Conversion Failed : Server timeout";
				break;
			}
		}
		Sprite = SpriteStart;
		Button.enabled = true;
	}



	// Lifecycle

	void Start() {
		Sprite = SpriteStart;
		Button.onClick.AddListener(StartRecord);
		Text = $"";
	}
}
