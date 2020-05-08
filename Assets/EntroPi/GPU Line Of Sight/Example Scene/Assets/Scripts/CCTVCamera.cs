using UnityEngine;

public class CCTVCamera : MonoBehaviour
{
    [SerializeField]
    private float m_MaxAngle = 45f;

    [SerializeField]
    private float m_Speed = 1f;

    private float m_StartTime;

    // Use this for initialization
    private void Start()
    {
        m_StartTime = Time.time;
    }

    // Update is called once per frame
    private void Update()
    {
        float sin = Mathf.Clamp(Mathf.Sin((Time.time - m_StartTime) * m_Speed) * 1.5f, -1, 1);

        transform.localEulerAngles = new Vector3(0, sin * m_MaxAngle, 0);
    }
}