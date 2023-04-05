using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boat : MonoBehaviour
{
    private Rigidbody _rb;
    private Vector3 _direction;
    [SerializeField][Range(0, 360)]private float rotation;
    [SerializeField] private float moveSpeed;
    [SerializeField] [Range(0, 0.1f)]private float rotSpeed;
    [SerializeField] private bool go = false;

    private void Awake () {
        _rb = GetComponent<Rigidbody>();
        Debug.Log(_rb);
        _direction = Vector3.forward;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        _direction = Quaternion.Euler(new Vector3(0, rotation, 0)) * Vector3.forward;
        var forward = transform.forward;
        forward = Vector3.Lerp(forward, _direction, rotSpeed);
        transform.forward = forward;
        if (go)
            _rb.AddForce(forward * (moveSpeed * Time.fixedDeltaTime * 100));
    }
}
