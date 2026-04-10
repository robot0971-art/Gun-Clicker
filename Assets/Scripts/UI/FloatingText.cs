using UnityEngine;
using TMPro;

/// <summary>
/// Floating Text (클릭 시 골드 표시)
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class FloatingText : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float fadeDuration = 1f;
    
    private TMP_Text tmpText;
    private float elapsed;
    
    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }
    
    public void Setup(string text, Color color)
    {
        if (tmpText != null)
        {
            tmpText.text = text;
            tmpText.color = color;
        }
        
        elapsed = 0f;
    }
    
    private void Update()
    {
        elapsed += Time.deltaTime;
        
        // 위로 이동
        transform.localPosition += Vector3.up * moveSpeed * Time.deltaTime;
        
        // 페이드 아웃
        if (tmpText != null)
        {
            var color = tmpText.color;
            color.a = 1f - (elapsed / fadeDuration);
            tmpText.color = color;
        }
        
        // 완료 시 제거
        if (elapsed >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }
}