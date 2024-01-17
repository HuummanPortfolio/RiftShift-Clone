using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class VCamMovementController : MonoBehaviour
{

    [SerializeField] private Camera mainCam;
    [SerializeField] private Renderer objRenderer;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector2 hitPositionDiff;
    [SerializeField] private Vector2 hitPosition;
    [SerializeField] private float objectWidth = 0f;
    [SerializeField] private float objectHeight = 0f;
    [SerializeField] private bool isControlled = false;
    [SerializeField] private Transform vcamOther = null;
    [SerializeField] bool canMoveHorizontal;
    [SerializeField] bool canMoveVertical;
    [SerializeField] bool lockedFromHorizontal;
    [SerializeField] bool lockedFromVertical;
    [SerializeField] bool hitOthers = false;
    private Rigidbody2D rb2d;

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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isControlled) return;
        hitOthers = true;
        //vcamOther = collision.transform;
        hitPositionDiff = transform.position - vcamOther.position;
        hitPosition = transform.position;
        rb2d.velocity = Vector2.zero;
        transform.position = rb2d.position;
        //print($"Our pos : {transform.position}");
        //print($"Other pos : {vcamOther.position}");
        //print($"Selisih vector = {hitPositionDiff}");
        //print($"Selisih vector position dengan hitpositiondiff : {(Vector2)transform.position - hitPositionDiff}");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isControlled) return;
        hitOthers = false;
        //vcamOther = null;
        hitPositionDiff = Vector2.zero;
        hitPosition = Vector2.zero;
    }

    #endregion


    #region PRIVATE METHOD
    /// <summary>
    /// Initialize something
    /// </summary>
    private void Initialize()
    {
        mainCam = Camera.main;
        objRenderer = GetComponent<Renderer>();
        rb2d = GetComponent<Rigidbody2D>();
        if (objRenderer != null)
        {
            objectWidth = objRenderer.bounds.extents.x;
            objectHeight = objRenderer.bounds.extents.y;
        }
    }

    private void VcamMovement()
    {
        //var a = Physics2D.OverlapArea(vcam)

        //move virtual cam
        Vector2 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;

        //#region CLAMP WITH OTHERS VCAM
        ////check if we collide with other vcam
        //if (vcamOther)
        //{
        //    //check hit point position to lock the movement
        //    hitPositionDiff = newPosition - (Vector2)vcamOther.position;
        //    if (hitPositionDiff.x <= -newPosition.x + 0.01f)// we move from left to right, so clamp the x to not move to right
        //    {
        //        if (newPosition.x > hitPosition.x)
        //        {
        //            newPosition = new Vector2(hitPosition.x, newPosition.y);
        //        }
        //    }
        //    if (hitPositionDiff.x >= newPosition.x - 0.01f)// we move from left to right, so clamp the x to not move to right
        //    {
        //        if (newPosition.x < hitPosition.x)
        //        {
        //            newPosition = new Vector2(hitPosition.x, newPosition.y);
        //        }
        //    }
        //    if (hitPositionDiff.y <= -newPosition.y + 0.01f)
        //    {
        //        if (newPosition.y > hitPosition.y)
        //        {
        //            newPosition = new Vector2(newPosition.x, hitPosition.y);
        //        }
        //    }
        //    if (hitPositionDiff.y >= newPosition.y - 0.01f)
        //    {
        //        if (newPosition.y < hitPosition.y)
        //        {
        //            newPosition = new Vector2(newPosition.x, hitPosition.y);
        //        }
        //    }
        //}
        //#endregion

        //rb2d.MovePosition(new Vector2((canMoveHorizontal) ? newPosition.x : transform.position.x
        //, (canMoveVertical) ? newPosition.y : transform.position.y));

        Vector2 destination = new Vector2((canMoveHorizontal) ? newPosition.x : transform.position.x
                    , (canMoveVertical) ? newPosition.y : transform.position.y);

        if (isControlled && hitOthers)
        {

            float horizontalDiff = Mathf.Abs(vcamOther.position.x - hitPosition.x);
            float verticalDiff = Mathf.Abs(vcamOther.position.y - hitPosition.y);
            if ((horizontalDiff >= transform.localScale.x - 0.1f &&
                horizontalDiff <= transform.localScale.x + 0.1f))//lock horizontal movement
            {
                if (hitPosition.x < vcamOther.position.x)
                {
                    if (transform.position.x >= hitPosition.x)
                        destination.x = vcamOther.position.x - vcamOther.localScale.x;
                }
                else if (hitPosition.x > vcamOther.position.x)
                {
                    if (transform.position.x <= hitPosition.x)
                        destination.x = vcamOther.position.x + vcamOther.localScale.x;
                }
            }
            else if ((verticalDiff >= transform.localScale.y - 0.1f &&
                verticalDiff <= transform.localScale.y + 0.1f))//lock vertical movement
            {
                if (hitPosition.y < vcamOther.position.y)
                {
                    if (transform.position.y >= hitPosition.y)
                        destination.y = vcamOther.position.y - vcamOther.localScale.y;
                }
                else if (hitPosition.y > vcamOther.position.y)
                {
                    if (transform.position.y <= hitPosition.y)
                        destination.y = vcamOther.position.y + vcamOther.localScale.y;
                }
            }

            
        }
        transform.position = destination;
        //clamp with camera device
        ClampVcamWihBoundingCamera();
    }

    /// <summary>
    /// Use to clamp plane position to not going outside of the camera while we playing, USED on PlayerMovement Method
    /// </summary>
    private void ClampVcamWihBoundingCamera()
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
    #endregion
}
