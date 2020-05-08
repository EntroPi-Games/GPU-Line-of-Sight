using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject m_CameraPrefab;

    [SerializeField]
    private float m_MovementSpeed = 5.0f;

    [SerializeField]
    private float m_RotationSpeed = 0.3f;

    private float m_LookAngle = 0;
    private float m_AngleVelocity = 0;

    private void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(mouseRay, out hit))
        {
            Vector3 aimDir = new Vector3(hit.point.x - transform.position.x, 0, hit.point.z - transform.position.z);
            aimDir.Normalize();

            float angle = Mathf.Rad2Deg * Mathf.Atan2(aimDir.x, aimDir.z);
            m_LookAngle = Mathf.SmoothDampAngle(m_LookAngle, angle, ref m_AngleVelocity, m_RotationSpeed);

            transform.eulerAngles = new Vector3(0, m_LookAngle, 0);
        }

        Vector3 translation = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        translation *= Time.deltaTime * m_MovementSpeed;

        CharacterController controller = gameObject.GetComponent<CharacterController>();
        if (controller)
        {
            controller.Move(translation);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceCamera();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
#if UNITY_5_2 || UNITY_5_3
            Application.LoadLevel(Application.loadedLevel);
#else
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
#endif
        }
    }

    private void PlaceCamera()
    {
        if (m_CameraPrefab)
        {
            float targetDistance = 1f;

            Ray targetRay = new Ray(transform.position, transform.forward);
            if (!Physics.Raycast(targetRay, targetDistance * 2f, 1))
            {
                Vector3 targetPos = transform.position + transform.forward * targetDistance;
                targetPos.y = 0;
                GameObject.Instantiate(m_CameraPrefab, targetPos, transform.rotation);
            }
        }
    }
}