using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class Clock : MonoBehaviour
{
    private float elapsedTime = 0.0f;

    private void Update()
    {
        this.elapsedTime += Time.deltaTime;
        int totalSec = (int)Mathf.Floor(this.elapsedTime);
        int min = (int)Mathf.Floor(totalSec / 60);
        int sec = totalSec % 60;
        Text text = this.GetComponent<Text>();
        text.text = ("" + min).PadLeft(2, '0') + ":" + ("" + sec).PadLeft(2, '0');
    }
}
