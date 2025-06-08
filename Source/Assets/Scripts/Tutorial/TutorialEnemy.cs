using System.Collections;
using UnityEngine;

public class TutorialEnemy : Fighter
{
    private bool isPeaceful = false;
    private bool updateHealthbar = true;

    protected new void FixedUpdate()
    {
        base.FixedUpdate();

        if (!isPastStage(7))
        {
            health = maxHealth;
        }

        switch (TutorialManager.Instance.stage)
        {
            case 4:
                if (state == State.Idle && enemy.state != State.Stunned && !isPeaceful)
                {
                    Attack(0);
                    StartCoroutine(GracePeriod());
                }
                break;

            case 5:
                SetBlocking(true);
                break;

            case 6:
                SetBlocking(false);
                break;

            case 8:
                if (updateHealthbar)
                {
                    healthbar.SetHealth(health);
                    healthbar.SetMaxHealth(maxHealth);
                    updateHealthbar = false;
                }
                break;
        }
    }

    protected override float GetMovement()
    {
        if (TutorialManager.Instance.stage != 4) return 0f;

        float distanceToPlayer = Mathf.Abs(transform.position.x - enemy.transform.position.x);

        if (distanceToPlayer <= 1.25) return 0f;

        return transform.localScale.x * distanceToPlayer / Mathf.Abs(distanceToPlayer);
    }

    protected override void Damage(float damage)
    {
        base.Damage(damage);

        TutorialManager.Instance.EnemyWasHit(enemy.state);
    }

    bool isPastStage(int stage)
    {
        return TutorialManager.Instance.stage > stage;
    }

    IEnumerator GracePeriod()
    {
        isPeaceful = true;
        yield return new WaitForSeconds(2f);
        isPeaceful = false;
    }
}