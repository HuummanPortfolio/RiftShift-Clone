using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VCamMovementController : MonoBehaviour
{

    private Camera mainCam;
    private Renderer objRenderer;
    private Vector3 offset;
    private float objectWidth = 0f;
    private float objectHeight = 0f;
    [SerializeField] private Transform vcamOther = null;
    #region MONO METHOD
    private void Awake()
    {
        Initialize();
    }

    private void OnMouseDown()
    {
        offset = transform.position - mainCam.ScreenToWorldPoint(Input.mousePosition);
    }
    private void OnMouseDrag()
    {
        //move virtual cam
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);

        //clamp
        CheckPlanePositionToClamp();
    }

    #endregion

    /// <summary>
    /// Initialize something
    /// </summary>
    private void Initialize()
    {
        mainCam = Camera.main;
        objRenderer = GetComponent<Renderer>();

        if (objRenderer != null)
        {
            objectWidth = objRenderer.bounds.extents.x;
            objectHeight = objRenderer.bounds.extents.y;
        }
    }

    /// <summary>
    /// Use to clamp plane position to not going outside of the camera while we playing, USED on PlayerMovement Method
    /// </summary>
    void CheckPlanePositionToClamp()
    {
        if (mainCam == null || objRenderer == null)
            return;

        #region CLAMP WITH CAMERA
        float minX = mainCam.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + objectWidth;
        float maxX = mainCam.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - objectWidth;

        float minY = mainCam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + objectHeight;
        float maxY = mainCam.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - objectHeight;

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(transform.position.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = clampedPosition;
        #endregion

        #region CLAMP WITH OTHERS VCAM

        #endregion
    }

    void CheckOverlapsWithAnotherVCam()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        vcamOther = collision.transform;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        vcamOther = null;
    }
}
