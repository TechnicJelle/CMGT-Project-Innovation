using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boat : MonoBehaviour
{
    private Rigidbody _rb;
    private Vector3 _direction;
    [SerializeField][Range(0, 360)]private float rotation;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotSpeed;
    void Awake () {
        _rb = GetComponent<Rigidbody>();
        _direction = Vector3.forward;
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        _direction = Quaternion.Euler(new Vector3(0, rotation, 0)) * Vector3.forward;
        Debug.Log(_direction);
        var forward = transform.forward;
        forward = Vector3.Lerp(forward, _direction, rotSpeed);
        transform.forward = forward;
        //transform.Rotate(new Vector3(0, rotation, 0));
        _rb.MovePosition(forward * (moveSpeed * Time.deltaTime));
        
    }
}
