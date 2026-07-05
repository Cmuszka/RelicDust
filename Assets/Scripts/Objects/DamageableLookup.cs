using UnityEngine;

public static class DamageableLookup
{
    public static IDamageable FindInParent(Collider2D collider)
    {
        if (collider == null)
        {
            return null;
        }

        MonoBehaviour[] behaviours = collider.GetComponentsInParent<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IDamageable damageable)
            {
                return damageable;
            }
        }

        return null;
    }
}
