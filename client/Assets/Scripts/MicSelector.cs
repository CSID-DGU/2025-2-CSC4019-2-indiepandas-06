using UnityEngine;
using System.Linq;
using TMPro;



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Microphone Selector
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI/Mic Selector")]
[RequireComponent(typeof(TMP_Dropdown))]
public class MicSelector : MonoBehaviour {

	// Fields

	TMP_Dropdown m_Dropdown;



	// Properties

	TMP_Dropdown Dropdown => m_Dropdown || TryGetComponent(out m_Dropdown) ? m_Dropdown : null;



	// Methods

	public string GetSelected() => Dropdown.options[Dropdown.value].text;



	// Lifecycle

	void Start() {
		Dropdown.ClearOptions();
		Dropdown.AddOptions(Microphone.devices.ToList());
		Dropdown.value = 0;
		Dropdown.RefreshShownValue();
	}
}