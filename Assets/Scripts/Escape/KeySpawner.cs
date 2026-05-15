using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KeySpawner : MonoBehaviour
{
    [Header("Key Prefabs")]
    public GameObject keyRedPrefab;
    public GameObject keyGreenPrefab;
    public GameObject keyBluePrefab;

    [Header("Spawn Area")]
    public float areaMinX = -50f;
    public float areaMaxX =  30f;
    public float areaMinZ =  -1f;
    public float areaMaxZ =  43f;

    [Header("Settings")]
    public float heightOffset       = 1.2f;
    public float navSampleRadius    = 5f;
    public float minDistBetweenKeys = 12f;
    public float minDistFromSpawn   = 8f;
    public int   maxAttempts        = 200;

    [Header("Height Filter")]
    public float maxGroundY = 2.0f;
    public float playerEyeHeight = 1.8f;
    public LayerMask obstacleMask = ~0;

    private static readonly Vector3 SpawnPos = new Vector3(-44f, 3.56f, 10f);

    IEnumerator Start()
    {
        // NavMesh + NeoFPS 초기화 완료 대기
        yield return new WaitForSeconds(1.5f);

        // ==========================================
        // [추가] 맵에 열쇠를 스폰하기 전에, 은신처의 세이브 데이터를 불러옵니다.
        // ==========================================
        EscapeItemRegistry.RestoreSavedKeys();

        SpawnKeys();
    }

    void SpawnKeys()
    {
        var prefabs = new[] { keyRedPrefab, keyGreenPrefab, keyBluePrefab };
        var placed  = new List<Vector3>();

        foreach (var prefab in prefabs)
        {
            if (prefab == null) { Debug.LogWarning("[KeySpawner] 프리팹 없음"); continue; }

            // ==========================================
            // [추가] 이미 세이브된(보유한) 열쇠라면 맵에 스폰하지 않고 패스!
            // ==========================================
            KeyPickup keyLogic = prefab.GetComponent<KeyPickup>();
            if (keyLogic != null && EscapeItemRegistry.Has(keyLogic.keyId))
            {
                Debug.Log($"[KeySpawner] '{keyLogic.displayName}'은(는) 이미 보유 중이므로 스폰을 생략합니다.");
                continue;
            }

            Vector3 pos = FindPoint(placed);
            if (pos == Vector3.zero)
            {
                pos = FallbackPoint(placed); // NavMesh 실패 시 Raycast fallback
            }

            if (pos == Vector3.zero) { Debug.LogWarning($"[KeySpawner] {prefab.name} 배치 실패"); continue; }

            placed.Add(pos);
            var go = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            go.name = prefab.name;
            Debug.Log($"[KeySpawner] {prefab.name} -> {pos}");
        }
    }

    Vector3 FindPoint(List<Vector3> placed)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            float rx = Random.Range(areaMinX, areaMaxX);
            float rz = Random.Range(areaMinZ, areaMaxZ);

            if (!NavMesh.SamplePosition(new Vector3(rx, 50f, rz), out NavMeshHit hit, navSampleRadius, NavMesh.AllAreas))
                continue;

            if (hit.position.y > maxGroundY) continue;

            Vector3 pos = hit.position + Vector3.up * heightOffset;

            if (Physics.Raycast(pos, Vector3.up, playerEyeHeight, obstacleMask)) continue;

            if (!Valid(pos, placed)) continue;
            return pos;
        }
        return Vector3.zero;
    }

    Vector3 FallbackPoint(List<Vector3> placed)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            float rx = Random.Range(areaMinX, areaMaxX);
            float rz = Random.Range(areaMinZ, areaMaxZ);
            Ray ray = new Ray(new Vector3(rx, 100f, rz), Vector3.down);

            if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) continue;

            if (hit.point.y > maxGroundY) continue;

            Vector3 pos = hit.point + Vector3.up * heightOffset;

            if (Physics.Raycast(pos, Vector3.up, playerEyeHeight, obstacleMask)) continue;

            if (!Valid(pos, placed)) continue;
            return pos;
        }
        return Vector3.zero;
    }

    bool Valid(Vector3 pos, List<Vector3> placed)
    {
        if (Vector3.Distance(pos, SpawnPos) < minDistFromSpawn) return false;
        foreach (var p in placed)
            if (Vector3.Distance(pos, p) < minDistBetweenKeys) return false;
        return true;
    }
}