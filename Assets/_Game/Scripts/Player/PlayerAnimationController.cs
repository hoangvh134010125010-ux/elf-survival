using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public static PlayerAnimationController Instance;

    [Header("Animator Main")]
    public Animator animator;
    public RuntimeAnimatorController baseController;

    [Header("Animation Clips - WARRIOR")]
    public AnimationClip warIdle;
    public AnimationClip warRun;
    public AnimationClip warAttack;
    public AnimationClip warDie;

    [Header("Animation Clips - MAGE")]
    public AnimationClip magiIdle;
    public AnimationClip magiRun;
    public AnimationClip magiAttack;
    public AnimationClip magiDie;

    [Header("Animation Clips - ARCHER")]
    public AnimationClip archerIdle;
    public AnimationClip archerRun;
    public AnimationClip archerAttack;
    public AnimationClip archerDie;

    private AnimatorOverrideController overrideController;
    private readonly System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>> overrideBuffer
        = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>(8);
    private bool lastMoving;

    private void Awake()
    {
        // Luôn cập nhật Instance mới nhất theo Player hiện tại
        Instance = this;
        
        if (animator == null) 
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Start()
    {
        ApplyClassAnimation();
    }

    public void ApplyClassAnimation()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null || baseController == null) return;

        overrideController = new AnimatorOverrideController(baseController);
        animator.runtimeAnimatorController = overrideController;
        lastMoving = false;

        PlayerClass currentClass = PlayerClass.Warrior;
        if (PlayerStats.Instance != null)
        {
            currentClass = PlayerStats.Instance.characterClass;
        }

        switch (currentClass)
        {
            case PlayerClass.Warrior:
                SetOverrideClips(warIdle, warRun, warAttack, warDie);
                break;

            case PlayerClass.Mage:
                SetOverrideClips(magiIdle, magiRun, magiAttack, magiDie);
                break;

            case PlayerClass.Archer:
                SetOverrideClips(archerIdle, archerRun, archerAttack, archerDie);
                break;
        }
    }

    private void SetOverrideClips(AnimationClip idle, AnimationClip run, AnimationClip attack, AnimationClip die)
    {
        if (overrideController == null) return;

        overrideBuffer.Clear();
        overrideController.GetOverrides(overrideBuffer);

        for (int i = 0; i < overrideBuffer.Count; i++)
        {
            string clipName = overrideBuffer[i].Key != null ? overrideBuffer[i].Key.name.ToLowerInvariant() : "";

            if (clipName.Contains("idle")) overrideBuffer[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(overrideBuffer[i].Key, idle);
            else if (clipName.Contains("run") || clipName.Contains("move")) overrideBuffer[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(overrideBuffer[i].Key, run);
            else if (clipName.Contains("att") || clipName.Contains("attack")) overrideBuffer[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(overrideBuffer[i].Key, attack);
            else if (clipName.Contains("die")) overrideBuffer[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(overrideBuffer[i].Key, die);
        }

        overrideController.ApplyOverrides(overrideBuffer);
    }

    public void SetMoving(bool isMoving)
    {
        if (animator == null || lastMoving == isMoving) return;
        lastMoving = isMoving;
        animator.SetBool("isMoving", isMoving);
    }

    public void TriggerAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void TriggerDie()
    {
        if (animator != null)
        {
            animator.SetBool("isDead", true);
            animator.SetTrigger("Die");
        }
    }
}