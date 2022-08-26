using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pheremone : MonoBehaviour
{
    [SerializeField] private float pheremoneLifetime = 100.0f;

    private SpriteRenderer spriteRenderer;
    private Color currentColor;
    private float dissapateDelta;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        dissapateDelta = 1 / pheremoneLifetime;
        currentColor = spriteRenderer.color;
    }

    private void Update()
    {
        currentColor.a -= dissapateDelta * Time.deltaTime;
        pheremoneLifetime -= Time.deltaTime;
        spriteRenderer.color = currentColor;

        if (pheremoneLifetime <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
