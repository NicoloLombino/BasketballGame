using UnityEngine;

public class DestroyThis : MonoBehaviour
{
    [SerializeField]
    private float lifeTime;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
