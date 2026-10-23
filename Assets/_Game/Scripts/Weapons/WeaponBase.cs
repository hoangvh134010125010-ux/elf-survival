using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Cấu Hình Vũ Khí")]
    public string weaponName;
    public float attackRange = 3f;
    public float attackCooldown = 1f;
    protected float lastAttackTime;

    public abstract void Attack(Transform target, int totalDamage);

    public virtual bool CanAttack()
    {
        float attackSpeed = 1f;
        if (PlayerStats.Instance != null)
        {
            attackSpeed = Mathf.Max(0.35f, PlayerStats.Instance.GetTotalAtkSpeed());
        }

        return Time.time >= lastAttackTime + (attackCooldown / attackSpeed);
    }
}
