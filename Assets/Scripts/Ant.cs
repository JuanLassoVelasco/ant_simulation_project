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
    [SerializeField] private GameObject leftSensor;
    [SerializeField] private GameObject middleSensor;
    [SerializeField] private GameObject rightSensor;
    [SerializeField] private float sensorRadius = 3.0f;
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

    private string colonyPheremoneTag = "ColonyPheremone";
    private string foodPheremoneTag = "FoodPheremone";
    private string trackingPheremone;

    private float previousTime = 0;
    private float currentTime = 0;

    private int currentFoodCargo = 0;

    private void Start()
    {
        previousTime = Time.fixedTime;
        trackingPheremone = foodPheremoneTag;
        position = transform.position;
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
        Collider2D colony = Physics2D.OverlapCircle(position, searchRadius, colonyLayer);

        if (colony != null)
        {
            target = colony.transform;
            hasTarget = true;
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

    private void SetDesiredDirection()
    {
        if (hasTarget)
        {
            desiredDirection = ((Vector2)target.position - position).normalized;
        }
        else
        {
            desiredDirection = (desiredDirection + Random.insideUnitCircle * wanderStrength).normalized;
            ScanForPheremoneTrail();
        }
    }

    private void ScanForPheremoneTrail()
    {
        int leftPCount = SampleSensorArea(leftSensor);
        int middlePCount = SampleSensorArea(middleSensor);
        int rightPCount = SampleSensorArea(rightSensor);

        if (leftPCount + middlePCount + rightPCount > 0)
        {
            if (leftPCount > rightPCount)
            {
                desiredDirection += ((Vector2)leftSensor.transform.position - position).normalized;
            }
            else if (middlePCount > Max(leftPCount, rightPCount))
            {
                desiredDirection += ((Vector2)middleSensor.transform.position - position).normalized;
            }
            else if (rightPCount > leftPCount)
            {
                desiredDirection += ((Vector2)rightSensor.transform.position - position).normalized;
            }
        }
    }

    private int Max(int num1, int num2)
    {
        if (num1 > num2)
        {
            return num1;
        }

        return num2;
    }

    private int SampleSensorArea(GameObject sensor)
    {
        int pheremoneCount = 0;

        Collider2D[] pheremones = Physics2D.OverlapCircleAll(sensor.transform.position, sensorRadius);

        foreach (Collider2D pheremone in pheremones)
        {
            if (pheremone.CompareTag(trackingPheremone))
            {
                pheremoneCount++;
            }
        }

        return pheremoneCount;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;
        if (collision.gameObject.CompareTag("Ant")) return;

        velocity += velocity.magnitude * collision.GetContact(0).normal;
        desiredDirection = velocity.magnitude * collision.GetContact(0).normal;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;

        if (collision.gameObject.CompareTag("Food"))
        {
            if (!(currentFoodCargo >= foodCapacity))
            {
                CollectFood(collision);
            }
        }
        else if (collision.gameObject.CompareTag("Colony"))
        {
            if (isCarryingFood)
            {
                Colony colony = collision.GetComponent<Colony>();
                DropOffFood(colony);
                target = null;
            }
        }
    }

    private void CollectFood(Collider2D collision)
    {
        Color foodColorTemp = heldFoodIndicator.color;
        foodColorTemp.a = 1;
        isCarryingFood = true;
        heldFoodIndicator.color = foodColorTemp;
        currentFoodCargo++;
        trackingPheremone = colonyPheremoneTag;
        Destroy(collision.gameObject);
        target = null;
        velocity = -velocity;
    }

    private void DropOffFood(Colony colony)
    {
        Color foodColorTemp = heldFoodIndicator.color;
        colony.StoreFood(currentFoodCargo);
        foodColorTemp.a = 0;
        isCarryingFood = false;
        heldFoodIndicator.color = foodColorTemp;
        currentFoodCargo = 0;
        trackingPheremone = foodPheremoneTag;
        velocity = -velocity;
    }

    public int GetAntCargo()
    {
        return currentFoodCargo;
    }

    public int GetAntCarryingCapacity()
    {
        return foodCapacity;
    }
}
