using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Colony : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMesh foodCountText;
    [SerializeField] private GameObject antPrefab;
    [SerializeField] private int antPopulation = 10;

    private bool antsReleased = false;

    private int storedFood = 0;

    private void Update()
    {
        foodCountText.text = storedFood.ToString();
    }

    public void StoreFood(int foodAmount)
    {
        storedFood += foodAmount;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!antsReleased)
        {
            for (int i = 0; i < antPopulation; i++)
            {
                Instantiate(antPrefab, transform.position, Quaternion.identity);
            }
            antsReleased = true;
        }
    }
}
