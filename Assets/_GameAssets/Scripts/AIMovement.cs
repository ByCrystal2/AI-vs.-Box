using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AIMovement : MonoBehaviour
{ // 03052025 tarihi kalinan scirpt. Devaminda rakip ve rakibe saldiri durumlari olacak.
    NavMeshAgent agent;
    Transform currentTarget;
    Transform enemyTarget;
    Enemy enemy;
    Animator animator;
    float defaultSpeed;
    bool isBusy = false;
    public float sightRange = 10f; // G�r�� mesafesi

    public LayerMask enemyLayers; // D��manlar� ve oyuncuyu tespit etmek i�in layer mask

    [Header("Attacking")]
    public float damage = 12f;
    public float attackRange = 2f; // Sald�r� mesafesi
    public float attackSpeed = 1f;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        defaultSpeed = agent.speed;
        agent.updateRotation = false;
    }
    private void Start()
    {
        SetTarget();
    }
    void SetTarget()
    {        
        while (true)
        {
            if (enemyTarget != null) { currentTarget = enemyTarget; break; }
            Transform randomWayPoint = WayPointController.instance.GetRandomWayPoint();
            if (currentTarget == null)
            {
                currentTarget = randomWayPoint;
                break;
            }
            if (currentTarget != randomWayPoint)
            {
                currentTarget = randomWayPoint;
                break;
            }            
        }
        agent.speed = defaultSpeed;
    }
    private void Update()
    {
        if (currentTarget == null || isBusy) return;

        // Hedefe olan mesafe hesaplan�yor
        float distance = Vector3.Distance(transform.position, currentTarget.position);

        // E�er d��man hedeften ��kt�ysa s�f�rla ve yeni hedef bul
        if (IsEnemyTarget() && distance > sightRange)
        {
            ResetSettings();
        }

        //  Yeni: S�rekli d��man arama (an�nda tepki)
        if (enemyTarget == null)
        {
            enemyTarget = FindClosestEnemy();
            if (enemyTarget != null) // E�er yeni d��man bulduysa hedefi de�i�tir
            {
                enemy = enemyTarget.GetComponent<Enemy>();
                currentTarget = enemyTarget;
                agent.speed = defaultSpeed * 1.5f; // D��man� g�r�nce h�z art���
            }
        }

        // D�n��� h�zland�r (H�z: 10f)
        Rotation();

        if (distance > attackRange)
        {
            animator.SetBool("Move", true);
            animator.SetFloat("W2R", (agent.speed / defaultSpeed) - 1);

            if (IsEnemyTarget())
                IfTargetIsEnemy();

            agent.SetDestination(currentTarget.position);
        }
        else
        {
            agent.speed = 0;
            animator.SetBool("Move", false);

            if (!IsEnemyTarget())
                StartCoroutine(WaitPatrol());
            else
                StartCoroutine(WaitAttack());
        }
    }
    IEnumerator WaitPatrol()
    {
        currentTarget = null;
        isBusy = true;
        float waitTime = Random.Range(3, 6); // Bekleme s�resi azalt�ld� (Daha h�zl� ge�i�)
        float timer = 0f;

        while (timer < waitTime)
        {
            yield return new WaitForSeconds(0.3f);

            // Yeni: Beklerken d��man g�rd���nde hemen harekete ge�
            enemyTarget = FindClosestEnemy();
            if (enemyTarget != null)
            {
                enemy = enemyTarget.GetComponent<Enemy>();
                currentTarget = enemyTarget;
                isBusy = false;
                yield break; // Coroutine'yi iptal et
            }

            timer += 0.3f;
        }

        isBusy = false;
        SetTarget();
    }

    IEnumerator WaitAttack()
    {
        if (enemy.IsDead() || !enemy.gameObject.activeSelf)
        {
            ResetSettings();
        }
        else
        {
            isBusy = true;
            yield return new WaitForSeconds((1 / attackSpeed) + 1);
            isBusy = false;
            AttackTheEnemy();
            if (enemy != null && (enemy.IsDead() || !enemy.gameObject.activeSelf))
            {
                ResetSettings();
            }
        }
    }
    void Rotation()
    {
        if (currentTarget == null) return;

        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0; // Y ekseninde d�nmeyi engelle
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        //  Daha h�zl� d�n�� (10f)
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }

    private Transform FindClosestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, sightRange, enemyLayers);
        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider enemy in enemies)
        {
            if (enemy.transform == this.transform) continue; // Kendi kendini alg�lamas�n
            if (enemy.GetComponent<Enemy>().IsDead()) continue; // Enemy olmusse...
            Debug.Log("Find an enemey: " + enemy.name);
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }
    void AttackTheEnemy()
    {  
        if(enemy == null) return;
        animator.SetTrigger("Slash");
        enemy.SetDamage(damage);
    }
    void IfTargetIsEnemy()
    {
        if (agent.speed < defaultSpeed * 2)
            agent.speed += Time.deltaTime * 5;
    }
    bool IsEnemyTarget()
    {
        if (enemyTarget == null) return false;
        return currentTarget.name == enemyTarget.name;
    }
    public bool EnemyIsThis(Transform target)
    {
        return enemyTarget == target;
    }
    public void ResetSettings()
    {
        enemyTarget = null;
        enemy = null;
        SetTarget();
    }
}
