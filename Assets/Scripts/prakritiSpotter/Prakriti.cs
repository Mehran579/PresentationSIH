using UnityEngine;
using UnityEngine.EventSystems;

public class Prakriti : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        gameObject.SetActive(false); // touched -> gone
        Debug.Log("clicked");
    }
}