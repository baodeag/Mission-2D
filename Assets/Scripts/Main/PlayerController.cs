using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 4.5f;
    public float gravity = -12f;
    public Transform cam;

    CharacterController cc;
    Vector3 velocity;

    bool controlLocked => MiniGameManager.Exists && MiniGameManager.Instance.IsBusy;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!cam) cam = Camera.main ? Camera.main.transform : null;
    }

    void Update()
    {
        // Nếu đang mở mini-game: đứng yên (chỉ áp trọng lực)
        if (controlLocked)
        {
            if (cc.isGrounded && velocity.y < 0) velocity.y = -2f;
            velocity.y += gravity * Time.deltaTime;
            cc.Move(velocity * Time.deltaTime);
            return;
        }

        float h = Input.GetAxisRaw("Horizontal"); // A/D, ←/→
        float v = Input.GetAxisRaw("Vertical");   // W/S, ↑/↓

        Vector3 fwd = cam ? Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized : transform.forward;
        Vector3 right = cam ? cam.right : transform.right;

        Vector3 dir = (fwd * v + right * h).normalized;
        Vector3 move = dir * moveSpeed;

        // áp vào velocity (x,z). y do gravity quản lý
        velocity.x = move.x;
        velocity.z = move.z;

        // gravity
        if (cc.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;

        cc.Move(velocity * Time.deltaTime);

        // quay theo hướng di chuyển
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 12f * Time.deltaTime);
    }
}
