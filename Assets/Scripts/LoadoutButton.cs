using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadoutButton : MonoBehaviour
{
    public int abilityIndex;

    public void SelectAbility()
    {
        LoadoutData.selectedAbility = abilityIndex;
        SceneManager.LoadScene("GameScene");
    }
}