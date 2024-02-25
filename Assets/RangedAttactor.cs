using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RangedAttactor : MonoBehaviour
{
    [SerializeField]
    private float _Radius = 1f;
    public float Radius
    {
        get => _Radius;
        set => _Radius = value;
    }

    [SerializeField]
    private float _Force = 1f;
    public float Force
    {
        get => _Force;
        set => _Force = value;
    }

    [SerializeField]
    private bool _MultiplyMass = false;
    public bool MultiplyMass
    {
        get => _MultiplyMass;
        set => _MultiplyMass = value;
    }

    [SerializeField]
    private AnimationCurve _FalloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    public AnimationCurve FalloffCurve
    {
        get => _FalloffCurve;
        set => _FalloffCurve = value;
    }

    [SerializeField]
    private AnimationCurve _SpeedToForceCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    public AnimationCurve SpeedToForceCurve
    {
        get => _SpeedToForceCurve;
        set => _SpeedToForceCurve = value;
    }

    [SerializeField]
    [Range(1, 12)]
    private int _InterpolationSteps = 1;
    public int InterpolationSteps
    {
        get => _InterpolationSteps;
        set => _InterpolationSteps = Mathf.Max(1, 32);
    }

    private HashSet<Collider> _Colliders = new();

    void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        _Colliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _Colliders.Remove(other);
    }

    private HashSet<Rigidbody> _VisitedRigidbodies = new();


    private Vector3? _PreviousPosition;
    private Quaternion? _PreviousRotation;
    private Vector3? _PreviousScale;


    private Vector3? _GizmosPreviousPosition;
    private Quaternion? _GizmosPreviousRotation;
    private Vector3? _GizmosPreviousScale;

    void FixedUpdateStep(Matrix4x4 mtx, float amount)
    {
        var nearest_rb = _Colliders
            .Select(c => (p: c.ClosestPoint(mtx.GetPosition()), rb: c.attachedRigidbody))
            .Select(e => (p: e.p, lp: mtx.inverse.MultiplyPoint(e.p), rb: e.rb))
            .OrderBy(e => e.lp.magnitude);
        _VisitedRigidbodies.Clear();
        var multiplier = 1f;
        foreach (var (p, lp, rb) in nearest_rb)
        {
            if (_VisitedRigidbodies.Add(rb))
            {
                var direction = -lp;
                var force = _Force * _FalloffCurve.Evaluate(lp.magnitude / _Radius);
                if (_MultiplyMass)
                    force *= rb.mass;
                force *= multiplier;
                rb.AddForceAtPosition(amount * force * direction.normalized, p);
            }
        }
        
    }

    void FixedUpdate()
    {
        var multiplier = 1f;
        if (_PreviousPosition.HasValue)
        {
            var distance_traveled = Vector3.Distance(_PreviousPosition.Value, transform.position);
            var t = distance_traveled / Time.fixedDeltaTime;
            multiplier *= _SpeedToForceCurve.Evaluate(t);
        }
        var amount = multiplier / InterpolationSteps;
        for(var i = 0; i < InterpolationSteps; i++)
        {
            var t = (float)i / (float)InterpolationSteps;
            var mtx = Matrix4x4.TRS(
                Vector3.Lerp(transform.position, _PreviousPosition ?? transform.position, t),
                Quaternion.Slerp(transform.rotation, _PreviousRotation ?? transform.rotation, t),
                Vector3.Lerp(transform.lossyScale, _PreviousScale ?? transform.lossyScale, t));
            FixedUpdateStep(mtx, amount);
        }
        _PreviousPosition = transform.position;
        _PreviousRotation = transform.rotation;
        _PreviousScale = transform.lossyScale;
    }

    void OnDrawGizmosStep(Matrix4x4 mtx)
    {
        Gizmos.color = Color.white;
        Gizmos.matrix = mtx;
        Gizmos.DrawWireSphere(Vector3.zero, _Radius);
        Gizmos.matrix = Matrix4x4.identity;
        if (!Application.isPlaying)
            return;
        var nearest_rb = _Colliders
            .Select(c => (p: c.ClosestPoint(mtx.GetPosition()), rb: c.attachedRigidbody))
            .Select(e => (p: e.p, lp: mtx.inverse.MultiplyPoint(e.p), rb: e.rb))
            .OrderBy(e => e.lp.magnitude);
        var visited_rb = new HashSet<Rigidbody>();
        foreach (var (p, lp, rb) in nearest_rb)
        {
            if (visited_rb.Add(rb))
            {
                var direction = -lp;
                var c = _FalloffCurve.Evaluate(lp.magnitude / _Radius);
                Gizmos.color = Color.HSVToRGB(0.333f * c, 1f, 1f);
                Gizmos.DrawLine(mtx.GetPosition(), p);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (var i = 0; i < InterpolationSteps; i++)
        {
            var t = (float)i / (float)InterpolationSteps;
            var mtx = Matrix4x4.TRS(
                Vector3.Lerp(transform.position, _GizmosPreviousPosition ?? transform.position, t),
                Quaternion.Slerp(transform.rotation, _GizmosPreviousRotation ?? transform.rotation, t),
                Vector3.Lerp(transform.lossyScale, _GizmosPreviousScale ?? transform.lossyScale, t));
            OnDrawGizmosStep(mtx);
        }
        _GizmosPreviousPosition = transform.position;
        _GizmosPreviousRotation = transform.rotation;
        _GizmosPreviousScale = transform.lossyScale;
    }
}
