using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecallScanningUI : MonoBehaviour {
    public List<Image> frameH;
    public List<Image> frameV;
    public Text text;
    public Text targetText;

    public void SetScanningState(bool enable, RecallPlatform highlighted, bool rewinding) {
        this.gameObject.SetActive(enable);
        
        if (rewinding)
            targetText.text = "RECALLING";
        else {
            if (highlighted != null)
                targetText.text = "TARGET FOUND";
            else {
                targetText.text = "";
            }
        }
    }

    private float scanningT = 0.0f;
    
    void Update() {
        scanningT += Time.deltaTime;
        if (scanningT > 2.0f) scanningT = 0.0f;
        float animT = Mathf.PingPong(scanningT, 1.0f);
        
        foreach(var frame in frameH)
            frame.rectTransform.sizeDelta = new Vector2(100 + 200 * animT, 10);
        foreach(var frame in frameV)
            frame.rectTransform.sizeDelta = new Vector2(10, 100 + 200 * animT);

        if (animT < 0.2f)      text.text = "Scanning";
        else if (animT < 0.4f) text.text = "Scanning .";
        else if (animT < 0.6f) text.text = "Scanning ..";
        else                   text.text = "Scanning ...";

    }
}
