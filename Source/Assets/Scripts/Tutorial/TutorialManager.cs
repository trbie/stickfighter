using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public float progress = 0f;
    public int stage = 0;
    public Color completedColor = Color.green;
    public TutorialEnemy enemy;
    public GameObject[] healthbars;

    private InputSystemActions controller;
    private bool isLeaving = false;

    public static TutorialManager Instance { get; private set; }
    public static string[] tutorialMessages = {
        "Walk around with [<sprite name=\"Left\">] and [<sprite name=\"Right\">]",
        "Use Light Punch on the enemy with [<sprite name=\"Light\">]",
        "Kick your enemy with [<sprite name=\"Kick\">]",
        "Use Heavy Punch on the enemy with [<sprite name=\"Heavy\">]",
        "Block the enemy's attack with [<sprite name=\"Block\">] or [<sprite name=\"BlockAlt\">]",
        "Break the enemy's block with Heavy Punch [<sprite name=\"Heavy\">]",
        "You can swap out with your fighter double at any time using [<sprite name=\"Swap\">]",
        "Your Tag-Team is made up of two fighters. One is fast but weak, the other is slow but can take more damage",
        "The health of each fighter can be seen at the top of the screen. The green is health left, and the yellow is combo damage",
        "Those are the basics, defeat the enemy to return to the main menu or press [<sprite name=\"Menu\">] at any time",
    };

    void Awake()
    {
        Instance = this;
    }

    void OnDisable()
    {
        controller.Player.Disable();
        controller.Game.Disable();
    }

    void Start()
    {
        controller = new InputSystemActions();
        controller.Player.Enable();
        controller.Game.Enable();

        controller.Game.Pause.performed += ctx =>
        {
            SceneManager.LoadScene("Main Menu");
        };

        tutorialText.text = tutorialMessages[0];
        SetTaskProgress(0f);
    }

    void Update()
    {
        if (controller.Game.DEBUG.WasPressedThisFrame())
        {
            SetTaskProgress(1f);
        }

        if (progress >= 1f)
        {
            stage++;
            if (stage >= tutorialMessages.Length)
            {
                if (!isLeaving) StartCoroutine(LeaveTutorial());
                return;
            }

            tutorialText.text = tutorialMessages[stage];
            SetTaskProgress(0f);

            if (stage == 8)
            {
                foreach (GameObject healthbar in healthbars)
                {
                    healthbar.SetActive(true);
                }
            }
        }

        if (stage == 0)
        {
            float movement = controller.Player.Move.ReadValue<Vector2>().x * Time.deltaTime;
            if (movement != 0)
            {
                SetTaskProgress(progress + Mathf.Abs(movement) / 1.5f);
            }
        }
        else if (stage == 6)
        {
            if (controller.Player.Swap.WasPressedThisFrame())
            {
                SetTaskProgress(1f);
            }
        }
        else if (stage == 7 || stage == 8)
        {
            SetTaskProgress(progress + Time.deltaTime / 10f);
        }
        else if (stage == 9)
        {
            if (enemy.state == Fighter.State.Dead)
            {
                SetTaskProgress(1f);
            }
        }
    }

    public void SetTaskProgress(float progress)
    {
        this.progress = progress;
        tutorialText.color = Color.Lerp(Color.white, completedColor, progress);
    }

    public void EnemyWasHit(Fighter.State hitType)
    {
        if (stage == 1 && hitType == Fighter.State.LightPunching)
        {
            SetTaskProgress(progress + 0.35f);
        }
        else if (stage == 2 && hitType == Fighter.State.Kicking)
        {
            SetTaskProgress(progress + 0.5f);
        }
        else if (stage == 3 && hitType == Fighter.State.HeavyPunching)
        {
            SetTaskProgress(progress + 0.5f);
        }
        else if (stage == 5 && hitType == Fighter.State.HeavyPunching)
        {
            SetTaskProgress(1f);
        }
    }

    IEnumerator LeaveTutorial()
    {
        isLeaving = true;
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Main Menu");
    }
}
