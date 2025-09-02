using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInteract : MonoBehaviour, IInteractable
{
    //protected Outline _outline;
    protected bool _isHovered = false;

    protected virtual void Awake()
    {
        if (GetComponent<Collider>() == null)
        {
            var mf = GetComponent<MeshFilter>();
            if (mf != null)
            {
                var mc = gameObject.AddComponent<MeshCollider>();
                mc.convex = true;
            }
        }

        //_outline = GetComponent<Outline>();
        //if (_outline == null) _outline = gameObject.AddComponent<Outline>();
        //_outline.enabled = false;
    }

    public virtual void OnHoverEnter()
    {
        _isHovered = true;
        //if (_outline != null) _outline.enabled = true;
    }

    public virtual void OnHoverExit()
    {
        _isHovered = false;
        //if (_outline != null) _outline.enabled = false;
    }

    public void ForceUnhover()
    {
        _isHovered = false;
        //if (_outline != null) _outline.enabled = false;
    }

    public virtual void OnInteract()
    {
        Debug.Log($"Interacción con {gameObject.name}");
    }
}
