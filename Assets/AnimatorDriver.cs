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
    private string _UpParameterName = "Up";
    public string UpParameterName
    {
        get => _UpParameterName;
        set => _UpParameterName = value;
    }

    [SerializeField]
    private string _DownParameterName = "Down";
    public string DownParameterName
    {
        get => _DownParameterName;
        set => _DownParameterName = value;
    }

    [SerializeField]
    private string _LeftParameterName = "Left";
    public string LeftParameterName
    {
        get => _LeftParameterName;
        set => _LeftParameterName = value;
    }

    [SerializeField]
    private string _RightParameterName = "Right";
    public string RightParameterName
    {
        get => _RightParameterName;
        set => _RightParameterName = value;
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
    private Vector2 _Rate = new Vector2(16f, 16f);
    public Vector2 Rate
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

    private static Vector4 GetLURD(Vector2 xy) => new Vector4(
        Mathf.Clamp01(xy.x),
        Mathf.Clamp01(xy.y),
        Mathf.Clamp01(-xy.x),
        Mathf.Clamp01(-xy.y));

    private static Vector2 LERP_XY(Vector2 a, Vector2 b, Vector2 t)
    {
        return new Vector2(Mathf.Lerp(a.x, b.x, t.x), Mathf.Lerp(a.y, b.y, t.y));
    }

    private static Vector4 LERP_LURD_XY(Vector4 a, Vector4 b, Vector2 t)
    {
        return new Vector4(
            Mathf.Lerp(a.x, b.x, t.x),
            Mathf.Lerp(a.y, b.y, t.y),
            Mathf.Lerp(a.z, b.z, t.x),
            Mathf.Lerp(a.w, b.w, t.y));
    }

    private static TVector TryLerp<TVector, TInterpolant>(
        System.Func<TVector, TVector, TInterpolant, TVector> func,
        TVector? a,
        TVector b,
        TInterpolant t)
        where TVector : struct
    {
        return func(a ?? b, b, t);
    }

    private Vector2? _PreviousXY;
    private Vector4? _PreviousLURD;

    void Start()
    {
    }

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
        var t = _Rate * Time.deltaTime;
        var lerpxy = TryLerp(LERP_XY, _PreviousXY, xy, t);
        animator.SetFloat(_XParameterName, lerpxy.x);
        animator.SetFloat(_YParameterName, lerpxy.y);
        var lurd = GetLURD(xy);
        var lerplurd = TryLerp(LERP_LURD_XY, _PreviousLURD, lurd, t);
        animator.SetFloat(_LeftParameterName, Mathf.Clamp01(lerplurd.x));
        animator.SetFloat(_UpParameterName, Mathf.Clamp01(lerplurd.y));
        animator.SetFloat(_RightParameterName, Mathf.Clamp01(lerplurd.z));
        animator.SetFloat(_DownParameterName, Mathf.Clamp01(lerplurd.w));
        _PreviousXY = lerpxy;
        _PreviousLURD = lerplurd;
    }
}
