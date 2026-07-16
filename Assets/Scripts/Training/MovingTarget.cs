using UnityEngine;

namespace FPSTrainingRoom.Training
{
    public enum MovementPattern
    {
        Linear,
        Circular,
        FigureEight,
        Random,
        SineWave,
        PathWaypoint
    }

    public class MovingTarget : TargetDummy
    {
        [Header("Movement Settings")]
        public MovementPattern movementPattern = MovementPattern.Linear;
        public float moveSpeed = 3f;
        public float moveRange = 5f;
        public bool randomizeSpeed = false;
        public Vector2 speedRange = new Vector2(1f, 5f);

        [Header("Circular Motion")]
        public float circleRadius = 3f;
        public float circleHeight = 0f;

        [Header("Path Waypoints")]
        public Transform[] waypoints;
        public bool loopPath = true;
        public float waypointWaitTime = 0.5f;

        [Header("Player Detection")]
        public bool facePlayer = false;
        public float rotationSpeed = 90f;
        public bool detectPlayerRange = false;
        public float detectionRange = 20f;

        protected Vector3 startPosition;
        protected float angle;
        protected int currentWaypoint;
        protected float waypointTimer;
        protected Vector3 moveDirection;
        protected float currentSpeed;
        protected Transform playerTransform;

        protected override void Start()
        {
            base.Start();
            startPosition = transform.position;
            currentSpeed = randomizeSpeed
                ? Random.Range(speedRange.x, speedRange.y)
                : moveSpeed;

            if (detectPlayerRange)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTransform = player.transform;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (isDestroyed) return;

            if (detectPlayerRange && playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position,
                    playerTransform.position);
                if (distance > detectionRange) return;
            }

            switch (movementPattern)
            {
                case MovementPattern.Linear:
                    MoveLinear();
                    break;
                case MovementPattern.Circular:
                    MoveCircular();
                    break;
                case MovementPattern.FigureEight:
                    MoveFigureEight();
                    break;
                case MovementPattern.Random:
                    MoveRandom();
                    break;
                case MovementPattern.SineWave:
                    MoveSineWave();
                    break;
                case MovementPattern.PathWaypoint:
                    MoveAlongPath();
                    break;
            }

            if (facePlayer && playerTransform != null)
            {
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        protected virtual void MoveLinear()
        {
            transform.position += moveDirection * currentSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, startPosition) > moveRange)
            {
                moveDirection = -moveDirection;
                transform.position = startPosition +
                    moveDirection * moveRange;
            }
        }

        protected virtual void MoveCircular()
        {
            angle += currentSpeed * Time.deltaTime / circleRadius;
            float x = Mathf.Cos(angle) * circleRadius;
            float z = Mathf.Sin(angle) * circleRadius;
            transform.position = startPosition + new Vector3(x, circleHeight, z);
        }

        protected virtual void MoveFigureEight()
        {
            angle += currentSpeed * Time.deltaTime / 2f;
            float x = Mathf.Sin(angle) * moveRange;
            float z = Mathf.Sin(angle * 2f) * moveRange * 0.5f;
            transform.position = startPosition + new Vector3(x, 0f, z);
        }

        protected Vector3 randomDirection;
        protected float randomChangeTimer;

        protected virtual void MoveRandom()
        {
            randomChangeTimer -= Time.deltaTime;
            if (randomChangeTimer <= 0f)
            {
                randomDirection = Random.insideUnitSphere;
                randomDirection.y = 0f;
                randomDirection.Normalize();
                randomChangeTimer = Random.Range(1f, 3f);

                if (randomizeSpeed)
                    currentSpeed = Random.Range(speedRange.x, speedRange.y);
            }

            transform.position += randomDirection * currentSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, startPosition) > moveRange)
            {
                randomDirection = (startPosition - transform.position).normalized;
            }
        }

        protected virtual void MoveSineWave()
        {
            angle += currentSpeed * Time.deltaTime;
            float x = Mathf.Sin(angle) * moveRange;
            float z = Mathf.Cos(angle * 0.5f) * moveRange * 0.5f;
            transform.position = startPosition + new Vector3(x, 0f, z);
        }

        protected virtual void MoveAlongPath()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            if (waypointTimer > 0f)
            {
                waypointTimer -= Time.deltaTime;
                return;
            }

            Transform targetWaypoint = waypoints[currentWaypoint];
            if (targetWaypoint == null) return;

            Vector3 direction = (targetWaypoint.position - transform.position).normalized;
            transform.position += direction * currentSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.5f)
            {
                if (!loopPath && currentWaypoint >= waypoints.Length - 1)
                {
                    currentWaypoint = 0;
                    transform.position = waypoints[0].position;
                }
                else
                {
                    currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
                    waypointTimer = waypointWaitTime;
                }
            }
        }

        public virtual void SetMovementPattern(MovementPattern pattern)
        {
            movementPattern = pattern;
            angle = 0f;
            currentWaypoint = 0;
            moveDirection = Vector3.forward;
        }

        public virtual void SetSpeed(float speed)
        {
            currentSpeed = speed;
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (Application.isPlaying) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(startPosition, moveRange);

            if (movementPattern == MovementPattern.PathWaypoint && waypoints != null)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < waypoints.Length; i++)
                {
                    if (waypoints[i] != null)
                    {
                        Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                        if (i > 0 && waypoints[i - 1] != null)
                            Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
                    }
                }
                if (loopPath && waypoints.Length > 1 &&
                    waypoints[0] != null && waypoints[^1] != null)
                    Gizmos.DrawLine(waypoints[^1].position, waypoints[0].position);
            }
        }
    }
}
