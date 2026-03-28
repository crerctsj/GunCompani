using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
 
public class NetworkButtons : MonoBehaviour
{
    [Header("네트워크 버튼")    ]
    public Button hostBtn;
    public Button clientBtn;

    void Start()
    {
        // 버튼을 눌렀을 때 실행될 함수 연결
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            HideButtons();
        });

        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            HideButtons();
        });
    }

    void HideButtons()
    {
        // 접속하면 버튼들을 화면에서 치웁니다.
        gameObject.SetActive(false);
    }
}