using UnityEngine;

public class Unit_Lifter : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteRight;

    private Vector2 _movementInput;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        _movementInput.x = Input.GetAxisRaw("Horizontal");
        _movementInput.y = Input.GetAxisRaw("Vertical");
        _movementInput.Normalize();

        UpdateSpriteDirection();
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _movementInput * moveSpeed;
    }

    private void UpdateSpriteDirection()
    {
        if (_movementInput != Vector2.zero)
        {
            if (Mathf.Abs(_movementInput.x) > 0)
            {
                _sr.flipX = _movementInput.x < 0;
                _sr.sprite = spriteRight;
            }
            else if (_movementInput.y > 0)
            {
                _sr.sprite = spriteUp;
            }
            else if (_movementInput.y < 0)
            {
                _sr.sprite = spriteDown;
            }
        }
    }
}