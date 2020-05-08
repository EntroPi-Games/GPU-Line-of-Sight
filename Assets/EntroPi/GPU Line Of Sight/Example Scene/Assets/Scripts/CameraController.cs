using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float m_SmoothTime = 0.3f;

    [SerializeField]
    private Transform m_Target;

    private Vector3 m_Velocity = new Vector3(0, 0, 0);

    // Update is called once per frame
    private void Update()
    {
        if (m_Target)
        {
            Vector3 targetPos = m_Target.transform.position;

            //Keep camera height the same
            targetPos.y = transform.position.y;

            //targetPos = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * m_LerpSpeed);
            targetPos = Vector3.SmoothDamp(transform.position, targetPos, ref m_Velocity, m_SmoothTime);

            transform.position = targetPos;
        }
    }
}