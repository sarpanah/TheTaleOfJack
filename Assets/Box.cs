using UnityEngine;

public class Box : MonoBehaviour
{
    
    private int destroyCounter = 0;

    void Update()
    {
        Debug.Log(destroyCounter);
    }

    public void BreakBox()
    {
        Destroy(gameObject);
    }
}
