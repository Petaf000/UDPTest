using UnityEngine;

public class Hide : MonoBehaviour
{
    [SerializeField] private GameObject targetObject1;
    [SerializeField] private GameObject targetObject2;

    public void HideObjects()
    {
        if (targetObject1 != null)
        {
            targetObject1.SetActive(false);
        }
        if (targetObject2 != null)
        {
            targetObject2.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}
