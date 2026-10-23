using UnityEngine;

public class BowWeapon : WeaponBase
{
    [Header("Cấu Hình Cung / Bắn Tầm Xa")]
    public GameObject arrowPrefab;
    public Transform firePoint;

    public override void Attack(Transform target, int totalDamage)
    {
        if (!CanAttack() || target == null || arrowPrefab == null) return;
        lastAttackTime = Time.time;

        if (PlayerAnimationController.Instance != null)
        {
            PlayerAnimationController.Instance.TriggerAttack();
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Projectile proj = SimpleObjectPool.Spawn<Projectile>(arrowPrefab, spawnPos, Quaternion.identity);
        if (proj != null)
        {
            proj.Setup(target, totalDamage);
        }
    }
}
