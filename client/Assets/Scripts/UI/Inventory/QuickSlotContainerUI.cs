using UnityEngine;

public class QuickSlotContainerUI : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotParent;

    private QuickSlotUI[] slotUIs;

    private void Start() {
        int count = QuickSlotManager.Instance.SlotCount;
        slotUIs = new QuickSlotUI[count];

        for (int i = 0; i < count; i++) {
            var slotObj = Instantiate(slotPrefab, slotParent);
            var slotUI = slotObj.GetComponent<QuickSlotUI>();
            slotUI.Init(i);
            slotUIs[i] = slotUI;
        }
    }

    private void Update() {
        foreach (var slotUI in slotUIs) {
            slotUI.UpdateUI();
        }
    }
}
