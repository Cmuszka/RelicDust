using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadoutButton : MonoBehaviour
{
    public int abilityIndex;
    public ShipLoadoutDefinition loadoutDefinition;
    [SerializeField] private string sceneName = "GameScene";

    public void SelectAbility()
    {
        LoadoutData.selectedAbility = abilityIndex;
        LoadoutData.selectedLoadout = loadoutDefinition;
        SceneManager.LoadScene(sceneName);
    }
}
