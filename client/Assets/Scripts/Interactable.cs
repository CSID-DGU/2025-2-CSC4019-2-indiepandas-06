using UnityEngine;



public enum InteractionType {
	Interact,
	Talk,
	Listen,
	Trade,
	BuildingEntry,
	BuildingExit,
	Fishing,
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Interactable
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public interface IInteractable {

	// Properties

	public InteractionType InteractionType { get; }

	public bool IsInteractable { get; }



	// Methods

	public void Interact(GameObject interactor);
}
