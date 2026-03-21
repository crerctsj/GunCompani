using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    public ItemData itemData;

    [Header("이 아이템의 실제 정보")]
    public float finalValue;   // 결정된 가치
    public float finalWeight;  // 결정된 무게
    public bool TwoHand;       // 양손 아이템 여부 (런타임 확인용)
    public bool canUse;

    void Start()
    {
        InitializeItem();
    }

    public void InitializeItem()
    {
        if (itemData == null)
        {
            Debug.LogWarning(gameObject.name + "에 ItemData가 연결되지 않았습니다!");
            return;
        }

        // 1. 가치 결정 (Random.Range 사용 후 0.1 단위 반올림)
        float rawValue = Random.Range(itemData.minValue, itemData.maxValue);
        finalValue = Mathf.Round(rawValue * 10f) / 10f;

        // 2. 무게 결정 (Random.Range 사용 후 0.1 단위 반올림)
        float rawWeight = Random.Range(itemData.minWeight, itemData.maxWeight);
        finalWeight = Mathf.Round(rawWeight * 10f) / 10f;

        // 3. 양손 여부 설정 (ItemData에 설정된 값을 그대로 가져옴)
        TwoHand = itemData.TwoHand;
        canUse = itemData.canUse;

        // 결과 확인
        Debug.Log($"{itemData.itemName} 생성! 가치: ${finalValue}, 무게: {finalWeight}kg, 양손: {TwoHand}, 사용: {canUse}");
    }
}