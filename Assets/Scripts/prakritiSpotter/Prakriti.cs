using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Prakriti : MonoBehaviour, IPointerDownHandler
{
    RectTransform rect;
    Vector3 originalScale;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
    }

    public void PopIn()
    {
        StopAllCoroutines();
        StartCoroutine(PopAnimation());
    }

    IEnumerator PopAnimation()
    {
        rect.localScale = Vector3.zero;

        float duration = 0.18f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Overshoot
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.15f;

            rect.localScale = originalScale * scale;

            yield return null;
        }

        rect.localScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(SquishAnimation());

        Debug.Log("clicked");
    }

    IEnumerator SquishAnimation()
    {
        // BIG horizontal squish
        rect.localScale = new Vector3(
            originalScale.x * 1.35f,
            originalScale.y * 0.55f,
            originalScale.z
        );

        yield return new WaitForSeconds(0.07f);

        // Bounce back slightly
        rect.localScale = new Vector3(
            originalScale.x * 0.9f,
            originalScale.y * 1.1f,
            originalScale.z
        );

        yield return new WaitForSeconds(0.06f);

        // Disappear
        rect.localScale = Vector3.zero;

        gameObject.SetActive(false);

        // Reset for next appearance
        rect.localScale = originalScale;
    }
}