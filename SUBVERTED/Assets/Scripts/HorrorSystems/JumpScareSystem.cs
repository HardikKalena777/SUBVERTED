using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

public class JumpScareSystem : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPointData
    {
        public Transform spawnPoint;
        public int spawnCount;
        public bool isEnabled = true;
        public float proximityThreshold;
    }

    [Header("Jump Scare Settings")]
    public GameObject characterPrefab;
    public SpawnPointData[] spawnPoints;
    public float delayBetweenSpawns = 0f;
    public Transform hidingSpot;
    public bool loopAnimation = false;
    public Transform player;
    public float animationDuration = 1f;

    [Header("GlitchEffect Settings")]
    public Material[] material;
    public float glitchEffectDuration = 0.5f;
    public float glitchDuration = 0.3f;
    public string glitchPropertyName = "_GlitchToggle";

    private int totalSpawns;
    private float totalDuration;
    private CancellationTokenSource cts;
    private GameObject currentCharacter;
    private Animator characterAnimator;
    private bool hasStarted = false;
    private float timer;
    private float remainingTime;
    private HashSet<SpawnPointData> recentlyUsedSpawnPoints = new HashSet<SpawnPointData>();

    void Start()
    {
        CreateAndHideCharacter();
        CalculateTotalValues();
        StartJumpScareLoop().Forget();
    }

    void Update()
    {
        if (hasStarted)
        {
            timer += Time.deltaTime;
        }
        CheckPlayerProximity();
    }

    public void PlayGlitchEffect()
    {
        ApplyGlitchEffect().Forget();
    }

    public async UniTask ApplyGlitchEffect()
    {
        ToggleGlitch(true);
        await UniTask.Delay((int)(glitchDuration * 1000));
        ToggleGlitch(false);
    }

    private void ToggleGlitch(bool state)
    {
        foreach (var mat in material)
        {
            if (mat.HasProperty(glitchPropertyName))
            {
                mat.SetFloat(glitchPropertyName, state ? 1f : 0f);
            }
        }
    }

    private void CreateAndHideCharacter()
    {
        currentCharacter = Instantiate(characterPrefab);
        currentCharacter.transform.position = hidingSpot.position;
        characterAnimator = currentCharacter.GetComponent<Animator>();
    }

    private void CalculateTotalValues()
    {
        totalSpawns = 0;
        totalDuration = 0f;

        foreach (var spawn in spawnPoints)
        {
            if (spawn.isEnabled)
            {
                totalSpawns += spawn.spawnCount;
                totalDuration += spawn.spawnCount * animationDuration;
            }
        }

        remainingTime = totalDuration;
    }

    private async UniTaskVoid StartJumpScareLoop()
    {
        hasStarted = true;
        cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        int remainingSpawns = totalSpawns;

        while (remainingSpawns > 0 && !token.IsCancellationRequested)
        {
            SpawnPointData spawnData = GetRandomValidSpawnPoint();

            if (spawnData != null)
            {
                MoveCharacterToSpawnPoint(spawnData.spawnPoint);

                if (characterAnimator != null)
                {
                    characterAnimator.SetBool("isPlaying", true);
                }

                await UniTask.Delay((int)(animationDuration * 1000), cancellationToken: token);

                if (!loopAnimation && characterAnimator != null)
                {
                    characterAnimator.SetBool("isPlaying", false);
                }

                spawnData.spawnCount--;
                remainingSpawns--;

                Debug.Log($"{spawnData.spawnPoint.name} - Remaining Spawns: {spawnData.spawnCount}");

                if (remainingSpawns > 0)
                {
                    await UniTask.Delay((int)(delayBetweenSpawns * 1000), cancellationToken: token);
                }

                RedistributeSpawnsIfNecessary();
                
                foreach (var spawnPoint in spawnPoints)
                {
                    if (spawnPoint.isEnabled && spawnPoint.spawnCount == 0)
                    {
                        if (player.transform != null && 
                            Vector3.Distance(player.transform.position, spawnPoint.spawnPoint.position) > spawnPoint.proximityThreshold)
                        {
                            spawnPoint.spawnCount = Mathf.Max(1, remainingSpawns / 4);
                            Debug.Log($"Restored spawns for {spawnPoint.spawnPoint.name}: {spawnPoint.spawnCount}");
                        }
                    }
                }
            }
            else
            {
                break;
            }
        }

        MoveCharacterToHidingSpot();
    }

    private SpawnPointData GetRandomValidSpawnPoint()
    {
        List<SpawnPointData> validSpawnPoints = spawnPoints
            .Where(spawn => spawn.isEnabled && spawn.spawnCount > 0 && !recentlyUsedSpawnPoints.Contains(spawn))
            .ToList();

        if (validSpawnPoints.Count == 0)
        {
            recentlyUsedSpawnPoints.Clear();
            validSpawnPoints = spawnPoints
                .Where(spawn => spawn.isEnabled && spawn.spawnCount > 0)
                .ToList();
        }

        if (validSpawnPoints.Count > 0)
        {
            var selectedSpawn = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
            recentlyUsedSpawnPoints.Add(selectedSpawn);
            return selectedSpawn;
        }

        return null;
    }

    private async void MoveCharacterToSpawnPoint(Transform spawnPoint)
    {
        if (currentCharacter != null)
        {
            PlayGlitchEffect();
            await UniTask.Delay((int)(glitchEffectDuration * 1000));
            currentCharacter.transform.position = spawnPoint.position;
            currentCharacter.transform.rotation = spawnPoint.rotation;
        }
    }

    private void MoveCharacterToHidingSpot()
    {
        if (currentCharacter != null && hidingSpot != null)
        {
            Debug.Log("Timer: " + timer);
            foreach (var spawnPoint in spawnPoints)
            {
                spawnPoint.spawnCount = 0;
            }
            hasStarted = false;
            currentCharacter.transform.position = hidingSpot.position;
        }
    }

    private void RedistributeSpawnsIfNecessary()
    {
        List<SpawnPointData> disabledSpawns = spawnPoints.Where(spawn => !spawn.isEnabled && spawn.spawnCount > 0).ToList();
        if (disabledSpawns.Count == 0) return;

        List<SpawnPointData> validSpawns = spawnPoints.Where(spawn => spawn.isEnabled).ToList();
        if (validSpawns.Count == 0) return;

        foreach (var disabledSpawn in disabledSpawns)
        {
            while (disabledSpawn.spawnCount > 0)
            {
                var targetSpawn = validSpawns.OrderBy(spawn => spawn.spawnCount).FirstOrDefault();
                if (targetSpawn != null)
                {
                    targetSpawn.spawnCount++;
                    disabledSpawn.spawnCount--;
                }
            }
        }

        RecalculateRemainingTime();
    }

    private void RecalculateRemainingTime()
    {
        float remainingSpawnTime = 0f;

        foreach (var spawn in spawnPoints)
        {
            if (spawn.isEnabled)
            {
                remainingSpawnTime += spawn.spawnCount * animationDuration;
            }
        }

        remainingTime = Mathf.Max(remainingSpawnTime, 0);
    }

    private void CheckPlayerProximity()
    {
        if (player == null) return;
        
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.spawnPoint != null)
            {
                float distance = Vector3.Distance(player.position, spawnPoint.spawnPoint.position);
                
                if (distance <= spawnPoint.proximityThreshold)
                {
                    if (currentCharacter != null && 
                        Vector3.Distance(currentCharacter.transform.position, spawnPoint.spawnPoint.position) < 0.1f)
                    {
                        SpawnPointData newSpawnPoint = GetRandomValidSpawnPoint();
                        if (newSpawnPoint != null)
                        {
                            MoveCharacterToSpawnPoint(newSpawnPoint.spawnPoint);
                            Debug.Log($"Character moved to {newSpawnPoint.spawnPoint.name} due to player proximity");
                        }
                    }
                    spawnPoint.isEnabled = false;
                }
                else
                {
                    spawnPoint.isEnabled = true;
                }
            }
        }
    }
}
