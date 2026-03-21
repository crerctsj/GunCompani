using Unity.Netcode;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour 
{
    [Header("내 캐릭터가 아닐 때 꺼야 할 것들")]
    public Camera remoteCamera;         // 남의 카메라
    public AudioListener remoteListener; // 남의 귀

    public override void OnNetworkSpawn()
    {
        // 내가 주인이 아니라면 (남의 캐릭터라면)
        if (!IsOwner)
        {
            if (remoteCamera != null) remoteCamera.enabled = false;
            if (remoteListener != null) remoteListener.enabled = false; // 이걸 꼭 체크!
            
            // 추가 팁: 만약 PlayerInteraction 같은 스크립트가 있다면 그것도 꺼야 함
            if (GetComponent<PlayerInteraction>() != null) 
                GetComponent<PlayerInteraction>().enabled = false;
        }
    }
}