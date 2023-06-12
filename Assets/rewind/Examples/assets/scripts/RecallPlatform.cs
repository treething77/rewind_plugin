using UnityEngine;

public class RecallPlatform : MonoBehaviour {
    public Transform startPt;
    public Transform endPt;

    public float speed;

    public Vector3 move;
    
    private void Update() {
        //ping pong between the two points
        Vector3 newPos = Vector3.Lerp(startPt.position, endPt.position, (Mathf.Sin(Time.time * speed) + 1.0f) * 0.5f);

        move = newPos - transform.position;
        transform.position = newPos;
    }
}
