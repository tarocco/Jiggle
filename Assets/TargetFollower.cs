using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TargetFollower : MonoBehaviour
{
    private Rigidbody _Rigidbody;
    public Rigidbody Rigidbody
    {
        set => _Rigidbody = value;
    }

    [SerializeField]
    private Transform _Target;
    public Transform Target
    {
        get => _Target;
        set => _Target = value;
    }

    [SerializeField]
    private float _Rate = 1f;
    public float Rate
    {
        get => _Rate;
        set => _Rate = value;
    }

    void Start()
    {
        _Rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (_Target == null)
            return;
        var position = Vector3.Lerp(_Rigidbody.position, _Target.position, _Rate / Time.fixedDeltaTime);
        _Rigidbody.MovePosition(position);
    }
}
