using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMapObject", menuName = "MapObject/MapObjectData")]
public class MapObjectData : ScriptableObject {
    [Header("Info")]
    public string ObjectId;
    public string ObjectName;
    public List<Sprite> Sprites; 
    public ItemData LinkedItem;

    [Header("Collider")]
    // true면 스프라이트의 콜라이더 사용
    public bool UseSpriteCollider = true;

    [Header("Extend Components")]
    public string[] AdditionalComponents;

    [Header("Option")]
    public bool IsRotatable = false;
    public bool UseFlipX = false;
    public bool IsEditable = true;

    [Header("Grid Footprint")]
    public int Size = 1;
    public bool[,] footprint;

    private void OnValidate() {
        // 인스펙터에서 새로 만들었을 때 기본값 설정
        if (string.IsNullOrEmpty(ObjectId)) {
            // ScriptableObject 이름을 기준으로 변환
            // ex) "NyangPunchData" -> "NyangPunchSkill"
            ObjectId = name.Replace("Data", "");
        }
    }
}