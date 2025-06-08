using UnityEngine;

public class TutorialPlayer : PlayerController
{
    new void FixedUpdate()
    {
        base.FixedUpdate();

        health = maxHealth;
    }

    protected new void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (collision.gameObject.CompareTag("Fist") && collision.gameObject != attackHitbox)
        {
            if (!enemy.IsHitting()) return;

            if (isBlocking && TutorialManager.Instance.stage == 4)
            {
                TutorialManager.Instance.SetTaskProgress(1f);
            }
        }
    }
}