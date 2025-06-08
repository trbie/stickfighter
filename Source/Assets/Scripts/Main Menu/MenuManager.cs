using UnityEngine;
using UnityEngine.AI;

public class MenuManager : MonoBehaviour
{
    private Fighter[] fighters;
    private InputSystemActions controller;

    private void Awake()
    {
        controller = new InputSystemActions();
    }

    private void Start()
    {
        fighters = FindObjectsByType<Fighter>(FindObjectsSortMode.None);
    }

    private void Update()
    {
        foreach (Fighter fighter in fighters)
        {
            float normalizedPosition = (fighter.transform.position.x + 10f) / 20f;
            float positionHealth = normalizedPosition * fighter.maxHealth;

            if (fighter.transform.localScale.x < 0)
            {
                positionHealth = fighter.maxHealth - positionHealth;
            }

            fighter.health = positionHealth;
        }
    }
}