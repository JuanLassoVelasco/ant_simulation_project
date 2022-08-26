using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBehaviour : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ant"))
        {
            Ant ant = GetComponent<Ant>();

            if (ant.GetAntCargo() < ant.GetAntCarryingCapacity())
            {
                Destroy(this.gameObject);
            }
        }
    }
}
