using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 DefualtPosition = new Vector3(0, 10, -15);
    public Vector3 offset;

    private Transform target;
    public float smoothTime = 4;

    private Vector3 velocity = Vector3.zero;

    public void Initialize(Transform target)
    {
        transform.position = DefualtPosition;
        this.target = target;
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPossiotion = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPossiotion, ref velocity, smoothTime);
    }
}
