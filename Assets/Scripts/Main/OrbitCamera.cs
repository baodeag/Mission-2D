using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // kéo Player
    public Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Orbit")]
    public bool rightMouseToRotate = true;   // giữ chuột phải mới xoay; OFF = luôn xoay
    public float sensitivityX = 4.0f;
    public float sensitivityY = 2.5f;
    public float minPitch = -30f;
    public float maxPitch = 70f;

    [Header("Zoom")]
    public float distance = 5f;
    public float minDistance = 1.2f;
    public float maxDistance = 8f;
    public float zoomSpeed = 4f;

    [Header("Smoothing")]
    public float rotationLerp = 12f;         // cao = mượt
    public float positionLerp = 12f;

    [Header("Collision")]
    public LayerMask collisionMask = ~0;     // nhớ bỏ layer Player
    public float collisionRadius = 0.25f;

    float yaw;   // quay ngang (Y)
    float pitch; // ngẩng/cúi (X)
    Vector3 currentPos;
    Quaternion currentRot;

    bool Busy => MiniGameManager.Exists && MiniGameManager.Instance.IsBusy;

    void Start()
    {
        if (!target) { Debug.LogWarning("[OrbitCamera] Chưa gán Target."); return; }
        // Khởi tạo góc nhìn từ vị trí hiện tại của camera
        Vector3 look = (target.position + targetOffset) - transform.position;
        var e = Quaternion.LookRotation(look, Vector3.up).eulerAngles;
        yaw = e.y;
        pitch = Mathf.Clamp(NormalizePitch(e.x), minPitch, maxPitch);

        currentRot = Quaternion.Euler(pitch, yaw, 0f);
        currentPos = transform.position;

        // bỏ Player khỏi collision mask (nếu Player ở layer "Player")
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0) collisionMask &= ~(1 << playerLayer);
    }

    void Update()
    {
        if (!target) return;

        // Nếu đang mở mini-game: thả chuột & đứng yên
        if (Busy)
        {
            UnlockCursor();
            ApplyTransform(Time.deltaTime);
            return;
        }

        // Zoom bằng cuộn chuột
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
        }

        bool rotating = !rightMouseToRotate || Input.GetMouseButton(1); // RMB
        if (rotating)
        {
            LockCursor();

            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            yaw += mx * sensitivityX;
            pitch -= my * sensitivityY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        ApplyTransform(Time.deltaTime);
    }

    void ApplyTransform(float dt)
    {
        if (!target) return;

        // Rotation mượt
        Quaternion desiredRot = Quaternion.Euler(pitch, yaw, 0f);
        currentRot = Quaternion.Slerp(currentRot, desiredRot, rotationLerp * dt);

        // Vị trí mong muốn (quay lùi theo trục Z)
        Vector3 focus = target.position + targetOffset;
        Vector3 desiredPos = focus - (currentRot * Vector3.forward) * distance;

        // Né va chạm giữa focus -> desiredPos
        Vector3 finalPos = desiredPos;
        Vector3 rayDir = (desiredPos - focus);
        float rayDist = rayDir.magnitude;
        if (rayDist > 0.001f)
        {
            rayDir /= rayDist;
            if (Physics.SphereCast(focus, collisionRadius, rayDir, out var hit, rayDist, collisionMask, QueryTriggerInteraction.Ignore))
            {
                finalPos = hit.point + hit.normal * collisionRadius;
            }
        }

        // Lerp vị trí
        currentPos = Vector3.Lerp(currentPos, finalPos, positionLerp * dt);

        // Áp vào camera
        transform.SetPositionAndRotation(currentPos, currentRot);
        // Nhìn về target (giữ ổn định)
        transform.LookAt(focus);
    }

    void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Quy chuẩn hoá pitch từ 0..360 về -180..180 để clamp dễ
    float NormalizePitch(float x)
    {
        if (x > 180f) x -= 360f;
        return x;
    }
}
