using System.Collections;
using UnityEngine;

public class SlideMovement : MonoBehaviour
{
    public float speed = 10f;
    public bool isActive = false;
    public Sprite slideSprite;
    public Sprite standSprite;
    public Transform target;
    private SpriteRenderer spriteRenderer;
    private Color endColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isActive)
        {
            if (target.position.x > transform.position.x)
            {
                transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
            }
            else if (target.position.x < transform.position.x)
            {
                transform.position -= new Vector3(speed * Time.deltaTime, 0, 0);
            }

            if (Mathf.Abs(target.position.x - transform.position.x) < 0.1f)
            {
                EndSlide();
                transform.position = target.position;
            }
        }
    }

    public void StartSlide(Color startColor, Color endColor)
    {
        isActive = true;
        spriteRenderer.color = startColor;
        transform.position = new Vector2(-5, target.position.y);
        spriteRenderer.sprite = slideSprite;
        spriteRenderer.enabled = true;
        this.endColor = endColor;
    }

    void EndSlide()
    {
        isActive = false;
        spriteRenderer.color = endColor;
        spriteRenderer.sprite = standSprite;
        target.GetComponent<PlayerController>().SwapStats();

        // Fade out the sprite
        StartCoroutine(Fade());
    }

    public void CancelSlide()
    {
        isActive = false;
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        float fadeDuration = 0.5f;
        float elapsedTime = 0f;

        Color startColor = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            spriteRenderer.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0), t);
            yield return null;
        }

        spriteRenderer.enabled = false; // Disable the sprite renderer after fading out
    }
}
