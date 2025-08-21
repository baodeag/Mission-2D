using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] RectTransform wheel;     // gán child "Wheel"
    [SerializeField] Image wheelImage;        // màu feedback
    [SerializeField] Button lockButton;       // gán child "BtnLock"

    [Header("Config")]
    [SerializeField] float speedDegPerSec = 120f; // tốc độ (+ = xoay ngược chiều kim đồng hồ)
    [SerializeField] float toleranceDeg = 10f;    // cửa sổ trúng (±)
    [SerializeField] Color baseColor = new Color32(0x3A, 0x3A, 0x3A, 0xFF);
    [SerializeField] Color successColor = new Color32(0x31, 0xB5, 0x52, 0xFF);
    [SerializeField] Color failColor = new Color32(0xE8, 0x52, 0x52, 0xFF);

    [Header("Runtime")]
    public bool IsLocked { get; private set; }
    float angle; // độ hiện tại (deg), 0° = 12 giờ
    const float TARGET_ANGLE = 0f;

    public System.Action<DialController, bool> OnAttempt; // callback về task

    void Reset()
    {
        if (!wheel) wheel = transform.Find("Wheel") as RectTransform;
        if (!wheelImage && wheel) wheelImage = wheel.GetComponent<Image>();
        if (!lockButton) lockButton = transform.Find("BtnLock")?.GetComponent<Button>();
    }

    void Awake()
    {
        if (!wheel) wheel = transform.Find("Wheel") as RectTransform;
        if (!wheelImage && wheel) wheelImage = wheel.GetComponent<Image>();
        if (!lockButton) lockButton = transform.Find("BtnLock")?.GetComponent<Button>();

        if (lockButton)
        {
            lockButton.onClick.RemoveAllListeners();
            lockButton.onClick.AddListener(TryLock);
        }
    }

    void OnEnable()
    {
        // random góc bắt đầu cho vui mắt
        angle = Random.Range(0f, 360f);
        ApplyAngle();
        SetColor(baseColor);
        IsLocked = false;
        SetInteractable(true);
    }

    void Update()
    {
        if (IsLocked || wheel == null) return;

        angle += speedDegPerSec * Time.unscaledDeltaTime; // dùng unscaled để không lệ thuộc Time.timeScale
        if (angle >= 360f) angle -= 360f;
        else if (angle < 0f) angle += 360f;
        ApplyAngle();
    }

    void ApplyAngle()
    {
        if (wheel) wheel.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    void SetColor(Color c)
    {
        if (wheelImage) wheelImage.color = c;
    }

    void SetInteractable(bool on)
    {
        if (lockButton) lockButton.interactable = on;
    }

    public void Configure(float speed, float tolerance)
    {
        speedDegPerSec = speed;
        toleranceDeg = tolerance;
    }

    public void TryLock()
    {
        if (IsLocked) return;

        float diff = Mathf.Abs(Mathf.DeltaAngle(angle, TARGET_ANGLE));
        bool ok = diff <= toleranceDeg;

        if (ok)
        {
            IsLocked = true;
            SetColor(successColor);
            SetInteractable(false);
        }
        else
        {
            // flash đỏ ngắn
            StartCoroutine(FlashFail());
        }

        OnAttempt?.Invoke(this, ok);
    }

    IEnumerator FlashFail()
    {
        SetColor(failColor);
        yield return new WaitForSecondsRealtime(0.12f);
        if (!IsLocked) SetColor(baseColor);
    }

    // Cho Task bật/tắt khóa input (khi phát hiệu ứng chung nếu cần)
    public void ForceInteractable(bool on)
    {
        if (!IsLocked) SetInteractable(on);
    }

    public void ForceUnlockAndReset()
    {
        IsLocked = false;
        SetColor(baseColor);
        SetInteractable(true);
        angle = Random.Range(0f, 360f);
        ApplyAngle();
    }
}
