using UnityEngine;

public class PlayerAbilitySystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
        {
            int selected = LoadoutData.selectedAbility;

            GetComponent<PlayerShieldAbility>().enabled = false;
            GetComponent<PlayerBurstAbility>().enabled = false;

            if (selected == 0)
            {
                GetComponent<PlayerShieldAbility>().enabled = true;
            }
            else if (selected == 1)
            {
                GetComponent<PlayerBurstAbility>().enabled = true;
            }
        }
}
