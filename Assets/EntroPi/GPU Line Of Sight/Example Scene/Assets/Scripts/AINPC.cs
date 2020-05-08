using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class AINPC : MonoBehaviour
{
    private NavMeshAgent m_NavAgent;

    private static System.Random m_Random;

    // Use this for initialization
    private void Start()
    {
        if (null == m_Random)
            m_Random = new System.Random();

        m_NavAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_NavAgent.remainingDistance < 1.0f)
        {
            m_NavAgent.SetDestination(GetNewDestination());
        }
    }

    private static Vector3 GetNewDestination()
    {
        GameObject[] m_WayPoints = GameObject.FindGameObjectsWithTag("Respawn");
        int index = m_Random.Next(m_WayPoints.Length);

        return m_WayPoints[index].transform.position;
    }
}