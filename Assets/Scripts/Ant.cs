using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : MonoBehaviour
{
    [SerializeField] private LayerMask foodLayer;
    [SerializeField] private LayerMask pheremoneLayer;
    [SerializeField] private LayerMask colonyLayer;
    [SerializeField] private SpriteRenderer heldFoodIndicator;
    [SerializeField] private GameObject colonyTrackerPheremonePrefab;
    [SerializeField] private GameObject foodTrackerPheremonePrefab;
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private float turnStrength = 2.0f;
    [SerializeField] private float wanderStrength = 1.0f;
    [SerializeField] private float searchRadius = 4.0f;
    [SerializeField] private float pheremoneReleaseRate = 1.0f;
    [SerializeField] private int foodCapacity = 1;
    [SerializeField] private Transform target;

    private bool isCarryingFood = false;
    private bool hasTarget = false;

    private Vector2 desiredDirection;
    private Vector2 velocity;
    private Vector2 position;

    private float previousTime = 0;
    private float currentTime = 0;

    private int currentFoodCargo = 0;

    private void Start()
    {
        previousTime = Time.fixedTime;
    }

    private void Update()
    {
        currentTime = Time.fixedTime;

        if (currentTime - previousTime >= pheremoneReleaseRate)
        {
            ReleasePheremone();
            previousTime = currentTime;
        }

        SetTarget();

        SetDesiredDirection();

        MoveInDesiredDirection();
    }

    private void SetTarget()
    {
        if (target == null)
        {
            hasTarget = false;
            if (isCarryingFood)
            {
                SearchForColony();
            }
            else
            {
                SearchForFood();
            }
        }
    }

    private void SearchForColony()
    {
        Collider2D colony = Physics2D.OverlapCircle(gameObject.transform.position, searchRadius, colonyLayer);

        if (colony != null)
        {
            target = colony.transform;
            hasTarget = true;
        }
    }

    private void SetDesiredDirection()
    {
        if (hasTarget)
        {
            desiredDirection = ((Vector2)target.position - position).normalized;
        }
        else
        {
            desiredDirection = (desiredDirection + Random.insideUnitCircle * wanderStrength).normalized;
        }
    }

    private void MoveInDesiredDirection()
    {
        Vector2 desiredVelocity = desiredDirection * speed;
        Vector2 desiredTurningForce = (desiredVelocity - velocity) * turnStrength;
        Vector2 acceleration = Vector2.ClampMagnitude(desiredTurningForce, turnStrength) / 1;

        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, speed);
        position += velocity * Time.deltaTime;

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));
    }

    private void ReleasePheremone()
    {
        if (isCarryingFood)
        {
            Instantiate(foodTrackerPheremonePrefab, gameObject.transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(colonyTrackerPheremonePrefab, gameObject.transform.position, Quaternion.identity);
        }
    }

    private void SearchForFood()
    {
        Collider2D[] visibleFood = Physics2D.OverlapCircleAll(position, searchRadius, foodLayer);

        if (visibleFood.Length > 0)
        {
            target = visibleFood[Random.Range(0, visibleFood.Length)].transform;
            hasTarget = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;


        velocity = -velocity;
        desiredDirection = collision.GetContact(0).normal;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;

        if (collision.gameObject.CompareTag("Food"))
        {
            if (!(currentFoodCargo >= foodCapacity))
            {
                CollectFood();
            }
        }
        else if (collision.gameObject.CompareTag("Colony"))
        {
            if (isCarryingFood)
            {
                DropOffFood();
                target = null;
            }
        }
    }

    private void CollectFood()
    {
        Color foodColorTemp = heldFoodIndicator.color;
        foodColorTemp.a = 1;
        isCarryingFood = true;
        heldFoodIndicator.color = foodColorTemp;
        currentFoodCargo++;
    }

    private void DropOffFood()
    {
        Color foodColorTemp = heldFoodIndicator.color;
        foodColorTemp.a = 0;
        isCarryingFood = false;
        heldFoodIndicator.color = foodColorTemp;
        currentFoodCargo = 0;
    }
}
