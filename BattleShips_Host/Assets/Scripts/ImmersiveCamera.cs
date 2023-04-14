using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ImmersiveCamera : MonoBehaviour
{
    [SerializeField] private List<Transform> targets;

    [SerializeField] private Vector3 offset;
    [SerializeField] private float smoothTime = 0.8f;
    [Space]
    [SerializeField] private float minZoom = 100f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private float zoomLimiter = 70f;
    [SerializeField] private bool lensZoom;
    [Space] 
    [SerializeField] private float maxDist;
    [SerializeField] private float minDist;
    [SerializeField] private float distLimiter;
    
    private Vector3 _velocity;
    private Vector3 _baseOffset;
    private Camera _camera;
    private Bounds _bounds;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _baseOffset = offset;
    }

    private void LateUpdate()
    {
        if (targets.Count == 0)
            return;
        
        _bounds = new Bounds(targets[0].position, Vector3.zero);
        foreach (var t in targets)
        {
            _bounds.Encapsulate(t.position);
        }
        
        Move();
        if (lensZoom)
            LensZoom();
        else
            DistZoom();
    }

    private void Move()
    {
        var centerPoint = GetCenterPoint();
        var newPos = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, newPos, ref _velocity, smoothTime);
    }

    private void LensZoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance()/zoomLimiter);
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, newZoom, Time.deltaTime);
    }

    private void DistZoom()
    {
        float newDist = Mathf.Lerp(maxDist, minDist, GetGreatestDistance()/distLimiter);
        offset = _baseOffset * newDist;
    }   

    private Vector3 GetCenterPoint()
    {
        return targets.Count == 1 ? targets[0].position : _bounds.center;
    }

    private float GetGreatestDistance()
    {
        return new Vector2(_bounds.size.x, _bounds.size.z).magnitude;
    }
}
