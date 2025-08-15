using UnityEngine;
using System;

public abstract class MiniGameBase : MonoBehaviour
{
    public event Action<bool> OnFinished; // true=hoàn thành, false=hủy
    protected bool isActive;

    public virtual void Open()
    {
        gameObject.SetActive(true);
        isActive = true;
        OnOpened();
    }

    public virtual void Close(bool completed)
    {
        if (!isActive) return;
        isActive = false;
        gameObject.SetActive(false);
        OnFinished?.Invoke(completed);
    }

    protected virtual void OnOpened() { }
}
