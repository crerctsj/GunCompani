using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("설정")]
    public Transform holdParent;    // 카메라 자식으로 만든 '손' 위치
    public int maxSlots = 4;        // 최대 슬롯 수
    
    [Header("상태")]
    private List<GameObject> slots = new List<GameObject>();
    private int currentSlotIndex = 0;
    public float totalWeight = 0;

    void Update()
    {
        // [수정] 현재 양손 아이템을 들고 있다면 슬롯 전환 입력을 무시함
        if (!IsHoldingTwoHanded())
        {
            // 1~4 숫자키로 슬롯 전환
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchSlot(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchSlot(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchSlot(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchSlot(3);
        }

        // G 키로 현재 아이템 버리기 (양손 아이템은 버려야만 다른 슬롯으로 갈 수 있음)
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropItem();
        }
    }

    // [완성] 현재 양손 아이템을 들고 있는지 확인하는 함수
    public bool IsHoldingTwoHanded()    
    {
        if (slots.Count == 0 || currentSlotIndex >= slots.Count) return false;

        GameObject currentItem = slots[currentSlotIndex];
        // InteractableItem에서 결정된 TwoHand 값을 가져옴
        return currentItem.GetComponent<InteractableItem>().TwoHand;
    }

    public void AddItem(GameObject item)
    {
        if (slots.Count >= maxSlots)
        {
            Debug.Log("인벤토리가 가득 찼습니다!");
            return;
        }

        float itemWeight = item.GetComponent<InteractableItem>().finalWeight;
        totalWeight += itemWeight;    

        // 1. Rigidbody 및 Collider 비활성화
        if (item.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (item.TryGetComponent<Collider>(out Collider col))
        {
            col.enabled = false;
        }

        // 2. 리스트에 추가
        slots.Add(item);

        // 3. 부모 설정 및 위치 초기화
        item.transform.SetParent(holdParent);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        // 4. 새로 주운 아이템을 바로 들도록 설정
        SwitchSlot(slots.Count - 1);
    }

    public void DropItem()
    {
        if (slots.Count == 0 || currentSlotIndex >= slots.Count) 
        {
            Debug.Log("버릴 아이템이 없습니다.");
            return;
        }

        GameObject itemToDrop = slots[currentSlotIndex];
        float itemWeight = itemToDrop.GetComponent<InteractableItem>().finalWeight;
        totalWeight -= itemWeight;

        // 2. 부모 관계 해제
        itemToDrop.transform.SetParent(null);

        // 3. 위치 및 회전 설정
        itemToDrop.transform.position = holdParent.position + holdParent.forward * 0.7f - holdParent.up * 0.2f;
        itemToDrop.transform.rotation = Quaternion.identity;

        // 4. 물리 및 충돌 활성화
        if (itemToDrop.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(holdParent.forward * 3f, ForceMode.Impulse);
        }

        if (itemToDrop.TryGetComponent<Collider>(out Collider col))
        {
            col.enabled = true;
        }

        // 5. 리스트에서 제거
        slots.RemoveAt(currentSlotIndex);
        
        if (slots.Count > 0)
        {
            currentSlotIndex = Mathf.Clamp(currentSlotIndex, 0, slots.Count - 1);
            SwitchSlot(currentSlotIndex);
        }
        else
        {
            currentSlotIndex = 0;
        }
    }

    void SwitchSlot(int index)
    {
        // 리스트 기반이므로 범위를 벗어난 인덱스 처리
        if (index < 0 || index >= slots.Count) return;

        currentSlotIndex = index;

        // 선택한 아이템만 활성화
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetActive(i == currentSlotIndex);
        }
    }
    public List<GameObject> GetSlots()
    {
        return slots;
    }
    public int GetCurrentSlotIndex() 
    {
        return currentSlotIndex;
    }
}