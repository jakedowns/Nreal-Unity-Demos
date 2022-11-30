using NRKernal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetModelManipulator : MonoBehaviour
{
    /// <summary> The model target. </summary>
    //public Transform modelTarget;
    /// <summary> The model renderer. </summary>
    //public MeshRenderer modelRenderer;
    /// <summary> The minimum scale. </summary>
    private float minScale = 0.1f;
    /// <summary> The maximum scale. </summary>
    private float maxScale = 10f;

    private float currentScale = 1f;

    private float currentX = 0f;
    private float currentY = 0f;
    private float currentZ = 0f;

    private float minX = -2f;
    private float maxX = 2f;

    private float minY = -2f;
    private float maxY = 2f;

    /// <summary> The NRInput. </summary>
    [SerializeField]
    private NRInput m_NRInput;

    /// <summary> The touch scroll speed. </summary>
    private float m_TouchScrollSpeed = 10f;
    /// <summary> The previous position. </summary>
    private Vector2 m_PreviousPos;
    
    // Start is called before the first frame update
    void Start()
    {
        currentScale = transform.localScale.x;
        currentX = transform.position.x;
        currentY = transform.position.y;
        currentZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (NRInput.GetButtonDown(ControllerButton.TRIGGER))
        {
            m_PreviousPos = NRInput.GetTouch();
        }
        else if (NRInput.GetButton(ControllerButton.TRIGGER))
        {
            UpdateScroll();
        }
        else if (NRInput.GetButtonUp(ControllerButton.TRIGGER))
        {
            m_PreviousPos = Vector2.zero;
        }
    }

    /// <summary> Updates the scroll. </summary>
    private void UpdateScroll()
    {
        if (m_PreviousPos == Vector2.zero)
            return;
        
        Vector2 deltaMove = NRInput.GetTouch() - m_PreviousPos;
        m_PreviousPos = NRInput.GetTouch();
        //modelTarget.Rotate(m_AroundLocalAxis, deltaMove.x * m_TouchScrollSpeed * Time.deltaTime, Space.Self);

        gameObject.transform.Translate(
            0,
            0,
            deltaMove.y * m_TouchScrollSpeed
        );

        currentScale += deltaMove.x * 10f;

        // clamp scale
        if (currentScale < minScale)
        {
            currentScale = minScale;
        }
        else if (currentScale > maxScale)
        {
            currentScale = maxScale;
        }

        Debug.Log(deltaMove.x + ", " + currentScale);

        transform.localScale = Vector3.one * currentScale; // Vector3.one * Mathf.SmoothStep(minScale, maxScale, currentScale);
    }
}
