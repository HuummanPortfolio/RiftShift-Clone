using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class VCamMovementController : MonoBehaviour
{

    private Camera mainCam;
    private Renderer objRenderer;
    private Vector3 offset;
    [SerializeField] private Vector2 hitPositionDiff;
    [SerializeField] private Vector2 hitPosition;
    private float objectWidth = 0f;
    private float objectHeight = 0f;
    private bool isControlled = false;
    [SerializeField] private Transform vcamOther = null;
    [SerializeField] bool canMoveHorizontal;
    [SerializeField] bool canMoveVertical;
    [SerializeField] bool lockedFromHorizontal;
    [SerializeField] bool lockedFromVertical;

    #region MONO METHOD
    private void Awake()
    {
        Initialize();
    }

    private void OnMouseDown()
    {
        isControlled = true;
        offset = transform.position - mainCam.ScreenToWorldPoint(Input.mousePosition);
    }
    private void OnMouseDrag()
    {
        VcamMovement();
    }
    private void OnMouseUp()
    {
        isControlled = false;
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

    private void VcamMovement()
    {
        //move virtual cam
        Vector2 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;

        #region CLAMP WITH OTHERS VCAM
        //check if we collide with other vcam
        if (vcamOther)
        {
            //check hit point position to lock the movement
            hitPositionDiff = newPosition - (Vector2)vcamOther.position;
            if (hitPositionDiff.x <= -newPosition.x + 0.01f)// we move from left to right, so clamp the x to not move to right
            {
                if (newPosition.x > hitPosition.x)
                {
                    newPosition = new Vector2(hitPosition.x, newPosition.y);
                }
            }
            if (hitPositionDiff.x >= newPosition.x - 0.01f)// we move from left to right, so clamp the x to not move to right
            {
                if (newPosition.x < hitPosition.x)
                {
                    newPosition = new Vector2(hitPosition.x, newPosition.y);
                }
            }
            //else if (hitPositionDiff.x >= transform.localPosition.x - 0.01f)// we move from right to left, so clamp the x to not move to left
            //{
            //    if (transform.position.x < hitPosition.x)
            //    {
            //        transform.position = new Vector2(hitPosition.x, transform.position.y);
            //    }
            //}
        }
        #endregion

        transform.position = new Vector2((canMoveHorizontal) ? newPosition.x : transform.position.x
            , (canMoveVertical) ? newPosition.y : transform.position.y);
        //clamp with camera device
        CheckPlanePositionToClamp();
    }

    /// <summary>
    /// Use to clamp plane position to not going outside of the camera while we playing, USED on PlayerMovement Method
    /// </summary>
    private void CheckPlanePositionToClamp()
    {
        if (mainCam == null || objRenderer == null)
            return;

        #region CLAMP WITH CAMERA
        float minX = mainCam.ViewportToWorldPoint(new Vector2(0, 0)).x + objectWidth;
        float maxX = mainCam.ViewportToWorldPoint(new Vector2(1, 0)).x - objectWidth;

        float minY = mainCam.ViewportToWorldPoint(new Vector2(0, 0)).y + objectHeight;
        float maxY = mainCam.ViewportToWorldPoint(new Vector2(0, 1)).y - objectHeight;

        Vector2 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(transform.position.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = clampedPosition;
        #endregion
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isControlled) return;
        vcamOther = collision.transform;
        hitPositionDiff = transform.position - vcamOther.position;
        hitPosition = transform.position;
        print($"Our pos : {transform.position}");
        print($"Other pos : {vcamOther.position}");

        print($"Selisih vector = {hitPositionDiff}");
        print($"Selisih vector position dengan hitpositiondiff : {(Vector2)transform.position - hitPositionDiff}");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isControlled) return;
        vcamOther = null;
        hitPositionDiff = Vector2.zero;
    }
}
