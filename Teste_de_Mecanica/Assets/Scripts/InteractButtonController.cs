using UnityEngine;

public class InteractButtonController : MonoBehaviour
{
    public static InteractButtonController Instance;

    public GameObject buttonSprite;

    private Transform target;

    private Camera cam;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        cam = Camera.main;

        buttonSprite.SetActive(false);
    }

    private void Update()
    {
        if (target == null)
            return;

        Vector3 pos =
            cam.WorldToScreenPoint(
                target.position +
                Vector3.up * 1.5f);

        buttonSprite.transform.position = pos;
    }

    public void ShowButton(Transform npc)
    {
        target = npc;

        buttonSprite.SetActive(true);
    }

    public void HideButton()
    {
        target = null;

        buttonSprite.SetActive(false);
    }
}