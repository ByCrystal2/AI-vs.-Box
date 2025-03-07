using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public float spawnRadius;
    [Header("AI Settings")]
    [SerializeField] Transform aiContent;
    [SerializeField] AIMovement aiPrefab;

    [Header("Enemy Settings")]
    [SerializeField] Transform enemyContent;
    [SerializeField] Enemy enemyPrefab;

    [Header("First Spawn Counts")]
    [SerializeField] int enemiesSpawnCount;
    [SerializeField] int aiSpawnCount;
    public bool drawGizmos = false;
    public static SpawnManager instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        SpawnEnemies(enemiesSpawnCount);
        SpawnAIs(aiSpawnCount);
    }
    private void Update()
    {
        BugController();
    }
    public void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetRandomPosition();
            Enemy newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, enemyContent);
            newEnemy.name += "_" + System.Guid.NewGuid().ToString().Substring(0,5);
        }
    }

    public void SpawnAIs(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetRandomPosition();
            AIMovement newAi = Instantiate(aiPrefab, spawnPosition, Quaternion.identity, aiContent);
            newAi.name += "_" + System.Guid.NewGuid().ToString().Substring(0, 5);
        }
    }

    Vector3 GetRandomPosition()
    {
        Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
        return new Vector3(randomPoint.x, 0.5f, randomPoint.y); // Y eksenini sýfýrda tutuyoruz
    }
    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
    float controlTime = 0.5f;
    float time = 0f;
    void BugController()
    {        
        if (time < controlTime)
        {
            time += Time.deltaTime;
            return;
        }
        time = 0f;
        int length = enemyContent.childCount;

        for (int i = 0; i < length; i++)
        {
            Transform enemyTrans = enemyContent.GetChild(i);
            if (enemyTrans.position.y <= -1.5f)
            {
                FaultyAIResolution(enemyTrans);
                Destroy(enemyTrans.gameObject,0.5f);
                SpawnEnemies(1);
            }
        }
    }
    public void FaultyAIResolution(Transform enemy)
    {
        int length = aiContent.childCount;
        for (int i = 0; i < length; i++)
        {
            Transform obj = aiContent.GetChild(i);
            if (obj.TryGetComponent(out AIMovement ai))
            {
                if (ai.EnemyIsThis(enemy))
                {
                    ai.ResetSettings();
                }
            }
        }
    }
}
