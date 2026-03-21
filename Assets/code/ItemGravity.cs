using UnityEngine;

public class ItemGravity : MonoBehaviour
{
    [Header("설정")]
    public float fallSpeed = 5f;
    public LayerMask groundLayer;

    public bool isHeld = false;

    private Collider col;

    void Start()
    {
        col = GetComponent<Collider>();
    }

    void Update()
{
    if (isHeld) return;

    float bottomOffset = col.bounds.extents.y; 

    // [수정] 시작점을 아이템 중심(transform.position)에서 쏘되, 
    // 길이를 '절반 높이 + 여유분'만큼 줍니다.
    float rayLength = bottomOffset + 0.1f;
    
    // 시각적으로 Ray를 확인하기 위해 Scene 뷰에 선을 그립니다 (디버그용)
    Debug.DrawRay(transform.position, Vector3.down * rayLength, Color.red);

    // [수정] Raycast를 쏠 때 groundLayer를 확실히 체크
    if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayLength, groundLayer))
    {
        // 바닥에 닿음: 위치 보정
        transform.position = new Vector3(transform.position.x, hit.point.y + bottomOffset, transform.position.z);
    }
    else
    {
        // 공중에 떠 있음: 하강
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }
}
}