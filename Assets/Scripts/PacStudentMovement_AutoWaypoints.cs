using UnityEngine;

[AddComponentMenu("PacStudent/PacStudentMovement_DirectionInt")]
public class PacStudentMovement_DirectionInt : MonoBehaviour
{
    [Header("Waypoints (world-space Transforms)")]
    public Transform[] waypoints;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float arriveThreshold = 0.05f;

    [Header("Options")]
    public bool setPositionToFirstOnStart = true;
    public bool detachWaypointsIfChild = true;

    private int currentIndex = 0;
    private Animator animator;
    private AudioSource audioSource;

    void Start()
    {
        // 检查 Waypoints
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("[PacStudentMovement] No waypoints assigned! Drag them into the inspector.");
            enabled = false;
            return;
        }

        // 自动解绑 waypoint（防止矩形随 PacStudent 移动）
        if (detachWaypointsIfChild)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null && waypoints[i].IsChildOf(transform))
                {
                    waypoints[i].parent = null;
                }
            }
        }

        animator = GetComponent<Animator>();
        if (animator != null && animator.applyRootMotion)
        {
            animator.applyRootMotion = false;
        }

        audioSource = GetComponent<AudioSource>();

        if (setPositionToFirstOnStart && waypoints[0] != null)
            transform.position = waypoints[0].position;

        currentIndex = 0;

        if (speed <= 0f)
            Debug.LogWarning("[PacStudentMovement] Speed is zero or negative.");
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        float dist = toTarget.magnitude;

        if (dist <= arriveThreshold)
        {
            // 到达当前点，切换到下一个
            currentIndex = (currentIndex + 1) % waypoints.Length;
            return;
        }

        // 匀速移动
        Vector3 dir = toTarget.normalized;
        float moveAmount = speed * Time.deltaTime;
        transform.position += dir * Mathf.Min(moveAmount, dist);

        // 音效
        if (audioSource != null && !audioSource.isPlaying) audioSource.Play();

        // 动画：移动中 + 根据方向切换
        if (animator != null)
        {
            animator.SetBool("isMoving", true);
            UpdateFacing(dir);
        }
    }

    // 根据移动方向更新 Animator Direction 参数
    void UpdateFacing(Vector3 dir)
    {
        if (animator == null) return;

        int dirInt = 0; // 默认 Down

        // 主方向判断
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            dirInt = dir.x > 0 ? 2 : 1; // Right : Left
        }
        else
        {
            dirInt = dir.y > 0 ? 3 : 0; // Up : Down
        }

        animator.SetInteger("Direction", dirInt);
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.08f);
            Transform next = waypoints[(i + 1) % waypoints.Length];
            if (next != null) Gizmos.DrawLine(waypoints[i].position, next.position);
        }
    }
}
