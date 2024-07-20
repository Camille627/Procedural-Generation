using UnityEngine;

public class LevelController : MonoBehaviour
{
    LevelManager levelManager;
    float lastActivationTime = 0f;

    private void Awake()
    {
        levelManager = GetComponent<LevelManager>();
    }
    
    private void Update()
    {
        CheckCheatCodes();
    }

    private void CheckCheatCodes()
    {
        if (Time.time < lastActivationTime + OptionsManager.cooldown || !Input.GetKey(OptionsManager.cheatKey)) { return; }
        // Régénérer le niveau
        if (IsCheatCodeActivated(OptionsManager.regenLevelCode))
        {
            StartCoroutine(levelManager.EndLevel());
            lastActivationTime = Time.time;
        }
    }

    private bool IsCheatCodeActivated(KeyCode[] cheatCode)
    {
        foreach (KeyCode key in cheatCode)
        {
            if (!Input.GetKey(key))
            {
                return false;
            }
        }
        return true;
    }
}
