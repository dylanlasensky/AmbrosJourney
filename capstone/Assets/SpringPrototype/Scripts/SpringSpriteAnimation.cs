using UnityEngine;
public class SpringSpriteAnimation : MonoBehaviour
{
    public Sprite[] frames;
    void Update()
    {
        var r = GetComponent<SpriteRenderer>();
        if (frames != null && frames.Length > 0) r.sprite = frames[(int)(Time.time*7)%frames.Length];
        r.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 10);
    }
}
