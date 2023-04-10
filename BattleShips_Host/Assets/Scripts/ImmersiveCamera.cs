using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ImmersiveCamera : MonoBehaviour
{
    public static ImmersiveCamera instance;
    
    [SerializeField] private List<Transform> targets;

    [SerializeField] private Vector3 offset;
    [SerializeField] private float smoothTime = 0.8f;
    [SerializeField] private float minZoom = 100f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private float zoomLimiter = 70f;

    private Vector3 _velocity;
    private Camera _camera;
    private Bounds _bounds;

    private void Awake()
    {
        if (instance == null)
        {
            _camera = GetComponent<Camera>();
            instance = this;
        }
        else
        {
            Debug.LogError($"There is more then one {this} in this scene!");
        }
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
        Zoom();
    }

    private void Move()
    {
        var centerPoint = GetCenterPoint();
        var newPos = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, newPos, ref _velocity, smoothTime);
    }

    private void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance()/zoomLimiter);
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, newZoom, Time.deltaTime);
    }

    private Vector3 GetCenterPoint()
    {
        return targets.Count == 1 ? targets[0].position : _bounds.center;
    }

    private float GetGreatestDistance()
    {
        return new Vector2(_bounds.size.x, _bounds.size.z).magnitude;
    }

    public void AddPlayer(Transform playerBoat)
    {
        targets.Add(playerBoat);
    }

    public void ClearPlayers()
    {
        targets.Clear();
    }
}
