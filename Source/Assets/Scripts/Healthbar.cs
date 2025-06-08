using System.Collections;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    private float health = 100f;
    private float damageStreak = 0f;
    private float maxHealth = 100f;

    private RectTransform damageLayer;
    private RectTransform healthLayer;
    private Coroutine clearDamageStreakCoroutine;

    void Start()
    {
        damageLayer = transform.GetChild(2).GetComponent<RectTransform>();
        healthLayer = transform.GetChild(3).GetComponent<RectTransform>();
    }

    private void RedrawHealthbar()
    {
        if (!healthLayer || !damageLayer) return;

        float healthPercentage = health / maxHealth;
        healthLayer.localScale = new Vector2(healthPercentage, 1);

        if (damageStreak > 0)
        {
            float damagePercentage = damageStreak / maxHealth;
            damageLayer.localScale = new Vector2(damagePercentage + healthPercentage, 1);
        }
        else
        {
            damageLayer.localScale = new Vector2(0, 1);
        }
    }

    public void SetHealth(float health)
    {
        this.health = health;
        RedrawHealthbar();
    }

    public void SetMaxHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
        damageStreak = 0f;
        RedrawHealthbar();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        damageStreak += damage;

        RedrawHealthbar();

        if (clearDamageStreakCoroutine != null)
        {
            StopCoroutine(clearDamageStreakCoroutine);
        }

        clearDamageStreakCoroutine = StartCoroutine(ClearDamageStreak());
    }

    IEnumerator ClearDamageStreak()
    {
        yield return new WaitForSeconds(1f);
        damageStreak = 0f;
        RedrawHealthbar();
        clearDamageStreakCoroutine = null;
    }
}
