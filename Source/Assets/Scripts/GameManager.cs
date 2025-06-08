using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI waveText;

    private bool isGameOver = false;
    private List<Fighter> fighters = new List<Fighter>();
    private InputSystemActions controller;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        controller = new InputSystemActions();
        controller.Game.Enable();
        controller.UI.Enable(); // Enable UI input for button interactions

        controller.Game.Reset.performed += ctx =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        };

        controller.Game.Pause.performed += ctx =>
        {
            SceneManager.LoadScene("Main Menu");
        };
    }

    private void OnDisable()
    {
        controller.Game.Disable();
        controller.UI.Disable(); // Disable UI input to prevent memory leaks
    }

    private void Start()
    {
        fighters.AddRange(FindObjectsByType<Fighter>(FindObjectsSortMode.None));

        if (GameMode.Current == GameMode.Mode.Onslaught)
        {
            waveText.gameObject.SetActive(true);
            waveText.text = "1/3";
        }
        else
        {
            waveText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        foreach (Fighter fighter in fighters)
        {
            if (fighter.state == Fighter.State.Dead)
            {
                if (GameMode.Current == GameMode.Mode.Onslaught && fighter is Enemy)
                {
                    continue;
                }
                winnerText.text = $"{fighter.enemy.name} wins!";
                winnerText.transform.parent.gameObject.SetActive(true);
                isGameOver = true;
                break;
            }
        }
    }
}