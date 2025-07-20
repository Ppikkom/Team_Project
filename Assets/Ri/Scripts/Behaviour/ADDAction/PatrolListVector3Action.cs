using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine.AI;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol_ListVector3", story: "[Agent] navigate to [Waypoints]", category: "Action", id: "4d5c1c06f6e01bbd6fa7f785b511658f")]
internal partial class PatrolListVector3Action : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<List<Vector3>> Waypoints;
    [SerializeReference] public BlackboardVariable<float> Speed = new(3f);
    [SerializeReference] public BlackboardVariable<float> WaypointWaitTime = new(1.0f);
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new(0.2f);
    [SerializeReference] public BlackboardVariable<string> AnimatorSpeedParam = new("SpeedMagnitude");
    [Tooltip("Should patrol restart from the latest point?")]
    [SerializeReference] public BlackboardVariable<bool> PreserveLatestPatrolPoint = new(false);

    private NavMeshAgent m_NavMeshAgent;
    private Animator m_Animator;
    [CreateProperty] private Vector3 m_CurrentTarget;
    [CreateProperty] private float m_OriginalStoppingDistance = -1f;
    [CreateProperty] private float m_OriginalSpeed = -1f;
    [CreateProperty] private float m_WaypointWaitTimer;
    private float m_CurrentSpeed;
    [CreateProperty] private int m_CurrentPatrolPoint = 0;
    [CreateProperty] private bool m_Waiting;

    protected override Status OnStart()
    {
        if (Agent.Value == null)
        {
            LogFailure("No agent assigned.");
            return Status.Failure;
        }

        if (Waypoints.Value == null || Waypoints.Value.Count == 0)
        {
            LogFailure("No waypoints to patrol assigned.");
            return Status.Failure;
        }

        Initialize();

        m_Waiting = false;
        m_WaypointWaitTimer = 0.0f;

        MoveToNextWaypoint();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value == null || Waypoints.Value == null)
        {
            return Status.Failure;
        }

        if (m_Waiting)
        {
            if (m_WaypointWaitTimer > 0.0f)
            {
                m_WaypointWaitTimer -= Time.deltaTime;
            }
            else
            {
                m_WaypointWaitTimer = 0f;
                m_Waiting = false;
                MoveToNextWaypoint();
            }
        }
        else
        {
            float distance = GetDistanceToWaypoint();
            bool destinationReached = distance <= DistanceThreshold;

            // Check if we've reached the waypoint (ensuring NavMeshAgent has completed path calculation if available)
            if (destinationReached && (m_NavMeshAgent == null || !m_NavMeshAgent.pathPending))
            {
                m_WaypointWaitTimer = WaypointWaitTime.Value;
                m_Waiting = true;
                m_CurrentSpeed = 0;

                return Status.Running;
            }
            else if (m_NavMeshAgent == null) // transform-based movement
            {
                m_CurrentSpeed = SimpleMoveTowardsLocation(Agent.Value.transform, m_CurrentTarget, Speed, distance, 1f);
            }
        }

        UpdateAnimatorSpeed();

        return Status.Running;
    }

    protected override void OnEnd()
    {
        UpdateAnimatorSpeed(0f);

        if (m_NavMeshAgent != null)
        {
            if (m_NavMeshAgent.isOnNavMesh)
            {
                m_NavMeshAgent.ResetPath();
            }
            m_NavMeshAgent.speed = m_OriginalSpeed;
            m_NavMeshAgent.stoppingDistance = m_OriginalStoppingDistance;
        }
    }

    protected override void OnDeserialize()
    {
        // If using a navigation mesh, we need to reset default value before Initialize.
        m_NavMeshAgent = Agent.Value.GetComponentInChildren<NavMeshAgent>();
        if (m_NavMeshAgent != null)
        {
            if (m_OriginalSpeed >= 0f)
                m_NavMeshAgent.speed = m_OriginalSpeed;
            if (m_OriginalStoppingDistance >= 0f)
                m_NavMeshAgent.stoppingDistance = m_OriginalStoppingDistance;

            m_NavMeshAgent.Warp(Agent.Value.transform.position);
        }

        int patrolPoint = m_CurrentPatrolPoint - 1;
        Initialize();
        // During deserialization, bypass PreserveLatestPatrolPoint.
        m_CurrentPatrolPoint = patrolPoint;
    }

    private void Initialize()
    {
        m_Animator = Agent.Value.GetComponentInChildren<Animator>();
        m_NavMeshAgent = Agent.Value.GetComponentInChildren<NavMeshAgent>();
        if (m_NavMeshAgent != null)
        {
            if (m_NavMeshAgent.isOnNavMesh)
            {
                m_NavMeshAgent.ResetPath();
            }

            m_OriginalSpeed = m_NavMeshAgent.speed;
            m_NavMeshAgent.speed = Speed.Value;
            m_OriginalStoppingDistance = m_NavMeshAgent.stoppingDistance;
            m_NavMeshAgent.stoppingDistance = DistanceThreshold;
        }

        m_CurrentPatrolPoint = PreserveLatestPatrolPoint.Value ? m_CurrentPatrolPoint - 1 : -1;

        UpdateAnimatorSpeed(0f);
    }

    private float GetDistanceToWaypoint()
    {
        if (m_NavMeshAgent != null)
        {
            return m_NavMeshAgent.remainingDistance;
        }

        Vector3 targetPosition = m_CurrentTarget;
        Vector3 agentPosition = Agent.Value.transform.position;
        agentPosition.y = targetPosition.y; // Ignore y for distance check.
        return Vector3.Distance(agentPosition, targetPosition);
    }

    private void MoveToNextWaypoint()
    {
        m_CurrentPatrolPoint = (m_CurrentPatrolPoint + 1) % Waypoints.Value.Count;

        m_CurrentTarget = Waypoints.Value[m_CurrentPatrolPoint];
        if (m_NavMeshAgent != null)
        {
            m_NavMeshAgent.SetDestination(m_CurrentTarget);
        }
    }

    private void UpdateAnimatorSpeed(float explicitSpeed = -1f)
    {
        UpdateAnim(m_Animator, AnimatorSpeedParam, m_NavMeshAgent, m_CurrentSpeed, explicitSpeed: explicitSpeed);

    }

    // Add
    bool UpdateAnim(Animator animator, string speedParameterName, NavMeshAgent navMeshAgent, float currentSpeed, float minSpeedThreshold = 0.1f, float explicitSpeed = -1f)
    {
        if (animator == null || string.IsNullOrEmpty(speedParameterName))
        {
            return false;
        }

        float num = 0f;
        num = ((explicitSpeed >= 0f) ? explicitSpeed : ((!(navMeshAgent != null)) ? currentSpeed : navMeshAgent.velocity.magnitude));
        if (num <= minSpeedThreshold)
        {
            num = 0f;
        }

        animator.SetFloat(speedParameterName, num);
        return true;
    }

    static float SimpleMoveTowardsLocation(Transform agentTransform, Vector3 targetLocation, float speed, float distance, float slowDownDistance = 0f, float minSpeedRatio = 0.1f)
    {
        if (agentTransform == null)
        {
            return 0f;
        }

        Vector3 position = agentTransform.position;
        float num = speed;
        if (slowDownDistance > 0f && distance < slowDownDistance)
        {
            float num2 = distance / slowDownDistance;
            num = Mathf.Max(speed * minSpeedRatio, speed * num2);
        }

        Vector3 vector = targetLocation - position;
        vector.y = 0f;
        if (vector.sqrMagnitude > 0.0001f)
        {
            vector.Normalize();
            position += vector * (num * Time.deltaTime);
            agentTransform.position = position;
            agentTransform.forward = vector;
        }

        return num;
    }
}
