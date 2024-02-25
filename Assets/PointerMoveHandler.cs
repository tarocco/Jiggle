using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class PointerMoveHandler : MonoBehaviour, IPointerMoveHandler
{
    [SerializeField]
    private Camera _Camera;
    public Camera Camera
    {
        get
        {
            if (_Camera != null)
                return _Camera;
            return Camera.main;
        }
        set
        {
            _Camera = value;
        }
    }

    [SerializeField]
    private int _PointerId = -1;
    public int PointerId
    {
        get => _PointerId;
        set => _PointerId = value;
    }

    [SerializeField]
    private RaycastMethod _RaycastMethod = RaycastMethod.CurrentRaycast;
    public RaycastMethod RaycastMethod
    {
        get => _RaycastMethod;
        set => _RaycastMethod = value;
    }

    [SerializeField]
    private bool _RequirePressRaycast = false;
    public bool RequirePressRaycast
    {
        get => _RequirePressRaycast;
        set => _RequirePressRaycast = value;
    }

    [SerializeField]
    private UnityEvent<Vector3> _PointerMove = new UnityEvent<Vector3>();
    public event UnityAction<Vector3> PointerMove
    {
        add => _PointerMove.AddListener(value);
        remove => _PointerMove.RemoveListener(value);
    }

    private RectTransform _RectTransform;
    public RectTransform RectTransform
    {
        get => _RectTransform;
    }

    void Start()
    {
        _RectTransform = GetComponent<RectTransform>();
    }

    protected void OnSetPointerScreenPosition(Vector2 screen_position)
    {
        var camera = Camera;
        Vector3 world_position;
        var cursor_in_plane = RectTransformUtility.ScreenPointToWorldPointInRectangle(
            RectTransform,
            screen_position,
            camera,
            out world_position);
        if (cursor_in_plane)
        {
            var cursor_in_rect = RectTransformUtility.RectangleContainsScreenPoint(
                RectTransform,
                screen_position,
                camera);
            if (cursor_in_rect)
                _PointerMove.Invoke(world_position);
        }
    }

    private static RaycastResult GetPointerRaycast(PointerEventData eventData, RaycastMethod method)
    {
        switch (method)
        {
            default:
            case RaycastMethod.CurrentRaycast:
                return eventData.pointerCurrentRaycast;
            case RaycastMethod.PressRaycast:
                return eventData.pointerPressRaycast;
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (eventData.pointerId != _PointerId)
            return;
        if (_RequirePressRaycast && !eventData.pointerPressRaycast.isValid)
            return;
        var raycast = GetPointerRaycast(eventData, _RaycastMethod);
        if (!raycast.isValid)
            return;
        // Optimization: only calculate the world position from screen position
        // if the world position is not already defined.
        // Ideally, the result should be the same.
        var world_position = raycast.worldPosition;
        if (world_position == Vector3.zero)
            OnSetPointerScreenPosition(eventData.position);
        else
            _PointerMove.Invoke(world_position);
    }
}