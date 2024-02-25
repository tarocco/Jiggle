using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class AspectRatioSlider : MonoBehaviour
{
    [SerializeField]
    private Camera _Camera;
    public Camera Camera
    {
        get => _Camera;
        set => _Camera = value;
    }

    [SerializeField]
    [Min(0.0001f)]
    private Vector2 _AspectRatioRange = new Vector2(1f, 2f);
    public Vector2 AspectRatioRange
    {
        get => _AspectRatioRange;
        set => _AspectRatioRange = value;
    }

    [SerializeField]
    private Vector2 _Amount = Vector2.zero;
    public Vector2 Amount
    {
        get => _Amount;
        set => _Amount = value;
    }

    private Camera GetCamera()
    {
        if (_Camera != null)
            return _Camera;
        return Camera.main;
    }

    void Start()
    {
    }

    void Update()
    {
        var camera = GetCamera();
        if (camera == null)
            return;
        var min_ratio = camera.aspect / Mathf.Max(_AspectRatioRange.x, camera.aspect);
        var max_ratio = Mathf.Min(_AspectRatioRange.y, camera.aspect) / camera.aspect;
        var shift_x = Mathf.Lerp(_Amount.x, 0f, min_ratio);
        var shift_y = Mathf.Lerp(_Amount.y, 0f, max_ratio);
        transform.localPosition = new Vector3(shift_x, shift_y, 0f);
    }
}
