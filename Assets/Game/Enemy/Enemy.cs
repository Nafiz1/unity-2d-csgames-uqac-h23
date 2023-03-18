using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 2f; // enemy speed
    public Transform[] waypoints; // patrol points
    public float detectionRange = 5f; // detection range of the player
    public float investigationTime = 5f; // time to investigate when the player is lost
    public LayerMask playerLayer; // layer where the player is located
    public float obstacleAvoidanceDistance = 1f; // distance to avoid obstacles
    public float pathfindingDistance = 5f; // distance to start pathfinding
    private int currentWaypointIndex = 0; // current waypoint index
    private Transform player; // player transform
    private bool playerDetected = false; // flag to indicate if the player has been detected
    private bool investigating = false; // flag to indicate if the enemy is investigating
    private Vector3 investigationPosition; // position to investigate
    private Vector3 targetPosition; // target position
    private Vector3[] path; // path to follow
    private int currentPathIndex = 0; // current path index

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // find player
        targetPosition = waypoints[currentWaypointIndex].position; // set initial target position
    }

    void Update()
    {
        // check if player is within detection range
        if (Vector3.Distance(transform.position, player.position) < detectionRange)
        {
            playerDetected = true; // player detected
            investigating = false; // stop investigating
            targetPosition = player.position; // set target position to player position
        }
        else if (playerDetected && !investigating)
        {
            // player lost, start investigating
            investigating = true;
            investigationPosition = transform.position;
            Invoke("StopInvestigating", investigationTime); // stop investigating after a period of time
        }

        if (investigating)
        {
            // move to investigation position
            MoveTowardsPosition(investigationPosition);
        }
        else
        {
            // move towards target position
            MoveTowardsPosition(targetPosition);
        }
    }

    void MoveTowardsPosition(Vector3 position)
    {
        // check if obstacle in the way
        RaycastHit2D hit = Physics2D.Raycast(transform.position, position - transform.position, obstacleAvoidanceDistance, LayerMask.GetMask("Obstacle"));
        if (hit.collider != null)
        {
            // obstacle in the way, find path around it
            Vector3[] newPath = Pathfinding.FindPath(transform.position, position, pathfindingDistance, obstacleAvoidanceDistance);
            if (newPath != null && newPath.Length > 0)
            {
                path = newPath;
                currentPathIndex = 0;
            }
        }
        else if (path != null && currentPathIndex < path.Length)
        {
            // follow path
            if (Vector3.Distance(transform.position, path[currentPathIndex]) < obstacleAvoidanceDistance)
            {
                currentPathIndex++;
            }
            if (currentPathIndex < path.Length)
            {
                position = path[currentPathIndex];
            }
            else
            {
                path = null;
            }
        }

        // move towards position
        Vector3 direction = (position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    void StopInvestigating()
    {
        investigating = false;
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length
        targetPosition = waypoints[currentWaypointIndex].position; // set new target position
        playerDetected = false; // reset player detected flag
    }

void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        // enemy spotted player, alert other enemies
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            if (enemy != this)
            {
                enemy.playerDetected = true;
                enemy.investigating = false;
                enemy.targetPosition = player.position;
            }
        }
    }
}
