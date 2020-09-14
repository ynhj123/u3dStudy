using UnityEngine;

public class BaseHuman : MonoBehaviour
{
    // Start is called before the first frame update
    protected bool isMoving = false;
    private Vector3 targetPosition;
    public float speed = 1.2f;
    private Animator animator;
    public string desc;

    public void MoveTo(Vector3 pos)
    {
        targetPosition = pos;
        isMoving = true;
        animator.SetBool("isMoving", true);
    }
    public void MoveUpdate()
    {
        if (isMoving == false)
        {
            return;
        }
        Vector3 position = transform.position;
        transform.position = Vector3.MoveTowards(position, targetPosition, speed * Time.deltaTime);
        transform.LookAt(targetPosition);
        if (Vector3.Distance(position, targetPosition) < 0.05f)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
        }
    }
    public void Start()
    {
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    public void Update()
    {
        MoveUpdate();
    }
}
