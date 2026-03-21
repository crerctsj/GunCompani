using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerInteraction : NetworkBehaviour
{
    public float interactionDistance = 3f; 
    public LayerMask interactableLayer;    
    public GameObject crosshair;
    public GameObject grab;

    private Inventory inventory; 

    // [추가] 멀티플레이에서 카메라와 리스너 충돌 방지
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // 내 캐릭터가 아니라면 카메라와 리스너를 끈다
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
        }
    }

    void Start()
    {
        inventory = GetComponentInParent<Inventory>();

        // [수정] Find 대신 더 안전한 방식으로 할당 확인
        // 만약 인스펙터에서 직접 넣었다면 이 과정은 건너뜁니다.
        if (crosshair == null) crosshair = GameObject.Find("Crosshair"); 
        if (grab == null) grab = GameObject.Find("Grab UI"); // 이름이 정확해야 함!
    }

    void Update()
    {
        // 1. 내가 주인일 때만 실행
        if (!IsOwner) return;

        // 2. UI가 없으면 실행 안 함 (Null 에러 방지)
        if (crosshair == null || grab == null) 
        {
            // 아직 UI를 못 찾았다면 다시 찾기 시도
            crosshair = GameObject.Find("Crosshair");
            grab = GameObject.Find("Grab");
            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            if (inventory != null && inventory.IsHoldingTwoHanded())
            {
                UpdateUI(true, false); // 일반 조준점
            }
            else
            {
                UpdateUI(false, true); // 집기 아이콘

                if (Input.GetKeyDown(KeyCode.E))
                {
                    DoInteract(hit.collider.gameObject);
                }
            }
        }
        else
        {
            UpdateUI(true, false);
        }
    }

    // UI 상태 변경을 한 곳에서 관리 (가독성 상향)
    void UpdateUI(bool showCrosshair, bool showGrab)
    {
        if(crosshair.activeSelf != showCrosshair) crosshair.SetActive(showCrosshair);
        if(grab.activeSelf != showGrab) grab.SetActive(showGrab);
    }

    void DoInteract(GameObject obj)
    {
        if (inventory != null && obj.GetComponent<InteractableItem>() != null)
        {
            if (inventory.IsHoldingTwoHanded()) return;

            // [주의] 멀티플레이에서는 아이템을 줍는 행동을 서버에 알려야 합니다.
            // 지금은 로컬에서만 추가되지만, 나중에 ServerRpc를 써야 남들도 아이템이 사라진 걸 봅니다.
            inventory.AddItem(obj); 
        }
    }
}