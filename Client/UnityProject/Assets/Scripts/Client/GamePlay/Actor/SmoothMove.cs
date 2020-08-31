﻿using UnityEngine;
using System.Collections;

public class SmoothMove : MonoBehaviour
{
    public float SmoothTime = 0.01f;

    private Vector3 DefaultLocalPosition;

    void Awake()
    {
        DefaultLocalPosition = transform.localPosition;
    }

    void Start()
    {
    }

    private Vector3 LastPosition;
    private Vector3 CurSpeed;

    void OnEnable()
    {
        LastPosition = transform.position;
        CurSpeed = Vector3.zero;
    }

    void LateUpdate()
    {
        if (enabled)
        {
            transform.position = Vector3.SmoothDamp(LastPosition, transform.parent.position + DefaultLocalPosition, ref CurSpeed, SmoothTime, 999f);
            LastPosition = transform.position;
        }
    }
}