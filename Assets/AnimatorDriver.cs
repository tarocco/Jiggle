using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorDriver : MonoBehaviour
{
    [SerializeField]
    private Animator _Animator;
    public Animator Animator
    {
        get => _Animator;
        set => _Animator = value;
    }

    [SerializeField]
    private string _XParameterName = "X";
    public string XParameterName
    {
        get => _XParameterName;
        set => _XParameterName = value;
    }

    [SerializeField]
    private string _YParameterName = "Y";
    public string YParameterName
    {
        get => _YParameterName;
        set => _YParameterName = value;
    }

    [SerializeField]
    private Transform _OriginObject;
    public Transform OriginObject
    {
        get => _OriginObject;
        set => _OriginObject = value;
    }

    [SerializeField]
    private Transform _PointerObject;
    public Transform PointerObject
    {
        get => _PointerObject;
        set => _PointerObject = value;
    }
    
    [SerializeField]
    [Min(0.001f)]
    private float _Radius = 1f;
    public float Radius
    {
        get => _Radius;
        set => _Radius = value;
    }

    [SerializeField]
    private AnimationCurve _InfluenceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    public AnimationCurve InfluenceCurve
    {
        get => _InfluenceCurve;
        set => _InfluenceCurve = value;
    }

    [SerializeField]
    private float _Rate = 16f;
    public float Rate
    {
        get => _Rate;
        set => _Rate = value;
    }

    private Transform GetOriginObject()
    {
        if (_OriginObject)
            return _OriginObject;
        return transform;
    }

    private Animator GetAnimator()
    {
        if (_Animator)
            return _Animator;
        return GetComponent<Animator>();
    }

    void Start()
    {
    }

    private Vector2? _PreviousXY;

    void Update()
    {
        if (_PointerObject == null)
            return;
        var animator = GetAnimator();
        if (animator == null)
            return;
        var origin_object = GetOriginObject();
        var pos_difference = _PointerObject.position - origin_object.position;
        var norm_difference = pos_difference / _Radius;
        var norm_distance = norm_difference.magnitude;
        var influence = _InfluenceCurve.Evaluate(norm_distance);
        var xy = influence * new Vector2(norm_difference.x, norm_difference.y);
        var lerpxy = Vector2.Lerp(_PreviousXY ?? xy, xy, _Rate * Time.deltaTime);
        animator.SetFloat(_XParameterName, lerpxy.x);
        animator.SetFloat(_YParameterName, lerpxy.y);
        _PreviousXY = lerpxy;
    }
}
