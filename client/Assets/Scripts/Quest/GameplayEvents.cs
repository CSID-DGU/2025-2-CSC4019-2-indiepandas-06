using System;

public static class GameplayEvents {
    public static event Action<string> OnItemChanged;
    public static event Action<string> OnNpcTalked;
    public static event Action<string> OnLocationReached;
    public static event Action<string> OnObjectBuilt;
    public static event Action<string> OnDialogueCompleted;

    public static void RaiseItemChanged(string itemId) => OnItemChanged?.Invoke(itemId);
    public static void RaiseNpcTalked(string npcId) => OnNpcTalked?.Invoke(npcId);
    public static void RaiseDialogueCompleted(string dialogueId) => OnDialogueCompleted?.Invoke(dialogueId);
    public static void RaiseLocationReached(string locId) => OnLocationReached?.Invoke(locId);
    public static void RaiseObjectBuilt(string objId) => OnObjectBuilt?.Invoke(objId);
}