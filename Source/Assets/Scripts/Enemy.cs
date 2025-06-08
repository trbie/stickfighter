using System;
using System.Collections;
using UnityEngine;
using static PlayerController;

public class Enemy : Fighter
{
    public float attackDistance = 1.25f;
    public float attackDelay = 4f;

    protected bool isPeaceful = false;

    public enum AIMode
    {
        Defensive,
        Offensive
    }

    private readonly float blockChance = 0.5f;

    private readonly float reactionTime = 0.175f;

    private float attackDetectedTime = -1f;

    private bool blockAttemptedAlready = false;

    private AIMode currentMode = AIMode.Defensive;

    protected new void FixedUpdate()
    {
        if (enemy == null) return;
        if (enemy.state == State.Dead) return;

        base.FixedUpdate();

        bool playerAttacking = enemy.IsHitting();

        if (playerAttacking)
        {
            if (attackDetectedTime < 0f)
            {
                attackDetectedTime = Time.time;
                blockAttemptedAlready = false;
            }
            else if (!blockAttemptedAlready && Time.time - attackDetectedTime >= reactionTime)
            {
                // Additional state check for stun/damage
                if (state != State.Stunned && !isBlocking && UnityEngine.Random.value <= blockChance)
                {
                    StartCoroutine(BlockPeriod());
                }
                blockAttemptedAlready = true;
            }
        }
        else
        {
            attackDetectedTime = -1f;
            blockAttemptedAlready = false;
        }

        if (health <= maxHealth * 0.3)
        {
            currentMode = AIMode.Offensive;
        }
        else
        {
            currentMode = AIMode.Defensive;
        }

        float distanceToPlayer = GetDistanceToPlayer();

        if (currentMode == AIMode.Defensive)
        {
            DefensiveMode(distanceToPlayer);
        }
        else
        {
            OffensiveMode(distanceToPlayer);
        }
    }

    public void SwitchMoveset(PlayerType newType)
    {
        if (currentMode != AIMode.Offensive) return;

        var lightPunching = hitValues[State.LightPunching];
        var kicking = hitValues[State.Kicking];

        switch (newType)
        {
            case PlayerType.Speedy:
                lightPunching.knockback = -1.5f;
                hitValues[State.LightPunching] = lightPunching;

                kicking.knockback = 1.8f;
                hitValues[State.Kicking] = kicking;
                break;

            case PlayerType.Tanky:
                lightPunching.knockback = 1.5f;
                hitValues[State.LightPunching] = lightPunching;

                kicking.knockback = 3f;
                hitValues[State.Kicking] = kicking;
                break;
        }
    }

    private void DefensiveMode(float distanceToPlayer)
    {
        if (state == State.Idle && distanceToPlayer <= attackDistance && !isPeaceful)
        {
            int attack = UnityEngine.Random.Range(0, 3);
            Attack(attack);

            isPeaceful = true;
            StartCoroutine(GracePeriod());
        }
    }

    private void OffensiveMode(float distanceToPlayer)
    {
        if (state == State.Idle && distanceToPlayer <= attackDistance)
        {
            if (enemy.IsBlocking)
            {
                Attack(1);
                isPeaceful = true;
                StartCoroutine(OffensiveGracePeriod());
                return;
            }

            int attack = UnityEngine.Random.Range(0, 2);
            if (attack == 1) attack = 2;
            Attack(attack);

            isPeaceful = true;
            StartCoroutine(OffensiveGracePeriod());
        }
    }

    protected override float GetMovement()
    {
        float distanceToPlayer = GetDistanceToPlayer();

        if (distanceToPlayer <= attackDistance) return 0f;

        // Calculate direction to player more reliably
        float directionToPlayer = enemy.transform.position.x - transform.position.x;

        // Return normalized movement direction
        if (Mathf.Abs(directionToPlayer) > 0.1f)
        {
            return Mathf.Sign(directionToPlayer);
        }

        return 0f;
    }

    protected override void Damage(float damage)
    {
        base.Damage(damage);

        attackDetectedTime = -1f;
        blockAttemptedAlready = true;

        if (currentMode == AIMode.Defensive)
        {
            StopCoroutine(GracePeriod());
        }
        else
        {
            StopCoroutine(OffensiveGracePeriod());
        }

        isPeaceful = false;
    }

    public float GetDistanceToPlayer()
    {
        return Math.Abs(transform.position.x - enemy.transform.position.x);
    }

    private IEnumerator GracePeriod()
    {
        yield return new WaitForSeconds(attackDelay);
        isPeaceful = false;
    }

    private IEnumerator OffensiveGracePeriod()
    {
        yield return new WaitForSeconds(attackDelay * 0.5f);
        isPeaceful = false;
    }

    private IEnumerator BlockPeriod()
    {
        SetBlocking(true);
        yield return new WaitForSeconds(hitValues[enemy.state].cooldown + 0.25f);
        SetBlocking(false);
    }

    public override void ReviveAtPosition(Vector3 position)
    {
        // Call base revival functionality
        base.ReviveAtPosition(position);

        // Reset Enemy-specific AI states
        isPeaceful = false;
        attackDetectedTime = -1f;
        blockAttemptedAlready = false;
        currentMode = AIMode.Defensive;
    }
}