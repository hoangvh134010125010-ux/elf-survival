using UnityEngine;

public class AutoWeapon : MonoBehaviour
{
    public static AutoWeapon Instance;

    [Header("Weapon References")]
    public WeaponBase currentWeapon;

    [Header("Weapon Holders")]
    public GameObject swordHolder;
    public GameObject bowHolder;
    public GameObject staffHolder;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        EquipWeaponByClass();
    }

    public void EquipWeaponByClass()
    {
        PlayerClass currentClass = PlayerStats.Instance != null
            ? PlayerStats.Instance.characterClass
            : PlayerClass.Warrior;

        if (swordHolder == null) swordHolder = FindChild("SwordHolder");
        if (bowHolder == null) bowHolder = FindChild("BowHolder");
        if (staffHolder == null) staffHolder = FindChild("StaffHolder");

        SetHolder(swordHolder, false);
        SetHolder(bowHolder, false);
        SetHolder(staffHolder, false);

        GameObject activeHolder = null;
        switch (currentClass)
        {
            case PlayerClass.Warrior: activeHolder = swordHolder; break;
            case PlayerClass.Archer: activeHolder = bowHolder; break;
            case PlayerClass.Mage: activeHolder = staffHolder; break;
        }

        if (activeHolder != null)
        {
            activeHolder.SetActive(true);
            currentWeapon = activeHolder.GetComponent<WeaponBase>();
        }
        else
        {
            currentWeapon = null;
        }

        if (PlayerAnimationController.Instance != null)
        {
            PlayerAnimationController.Instance.ApplyClassAnimation();
        }
    }

    private GameObject FindChild(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.gameObject : null;
    }

    private static void SetHolder(GameObject holder, bool active)
    {
        if (holder != null) holder.SetActive(active);
    }

    private void Update()
    {
        if (currentWeapon == null || !currentWeapon.CanAttack()) return;
        if (PlayerController.Instance != null && PlayerController.Instance.IsDead) return;

        Transform nearestEnemy = EnemyRegistry.FindNearest(transform.position, currentWeapon.attackRange);
        if (nearestEnemy == null) return;

        int damage = PlayerStats.Instance != null ? PlayerStats.Instance.GetTotalAtk() : 10;
        currentWeapon.Attack(nearestEnemy, damage);
    }
}
