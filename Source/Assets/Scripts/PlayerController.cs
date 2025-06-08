using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Fighter
{
    public Color speedyColor = new Color(0.5f, 1f, 0.5f);
    public Color tankyColor = new Color(0.5f, 0.5f, 1f);
    public SlideMovement swapper;

    private readonly Dictionary<State, (float damage, float stun, float knockback, float cooldown)> speedyHitValues = new Dictionary<State, (float damage, float stun, float knockback, float cooldown)>
    {
        { State.LightPunching, (4f, 0.4f, 0.5f, 0.15f) },
        { State.HeavyPunching, (14f, 1.0f, 2f, 0.833f) },
        { State.Kicking, (7f, 0.5f, 1f, 0.417f) }
    };

    private readonly Dictionary<State, (float damage, float stun, float knockback, float cooldown)> tankyHitValues = new Dictionary<State, (float damage, float stun, float knockback, float cooldown)>
    {
        { State.LightPunching, (6.5f, 0.6f, 1.5f, 0.3f) },
        { State.HeavyPunching, (22.5f, 1.45f, 4.5f, 1f) },
        { State.Kicking, (10f, 0.6f, 2.0f, 0.5f) }
    };

    public enum PlayerType
    {
        Speedy,
        Tanky
    }

    public PlayerType playerType { get; private set; } = PlayerType.Speedy;
    private float otherHealth = 200f;
    private float otherMaxHealth = 200f;
    public Healthbar otherHealthbar;
    private float otherSpeed = 1.0f;
    private float otherJumpSpeed = 2.0f;
    private bool isSwapping = false;

    private InputSystemActions controller;

    private void Awake()
    {
        controller = new InputSystemActions();
        controller.Player.Enable();
    }

    private new void Start()
    {
        base.Start();

        spriteRenderer.color = speedyColor;

        healthbar.SetHealth(health);
        healthbar.SetMaxHealth(maxHealth);

        otherHealthbar.SetHealth(otherHealth);
        otherHealthbar.SetMaxHealth(otherMaxHealth);
    }

    private void OnDisable()
    {
        controller.Player.Disable();
    }

    private void Update()
    {
        if (state == State.Dead) return;

        // Blocking
        if (controller.Player.Block.WasPressedThisFrame())
        {
            SetBlocking(true);
        }
        if (controller.Player.Block.WasReleasedThisFrame())
        {
            SetBlocking(false);
        }

        // if (state == State.Walking || state == State.Idle)
        // {
        //     if (onGround && controller.Player.Jump.triggered)
        //     {
        //         state = State.Jumping;
        //         animator.Play("Jump");
        //         shouldJump = true;

        //         StartCoroutine(ActionCooldown());
        //     }
        // }

        if (controller.Player.LightAttack.triggered)
        {
            Attack(0);
        }
        else if (controller.Player.HeavyAttack.triggered)
        {
            Attack(1);
        }
        else if (controller.Player.KickAttack.triggered)
        {
            Attack(2);
        }

        if (!isSwapping && controller.Player.Swap.triggered)
        {
            SwapPlayers();
        }
    }

    protected override float GetMovement()
    {
        float horizontalInput = controller.Player.Move.ReadValue<Vector2>().x;
        horizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);

        float baseSpeed = playerType == PlayerType.Speedy ? 3.5f : 1.0f;

        float finalSpeed = (isBlocking && playerType == PlayerType.Speedy) ? baseSpeed * 0.5f : baseSpeed;

        speed = finalSpeed;

        return horizontalInput;
    }

    protected new void Kill()
    {
        base.Kill();
        swapper.CancelSlide();
    }

    private void SwapPlayers()
    {
        isSwapping = true;
        swapper.StartSlide(playerType == PlayerType.Speedy ? tankyColor : speedyColor, spriteRenderer.color);
        AudioSource.PlayClipAtPoint(GetRandomAudioClip("Swap"), swapper.transform.position);
    }

    public void SwapStats()
    {
        if (state == State.Dead) return;

        animator.Play("Slide");
        playerType = playerType == PlayerType.Speedy ? PlayerType.Tanky : PlayerType.Speedy;

        if (enemy is Enemy)
        {
            enemy.transform.gameObject.GetComponent<Enemy>().SwitchMoveset(playerType);
        }

        (otherHealth, health) = (health, otherHealth);

        var tmp = otherMaxHealth;
        otherMaxHealth = maxHealth;
        SetMaxHealth(tmp);

        healthbar.SetHealth(health);
        healthbar.SetMaxHealth(maxHealth);

        otherHealthbar.SetHealth(otherHealth);
        otherHealthbar.SetMaxHealth(otherMaxHealth);

        (otherSpeed, speed) = (speed, otherSpeed);
        (otherJumpSpeed, jumpSpeed) = (jumpSpeed, otherJumpSpeed);

        spriteRenderer.color = playerType == PlayerType.Speedy ? speedyColor : tankyColor;
        hitValues = playerType == PlayerType.Speedy ? speedyHitValues : tankyHitValues;

        StartCoroutine(Swap());
    }

    private IEnumerator Swap()
    {
        yield return StartCoroutine(ActionCooldown());
        isSwapping = false;
    }
}