using UnityEngine;

public class SwordWeapon : WeaponBase
{
    public override void Attack(Transform target, int totalDamage)
    {
        if (!CanAttack() || target == null) return;

        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy == null || !enemy.IsAlive) return;

        lastAttackTime = Time.time;

        if (PlayerAnimationController.Instance != null)
        {
            PlayerAnimationController.Instance.TriggerAttack();
        }

        enemy.TakeDamage(totalDamage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
