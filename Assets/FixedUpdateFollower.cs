using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedUpdateFollower : MonoBehaviour
{
    [SerializeField]
    private Transform _Target;
    public Transform Target
    {
        get => _Target;
        set => _Target = value;
    }

    [SerializeField]
    [Range(0f, 240f)]
    private float _Rate = 16f;
    public float Rate
    {
        get => _Rate;
        set => _Rate = value;
    }

    void FixedUpdate()
    {
        if (_Target == null)
            return;
        var position = Vector3.Lerp(
            transform.position,
            _Target.position,
            _Rate * Time.fixedDeltaTime);
        transform.position = position;
    }
}
