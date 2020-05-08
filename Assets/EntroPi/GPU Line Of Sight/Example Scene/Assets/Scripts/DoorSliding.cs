using UnityEngine;

public class DoorSliding : MonoBehaviour
{
    [SerializeField]
    private Door m_LeftDoor;

    [SerializeField]
    private Door m_RightDoor;

    // Use this for initialization
    private void Start()
    {
        if (!m_LeftDoor || !m_RightDoor)
        {
            enabled = false;
            Debug.LogError("Left or Right door not assigned");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        m_LeftDoor.IsOpened = true;
        m_RightDoor.IsOpened = true;
    }

    private void OnTriggerExit(Collider other)
    {
        m_LeftDoor.IsOpened = false;
        m_RightDoor.IsOpened = false;
    }
}