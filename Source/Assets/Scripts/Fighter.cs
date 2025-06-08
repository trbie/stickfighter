using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Fighter : MonoBehaviour
{
    protected Dictionary<State, (float damage, float stun, float knockback, float cooldown)> hitValues = new Dictionary<State, (float damage, float stun, float knockback, float cooldown)>
    {
        { State.LightPunching, (5f, 0.5f, 1.5f, 0.25f) },
        { State.HeavyPunching, (18f, 1.32f, 3.5f, 0.833f) },
        { State.Kicking, (8f, 0.6f, 1.8f, 0.417f) }
    };

    public enum State
    {
        Idle,
        Walking,
        Jumping,
        LightPunching,
        HeavyPunching,
        Kicking,
        Stunned,
        Dead
    }

    public State state { get; protected set; } = State.Idle;

    public float health = 100f;
    public float speed = 1.5f;
    public float jumpSpeed = 4f;
    public Healthbar healthbar;
    public Fighter enemy;

    public float maxHealth { get; private set; } = 100f;

    protected bool onGround = true;
    protected bool shouldJump = false;
    protected bool isInvincible = false;
    protected Animator animator;
    protected Rigidbody2D rb;
    protected GameObject attackHitbox;
    protected GameObject blockingShield;

    // Blocking
    protected bool isBlocking = false;

    public bool IsBlocking => isBlocking;

    protected SpriteRenderer spriteRenderer;

    private static readonly Dictionary<string, string[]> AUDIO_PATHS = new Dictionary<string, string[]>()
    {
        { "LightHit", new string[] { "Audio/SFX/LightHit", "Audio/SFX/LightHit2", "Audio/SFX/LightHit3" }},
        { "HeavyHit", new string[] { "Audio/SFX/HeavyHit", "Audio/SFX/HeavyHit2", "Audio/SFX/HeavyHit3" }},
        { "Kick", new string[] { "Audio/SFX/Kick", "Audio/SFX/Kick2", "Audio/SFX/Kick3" }},
        { "Miss", new string[] { "Audio/SFX/Miss" }},
        { "Block", new string[] { "Audio/SFX/Block" }},
        { "Swap", new string[] { "Audio/SFX/Swap" }},
    };

    private static Dictionary<string, AudioClip[]> audioClips = new Dictionary<string, AudioClip[]>();


    void Awake()
    {
        if (audioClips.Count == 0)
        {
            foreach (var entry in AUDIO_PATHS)
            {
                audioClips[entry.Key] = new AudioClip[entry.Value.Length];
                for (int i = 0; i < entry.Value.Length; i++)
                {
                    audioClips[entry.Key][i] = Resources.Load<AudioClip>(entry.Value[i]);
                }
            }
        }
    }

    protected void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        attackHitbox = transform.Find("AttackHitbox").gameObject;
        blockingShield = transform.Find("Shield").gameObject;

        spriteRenderer = GetComponent<SpriteRenderer>();

        animator.Play("Idle");
        attackHitbox.SetActive(false);
        blockingShield.SetActive(false);

        SetMaxHealth(health);
        if (healthbar)
        {
            healthbar.SetHealth(health);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (state == State.Dead) return;

        float horizontalInput = GetMovement();

        if (horizontalInput != 0 && (state == State.Idle || state == State.Walking))
        {
            if (state == State.Idle)
            {
                state = State.Walking;
                animator.Play("Walk");
            }

            animator.SetFloat("Direction", horizontalInput * transform.localScale.x);
        }
        else if (state == State.Walking)
        {
            state = State.Idle;
            animator.Play("Idle");
        }

        if (shouldJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpSpeed);
            shouldJump = false;
            onGround = false;
        }

        if (state == State.Jumping || state == State.Idle || state == State.Walking)
        {
            Vector2 movement = new Vector2(horizontalInput * speed, rb.linearVelocityY);
            rb.linearVelocity = movement;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = false;
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Fist") && collision.gameObject != attackHitbox)
        {
            if (!enemy.IsHitting() || isInvincible) return;

            // Blocking
            if (isBlocking && (enemy.state == State.LightPunching || enemy.state == State.Kicking))
            {
                AudioClip blockClip = GetRandomAudioClip("Block");
                if (blockClip)
                {
                    AudioSource.PlayClipAtPoint(blockClip, transform.position);
                }
                return;
            }

            var hit = enemy.hitValues[enemy.state];

            AudioClip audioClip = null;

            switch (enemy.state)
            {
                case State.LightPunching:
                    audioClip = GetRandomAudioClip("LightHit");
                    break;
                case State.HeavyPunching:
                    audioClip = GetRandomAudioClip("HeavyHit");
                    break;
                case State.Kicking:
                    audioClip = GetRandomAudioClip("Kick");
                    break;
            }

            if (audioClip)
                AudioSource.PlayClipAtPoint(audioClip, transform.position);

            Damage(hit.damage);
            StartCoroutine(Stun(hit.stun));

            Vector2 movement = new Vector2(-transform.localScale.x * hit.knockback, rb.linearVelocityY);
            rb.linearVelocity = movement;
        }
    }

    protected abstract float GetMovement();

    public bool CanAttack()
    {
        if (isBlocking)
        {
            return false;
        }
        return state == State.Idle || state == State.Walking || state == State.Jumping;
    }

    public bool IsHitting()
    {
        return state == State.LightPunching || state == State.HeavyPunching || state == State.Kicking;
    }

    protected virtual void Damage(float damage)
    {
        health -= damage;
        if (healthbar && healthbar.gameObject.activeSelf) healthbar.TakeDamage(damage);

        if (health <= 0f)
        {
            Kill();
        }
    }

    protected void Attack(int attackType)
    {
        if (!CanAttack()) return;

        switch (attackType)
        {
            case 0:
                animator.Play("LightAttack");
                state = State.LightPunching;
                break;

            case 1:
                animator.Play("HeavyAttack");
                state = State.HeavyPunching;
                break;

            case 2:
                animator.Play("Kick");
                state = State.Kicking;
                break;
        }

        attackHitbox.SetActive(true);
        StartCoroutine(ActionCooldown(hitValues[state].cooldown));
    }

    protected virtual void Kill()
    {
        StopAllCoroutines();
        state = State.Dead;
        animator.Play("Death");
        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = false;
        attackHitbox.SetActive(false);
    }

    public void SetMaxHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
        if (healthbar) healthbar.SetMaxHealth(maxHealth);
    }

    private IEnumerator Stun(float duration)
    {
        if (state == State.Dead) yield break;

        state = State.Stunned;
        animator.Play("Hurt");

        yield return new WaitForSeconds(duration);

        animator.Play("Idle");
        state = State.Idle;
    }

    protected IEnumerator ActionCooldown(float duration = 0f)
    {
        if (duration == 0f) duration = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(duration);

        if (IsHitting())
        {
            attackHitbox.SetActive(false);
        }

        state = State.Idle;
    }

    public void SetBlocking(bool blocking)
    {
        if (isBlocking == blocking) return;
        isBlocking = blocking;
        blockingShield.SetActive(blocking);
    }

    public virtual void ReviveAtPosition(Vector3 position)
    {
        // Reset position
        transform.position = position;

        // Reset fighter state
        state = State.Idle;
        animator.Play("Idle");
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;

        // Reset invincibility
        isInvincible = false;

        // Reset blocking state to prevent permanent blocking bug
        SetBlocking(false);

        // Stop any remaining coroutines that might interfere
        StopAllCoroutines();
    }

    protected AudioClip GetRandomAudioClip(string clipName)
    {
        if (audioClips.ContainsKey(clipName))
        {
            AudioClip[] clips = audioClips[clipName];
            if (clips.Length > 0)
            {
                return clips[Random.Range(0, clips.Length)];
            }
        }

        return null;
    }
}
