using UnityEngine;

public class RandomizeBackground : MonoBehaviour
{
    public Sprite[] backgrounds;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        int randomIndex = Random.Range(0, backgrounds.Length);
        spriteRenderer.sprite = backgrounds[randomIndex];
    }
}
