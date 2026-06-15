using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimController : MonoBehaviour
{
    private Animator anim;

    // Animator parameter names — เก็บเป็น string ไว้ที่เดียว
    private static readonly int HashSpeed    = Animator.StringToHash("Speed");
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashAttack   = Animator.StringToHash("Attack");
    private static readonly int HashDodge    = Animator.StringToHash("Dodge");

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetMoving(bool moving, float speed)
    {
        anim.SetBool(HashIsMoving, moving);
        anim.SetFloat(HashSpeed, speed);
    }

    public void TriggerAttack(int comboStep)
    {
        anim.SetInteger("ComboStep", comboStep);
        anim.SetTrigger(HashAttack);
    }

    public void TriggerDodge()
    {
        anim.SetTrigger(HashDodge);
    }
}