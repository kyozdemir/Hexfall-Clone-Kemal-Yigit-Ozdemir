using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteOutline : MonoBehaviour
{
    public Color color = Color.white;
    public bool isOutlineActive { get; set; }
    private SpriteRenderer spriteRenderer;

    void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateOutline(isOutlineActive);
        
    }

    void OnDisable()
    {
        UpdateOutline(false);
    }

    void Update()
    {
        UpdateOutline(false);
    }

    public void UpdateOutline(bool outline)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_Outline", outline ? 1f : 0);
        mpb.SetColor("_OutlineColor", color);
        spriteRenderer.SetPropertyBlock(mpb);
    }
}
