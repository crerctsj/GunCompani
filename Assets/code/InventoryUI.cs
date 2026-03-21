using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("연결된 시스템")]
    public Inventory inventory; 

    [Header("슬롯 UI 요소")]
    public Image[] slotImages;           // 슬롯 배경 이미지 4개
    public TextMeshProUGUI[] nameTexts;  // 아이템 이름 텍스트 4개
    public TextMeshProUGUI weightText;   // 무게 표시 텍스트

    [Header("설정")]
    public Color activeColor = Color.white;    // 선택된 슬롯 색상
    public Color inactiveColor = Color.gray;   // 선택되지 않은 슬롯 색상
    public float activeScale = 1.2f;           // 선택된 슬롯 크기

    void Update()
    {
        if (inventory == null) return;

        UpdateSlots();
        UpdateWeight();
    }

    void UpdateSlots()
    {
        // 인벤토리 클래스에서 현재 슬롯 인덱스와 아이템 리스트를 가져옵니다.
        int currentIndex = inventory.GetCurrentSlotIndex();
        List<GameObject> items = inventory.GetSlots();

        for (int i = 0; i < slotImages.Length; i++)
        {
            // 1. 슬롯 선택 하이라이트 효과
            if (i == currentIndex)
            {
                slotImages[i].color = activeColor;
                slotImages[i].transform.localScale = Vector3.one * activeScale;
            }
            else
            {
                slotImages[i].color = inactiveColor;
                slotImages[i].transform.localScale = Vector3.one;
            }

            // 2. 아이템 이름 표시 (리스트 기반이므로 개수 체크 필수)
            if (i < items.Count && items[i] != null)
            {
                var itemData = items[i].GetComponent<InteractableItem>().itemData;
                nameTexts[i].text = itemData.itemName;
            }
            else
            {
                nameTexts[i].text = ""; // 빈 슬롯
            }
        }
    }

    void UpdateWeight()
    {
        // 전체 무게 표시 (0.1 단위)
        weightText.text = $"{inventory.totalWeight:F1} kg";
    }
}