using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class androidCam : MonoBehaviour
{
    [SerializeField] float rotateSenstivity;
    [SerializeField] FixedTouchField touchField;

    float xRotationMin = 25f, xRotationMax = 90f;
    float zoomMin = 1.2f, zoomMax = 10f;

    float x_axis, y_axis;
    float zoomAmount;

    Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        y_axis += touchField.TouchDist.x * rotateSenstivity;
        x_axis -= touchField.TouchDist.y * rotateSenstivity;

        x_axis = Mathf.Clamp(x_axis, xRotationMin, xRotationMax);


        Vector3 targetRotation = new Vector3(x_axis, y_axis);
        transform.eulerAngles = targetRotation;

        zoomAmount = Mathf.Clamp(0.2f * x_axis, zoomMin, zoomMax);
        transform.position = player.position - transform.forward * zoomAmount;

    }
}
