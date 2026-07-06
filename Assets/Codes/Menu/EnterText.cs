using UnityEngine;
using TMPro;
using System.Collections;

public class EnterText : MonoBehaviour
{
    public TextMeshProUGUI textBox;
    public GameObject ip;

    private float longPressTime = 0.8f;
    private float repeatInterval = 0.1f;

    private Coroutine repeatCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // テキストを入力
    public void PutText()
    {
        textBox.text += this.name;
    }

    public void PushDelete()
    {
        repeatCoroutine = StartCoroutine(RepeatDelete());
    }
    
    public void ReleaseDelete()
    {
        if (repeatCoroutine != null) StopCoroutine(repeatCoroutine);
        repeatCoroutine = null;
    }

    private IEnumerator RepeatDelete()
    {
        Delete();

        yield return new WaitForSeconds(longPressTime);

        while (true)
        {
            Delete();
            yield return new WaitForSeconds(repeatInterval);
        }
    }

    private void Delete()
    {
        if (textBox.text.Length > 0)
        {
            textBox.text = textBox.text.Remove(textBox.text.Length - 1);
        }
    }

    


    public void Enter()
    {
        ip.name = textBox.text;
    }


}
