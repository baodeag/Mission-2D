using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 4.5f;
    public Transform cam;

    [Header("Gravity")]
    public float airGravity = -20f;     // khi không chạm đất
    public float groundedGravity = -0.1f; // lực dính đất rất nhỏ

    [Header("Grounding (probe)")]
    public LayerMask groundMask = ~0;   // bỏ layer Player
    public float groundProbeRadius = 0.35f;   // ≈ cc.radius
    public float groundProbeOffset = 0.05f;   // nâng điểm cast lên 1 chút
    public float groundSnapDistance = 0.6f;   // tầm dò mặt đất
    public float stepOffsetGrounded = 0.3f;   // StepOffset khi chạm đất

    CharacterController cc;
    Vector3 velocity;
    bool grounded;
    Vector3 groundNormal = Vector3.up;
    RaycastHit groundHit;

    bool Busy => MiniGameManager.Exists && MiniGameManager.Instance.IsBusy;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!cam) cam = Camera.main ? Camera.main.transform : null;

        // đảm bảo các thông số CC hợp lý
        cc.minMoveDistance = 0f;
        // đặt layer cho Player để camera & mask dễ loại bỏ
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0) gameObject.layer = playerLayer;
    }

    void Update()
    {
        // 1) Ground probe trước khi tính di chuyển
        GroundProbe();

        // 2) Tính input di chuyển theo camera
        Vector3 camFwd = cam ? Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 camRight = cam ? Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized : Vector3.right;

        float h = Busy ? 0f : Input.GetAxisRaw("Horizontal");
        float v = Busy ? 0f : Input.GetAxisRaw("Vertical");

        Vector3 desiredDir = (camFwd * v + camRight * h);
        if (desiredDir.sqrMagnitude > 1f) desiredDir.Normalize();

        // Nếu chạm đất, project hướng lên mặt phẳng đất để đi dốc mượt
        Vector3 moveDir = grounded ? Vector3.ProjectOnPlane(desiredDir, groundNormal).normalized : desiredDir;

        Vector3 move = moveDir * moveSpeed;
        velocity.x = move.x;
        velocity.z = move.z;

        // 3) Gravity/stepOffset
        if (grounded)
        {
            if (velocity.y < 0f) velocity.y = groundedGravity;
            cc.stepOffset = stepOffsetGrounded;
        }
        else
        {
            velocity.y += airGravity * Time.deltaTime;
            cc.stepOffset = 0f; // đang trên không thì tắt step để tránh giật
        }

        // 4) Thực sự di chuyển
        cc.Move(velocity * Time.deltaTime);

        // 5) Nhấc khỏi đất nếu lún bởi skin width
        if (grounded && velocity.y <= 0f) StickToGround();

        // 6) Quay mặt theo hướng đi
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion face = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, face, 12f * Time.deltaTime);
        }
    }

    void GroundProbe()
    {
        Vector3 origin = transform.position + Vector3.up * (cc.radius + groundProbeOffset);
        if (Physics.SphereCast(origin, groundProbeRadius, Vector3.down, out groundHit,
                               groundSnapDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            grounded = true;
            groundNormal = groundHit.normal;
        }
        else
        {
            grounded = false;
            groundNormal = Vector3.up;
        }
    }

    void StickToGround()
    {
        // mục tiêu: đáy capsule nằm ngay trên mặt đất
        float desiredY = groundHit.point.y + cc.height * 0.5f; // vì center=(0, height/2, 0)
        float deltaY = desiredY - transform.position.y;
        if (deltaY > 0f)
        {
            // nhấc nhẹ (clamp để không teleport mạnh)
            cc.Move(Vector3.up * Mathf.Min(deltaY, 0.2f));
        }
    }
}
