using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("이동 및 관성 설정")]
    public float baseMoveSpeed = 5f;
    public float sprintMultiplier = 1.6f;
    public float crouchSpeedMultiplier = 0.5f; 
    public float weightSpeedPenalty = 0.05f;
    public float minMoveSpeed = 1.5f;
    
    [Header("가속/감속 관성 (무게 비례)")]
    public float emptySmoothTime = 0.1f;      // 빈손일 때 반응 속도
    public float maxWeightSmoothTime = 1.5f;  // 무거울 때 미끄러지는 정도
    public float weightThreshold = 50f;       // 최대 관성이 적용되는 무게 기준
    
    [Header("점프 및 중력")]
    public float jumpHeight = 1.5f;
    public float gravity = -25f;
    public float weightJumpPenalty = 0.02f;
    [Range(0f, 0.1f)] public float airControl = 0.02f; // 공중 방향 전환 제어 (0에 가까울수록 불가능)
    public float jumpStaminaCost = 15f;
    private Vector3 verticalVelocity;
    private bool isGrounded;

    [Header("웅크리기 설정")]
    public float crouchHeight = 1.0f;
    public float standHeight = 2.0f;
    public float crouchSmoothTime = 10f;
    public bool isCrouching = false;
    private float targetHeight;

    [Header("마우스 감도 및 시야")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera; 
    private float xRotation = 0f;

    [Header("스태미나 설정")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaConsumeRate = 15f;
    public float staminaRegenRate = 10f;
    public float weightStaminaPenalty = 0.2f;

    [Header("상태 디버그")]
    public bool isSprinting = false;
    public bool isExhausted = false;
    [SerializeField] private Vector3 currentMoveVelocity; // 현재 속도 벡터

    private CharacterController controller;
    private Inventory inventory;
    private Vector3 currentInertiaVelocity;
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        inventory = GetComponent<Inventory>();
        
        currentStamina = maxStamina;
        targetHeight = standHeight;
        
        // 커서 고정
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if(!IsOwner) return;

        HandleMouseLook();
        HandleStamina();
        HandleCrouch();
        
        // 바닥 체크 및 중력 초기화
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f; 
        }

        // 이동 로직 (관성 및 공중 제어 포함)
        MoveWithWeightInertia(); 

        // 점프 로직 (웅크리기 중이거나 스태미나 부족 시 차단)
        if (Input.GetButtonDown("Jump"))
        {
            DoJump();
        }

        // 수직 속도(중력) 적용
        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = true;
            targetHeight = crouchHeight;
        }
        else
        {
            isCrouching = false;
            targetHeight = standHeight;
        }

        // 높이 및 카메라 위치 보간
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchSmoothTime);
        Vector3 camPos = playerCamera.localPosition;
        camPos.y = controller.height * 0.45f; 
        playerCamera.localPosition = camPos;
    }

    void MoveWithWeightInertia()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // 무게 페널티 적용된 기본 속도
        float speedLoss = inventory.totalWeight * weightSpeedPenalty;
        float currentMaxSpeed = Mathf.Max(baseMoveSpeed - speedLoss, minMoveSpeed);

        bool isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);
        
        // 웅크리기 중에는 달리기 불가
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving && currentStamina > 0 && !isExhausted && !isCrouching;

        if (isSprinting) currentMaxSpeed *= sprintMultiplier;
        else if (isCrouching) currentMaxSpeed *= crouchSpeedMultiplier;

        // 무게에 따른 관성 계수(SmoothTime) 결정
        float weightPercent = Mathf.Clamp01(inventory.totalWeight / weightThreshold);
        float currentSmoothTime = Mathf.Lerp(emptySmoothTime, maxWeightSmoothTime, weightPercent);

        // 목표 방향 및 속도 벡터
        Vector3 targetInputVector = (transform.right * x + transform.forward * z).normalized;
        Vector3 targetVelocity = targetInputVector * currentMaxSpeed;

        // 공중 마찰력 최소화 (방향 전환 억제)
        if (!isGrounded)
        {
            targetVelocity = Vector3.Lerp(moveDirection, targetVelocity, airControl);
        }

        // 관성 적용하여 현재 속도 벡터(moveDirection) 갱신
        moveDirection = Vector3.SmoothDamp(moveDirection, targetVelocity, ref currentInertiaVelocity, currentSmoothTime);
        
        // 실제 수평 이동
        controller.Move(moveDirection * Time.deltaTime);
        currentMoveVelocity = moveDirection; // 디버그용
    }

    void DoJump()
    {
        // 웅크리기 중 점프 차단 + 스태미나 부족 시 차단
        if (isGrounded && !isCrouching && !isExhausted && currentStamina >= jumpStaminaCost)
        {
            float currentJumpHeight = jumpHeight - (inventory.totalWeight * weightJumpPenalty);
            
            if (currentJumpHeight > 0.2f) 
            {
                verticalVelocity.y = Mathf.Sqrt(currentJumpHeight * -2f * gravity);
                
                // 스태미나 차감 (0 이하 방지)
                currentStamina = Mathf.Max(currentStamina - jumpStaminaCost, 0f);
                if (currentStamina <= 0.1f) isExhausted = true;
            }
        }
    }

    void HandleStamina()
    {
        if (isSprinting)
        {
            float totalConsume = staminaConsumeRate + (inventory.totalWeight * weightStaminaPenalty);
            currentStamina -= totalConsume * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true;
            }
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            
            // 탈진 해제 조건 (20%)
            if (isExhausted && currentStamina >= maxStamina * 0.2f)
            {
                isExhausted = false;
            }

            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }
    }

    /*void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        
        // 상태에 따른 색상 변경
        style.normal.textColor = isGrounded ? Color.green : Color.red;
        GUI.Label(new Rect(10, 10, 600, 30), $"Grounded: {isGrounded} | Crouching: {isCrouching}", style);

        style.normal.textColor = isExhausted ? Color.red : Color.white;
        GUI.Label(new Rect(10, 40, 600, 30), $"Stamina: {currentStamina:F1}% {(isExhausted ? "(EXHAUSTED)" : "")}", style);

        style.normal.textColor = Color.cyan;
        GUI.Label(new Rect(10, 70, 600, 30), $"Velocity: {currentMoveVelocity.x:F2}, {currentMoveVelocity.z:F2} (Mag: {currentMoveVelocity.magnitude:F2})", style);
        
        style.normal.textColor = Color.yellow;
        GUI.Label(new Rect(10, 100, 600, 30), $"Total Weight: {inventory.totalWeight:F1} kg", style);
    }*/
}