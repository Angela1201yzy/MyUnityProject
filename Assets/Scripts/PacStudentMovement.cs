using System.Collections;
using UnityEngine;

[AddComponentMenu("PacStudent/PacStudentMovement")]
public class PacStudentMovement : MonoBehaviour
{
    [Tooltip("Waypoints in clockwise order around the top-left inner block.")]
    public Transform[] waypoints;

    [Tooltip("Speed in units per second. Movement is linear; duration = distance / speed.")]
    public float speed = 2f;

    [Tooltip("Animator controlling PacStudent movement animation. Animator should have a bool 'isMoving' (or change code accordingly).")]
    public Animator animator;

    [Tooltip("Audio source for moving sound (loop recommended).")]
    public AudioSource moveAudio;

    int currentIndex = 0;
    Coroutine moveLoopCoroutine;

    void Start()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogError("[PacStudentMovement] Assign at least 2 waypoints (in clockwise order).");
            enabled = false;
            return;
        }

        if (animator == null) animator = GetComponent<Animator>();
        if (moveAudio == null) moveAudio = GetComponent<AudioSource>();

        // 可选：把 PacStudent 放到第一个 waypoint（使测试更直观）
        transform.position = waypoints[0].position;
        currentIndex = 0;

        // 开始循环移动
        moveLoopCoroutine = StartCoroutine(MoveLoop());
    }

    IEnumerator MoveLoop()
    {
        while (true)
        {
            Transform from = waypoints[currentIndex];
            Transform to = waypoints[(currentIndex + 1) % waypoints.Length];

            yield return StartCoroutine(MoveFromTo(from.position, to.position));

            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }

    IEnumerator MoveFromTo(Vector3 start, Vector3 end)
    {
        // 启动移动动画与声音（如果有）
        if (animator != null) animator.SetBool("isMoving", true);
        if (moveAudio != null && !moveAudio.isPlaying) moveAudio.Play();

        Vector3 offset = end - start;
        float distance = offset.magnitude;
        if (distance <= Mathf.Epsilon) yield break;
        Vector3 dir = offset.normalized;

        float duration = distance / Mathf.Max(0.0001f, speed);
        float elapsed = 0f;

        // 线性插值（t 从 0 -> 1 均匀增长，保证匀速）
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; // 帧率无关
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, end, t);

            // 可选：让 sprite 面向运动方向（仅作示例，按需调整）
            UpdateFacing(dir);

            yield return null;
        }

        // 确保位置精确到终点
        transform.position = end;

        // NOTE: 不在每个角落停止动画/声音，因为要求是移动时播放。
        // 如果你想在角落短暂停止（或播放其它状态），可以在这里处理。
        yield break;
    }

    void UpdateFacing(Vector3 dir)
    {
        // 简单示例：如果主要水平移动则翻转 X 轴方向（假设默认朝右）
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(dir.x) * Mathf.Abs(s.x);
            transform.localScale = s;
        }
        // 对于上下方向，你可以通过 animator 参数来切换竖直朝向动画
    }

    // 调试：在 Scene 视图绘制路径
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

    // 可在以后需要时调用来停止移动与声音
    public void StopMovement()
    {
        if (moveLoopCoroutine != null) StopCoroutine(moveLoopCoroutine);
        if (animator != null) animator.SetBool("isMoving", false);
        if (moveAudio != null) moveAudio.Stop();
    }
}
