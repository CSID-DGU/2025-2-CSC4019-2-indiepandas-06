using UnityEngine;

[CreateAssetMenu(fileName = "PlantData", menuName = "Farming/PlantData")]
public class PlantData : ScriptableObject
{
    [SerializeField] private string plantId;
    public string PlantId => plantId;
    [SerializeField] private string plantName;
    public string PlantName => plantName;
    [SerializeField] private float growthTime;
    public float GrowthTime => growthTime;
    [SerializeField] private Sprite[] growthSprites;
    public Sprite[] GrowthSprites => (Sprite[])growthSprites.Clone();
    [SerializeField] private ItemData itemData;
    public ItemData ItemData => itemData;

}
