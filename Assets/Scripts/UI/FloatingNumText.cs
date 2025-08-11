using TMPro;
using UnityEngine;

public class FloatingNumText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float lifeTime = 1f;

    private Color _startColor;
    private float _timer;
    private TextMeshProUGUI _tmp;

    private void Awake()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        _startColor = _tmp.color;
    }

    private void Update()
    {
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        _timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1, 0, _timer / lifeTime);
        _tmp.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);

        if (_timer >= lifeTime) {
            Destroy(gameObject);
        }
    }

    public void SetText(string text)
    {
        _tmp.text = text;
    }
}
