using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Canvas))]
public class CameraCanvasSizeAdjuster : MonoBehaviour
{
    private Canvas _Canvas;
    public Canvas Canvas
    {
        get => _Canvas;
    }

    private CanvasScaler _CanvasScaler;
    public CanvasScaler CanvasScaler
    {
        get => _CanvasScaler;
    }

    [SerializeField]
    private float _OrthoCameraSize = 5f;
    public float OrthoCameraSize
    {
        get => _OrthoCameraSize;
        set => _OrthoCameraSize = value;
    }

    [SerializeField]
    private float _MaxAspectRatio = 1.25f;
    public float MaxAspectRatio
    {
        get => _MaxAspectRatio;
        set => _MaxAspectRatio = value;
    }

    [SerializeField]
    private List<Camera> _AdditionalCameras;
    public IReadOnlyList<Camera> AdditionalCameras
    {
        get => _AdditionalCameras;
    }

    private float? GetOrthographicSize()
    {
        if (_Canvas == null)
            return null;
        var canvas_rect_transform = _Canvas.transform as RectTransform;
        if (canvas_rect_transform == null)
            return null;
        var canvas_aspect_ratio = canvas_rect_transform.sizeDelta.x / canvas_rect_transform.sizeDelta.y;
        var orthographic_size = _OrthoCameraSize / Mathf.Max(1, canvas_aspect_ratio / MaxAspectRatio);
        return orthographic_size;
    }

    private void _UpdateCameras(float orthographic_size)
    {
        var camera = _Canvas.worldCamera;
        if (camera != null)
            camera.orthographicSize = orthographic_size;
        foreach (var cam in _AdditionalCameras)
        {
            if (cam == null)
                continue;
            cam.orthographicSize = orthographic_size;
        }
    }

    void Start()
    {
        _Canvas = GetComponent<Canvas>();
        if (_Canvas != null)
            _CanvasScaler = _Canvas.GetComponent<CanvasScaler>();
        var orthograpic_size = GetOrthographicSize();
        if (orthograpic_size.HasValue)
            _UpdateCameras(orthograpic_size.Value);
    }

    void Update()
    {
        var orthograpic_size = GetOrthographicSize();
        if (orthograpic_size.HasValue)
            _UpdateCameras(orthograpic_size.Value);
    }
}
