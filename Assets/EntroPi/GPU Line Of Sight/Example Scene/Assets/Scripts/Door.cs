using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private Vector3 m_SlideDirection = Vector3.left;

    [SerializeField]
    private float m_SlideDistance = 1.0f;

    [SerializeField]
    private float m_SlideSpeed = 5.0f;

    private Vector3 m_StartPosition;
    private float m_Offset = 0;
    private bool m_Opened;

    public bool IsOpened
    {
        set { m_Opened = value; }
        get { return m_Opened; }
    }

    // Use this for initialization
    private void Start()
    {
        m_StartPosition = transform.localPosition;
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_Opened)
            m_Offset += Time.deltaTime * m_SlideSpeed;
        else
            m_Offset -= Time.deltaTime * m_SlideSpeed;

        m_Offset = Mathf.Clamp(m_Offset, 0, m_SlideDistance);

        transform.localPosition = m_StartPosition + m_SlideDirection * m_Offset;
    }
}