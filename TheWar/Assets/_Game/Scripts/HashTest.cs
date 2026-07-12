using UnityEngine;

public class HashTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Base_Idle: " + Animator.StringToHash("Base_Idle"));
        Debug.Log("Base_Run: " + Animator.StringToHash("Base_Run"));
        Debug.Log("Base_Attack: " + Animator.StringToHash("Base_Attack"));
        Debug.Log("Base_Die: " + Animator.StringToHash("Base_Die"));
    }
}
