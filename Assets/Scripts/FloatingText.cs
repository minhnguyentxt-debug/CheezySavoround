using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    // Đổi sang kiểu TextMeshPro (không phải UGUI)
    public TextMeshPro text;

    public void Setup(string content, Vector3 position)
    {
        text.text = content;

        // Đặt vị trí tại (x, z) của đĩa, cộng thêm Y để nổi lên trên mặt bàn
        transform.position = position + new Vector3(0, 1.5f, 0);

        // Cố định mặt chữ quay về hướng Y+ (ngẩng lên trời)
        transform.rotation = Quaternion.Euler(90, 0, 0);

        StartCoroutine(Animate());
    }

    private System.Collections.IEnumerator Animate()
    {
        float duration = 1.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Bay dần theo hướng Z dương (Z++)
            transform.position += Vector3.forward * Time.deltaTime * 1.5f;

            // Mờ dần
            Color c = text.color;
            c.a = Mathf.Lerp(1, 0, elapsed / duration);
            text.color = c;

            yield return null;
        }
        Destroy(gameObject);
    }
}