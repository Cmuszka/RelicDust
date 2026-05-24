using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadoutButton : MonoBehaviour
{
    public int abilityIndex;
    public ShipLoadoutDefinition loadoutDefinition;

    public void SelectAbility()
    {
        LoadoutData.selectedAbility = abilityIndex;
        LoadoutData.selectedLoadout = loadoutDefinition;
        SceneManager.LoadScene("GameScene");
    }
}
