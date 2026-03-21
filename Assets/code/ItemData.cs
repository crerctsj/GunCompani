using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;

    [Header("가치 범위 (0.1 단위 가능)")]
    public float minValue; // 최소 가격 (예: 10.0)
    public float maxValue; // 최대 가격 (예: 15.5)

    [Header("무게 범위 (0.1 단위 가능)")]
    public float minWeight; // 최소 무게 (예: 1.0)
    public float maxWeight; // 최대 무게 (예: 5.5)

    [Header("프리팹")]
    public GameObject itemPrefab;

    [Header("사용 여부")]
    public bool canUse;
    [Header("양손 여부")]
    public bool TwoHand;
}