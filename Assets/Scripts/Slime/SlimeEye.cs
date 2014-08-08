using UnityEngine;
using System.Collections;

public class SlimeEye : MonoBehaviour {

    void Update() {
        movePupil();
    }

    public void movePupil() {
        Vector3 mouseGlobal = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseLocal = transform.parent.InverseTransformPoint(mouseGlobal);
        transform.localPosition = mouseLocal / 1.5f;
        //finds distance from local position to the origin
        float distance = Vector3.Distance(transform.localPosition, Vector3.zero);
        Vector3 direction = transform.localPosition / distance;
        distance = Mathf.Min(distance, .135f);
        transform.localPosition = direction * distance;
    }
}
