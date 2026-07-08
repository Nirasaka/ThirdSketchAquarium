using System.Collections;
using UnityEngine;

public class HideAfterSeconds : MonoBehaviour
{
    public float hideTime = 10f;

    public void Show()
    {
        this.gameObject.SetActive(true);
        StartCoroutine(StartTimer());
    }

    IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(hideTime);
        this.gameObject.SetActive(false);
    }
}
