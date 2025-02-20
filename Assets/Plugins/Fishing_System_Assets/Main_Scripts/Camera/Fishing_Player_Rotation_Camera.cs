﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fishing_Player_Rotation_Camera : MonoBehaviour
{
    public float YMin = -9.0f;
    public float YMax = 9.0f;

    public Transform lookAt;

    public Transform Player;

    public float distance = 10.0f;
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    public float sensivity = 4.0f;

    void LateUpdate()
    {
        if(Input.GetMouseButton(1))
        {
            currentX += Input.GetAxis("Mouse X") * sensivity * Time.deltaTime;
            currentY += Input.GetAxis("Mouse Y") * sensivity * Time.deltaTime;
        }

        currentY = Mathf.Clamp(currentY, YMin, YMax);

        Vector3 Direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        transform.position = lookAt.position + rotation * Direction;

        transform.LookAt(lookAt.position);
    }
}
