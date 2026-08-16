using UnityEngine;
using System.Collections.Generic;

// Object Pool สำหรับ FloatingDamageNumber
public class FloatingDamagePool : MonoBehaviour
{
    public static FloatingDamagePool Instance { get; private set; }

    [SerializeField] private GameObject floatingDamagePrefab;
    [SerializeField] private int poolSize = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Pre-spawn pool
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(floatingDamagePrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public void SpawnDamageNumber(Vector3 position, float damage, bool isCrit = false, bool isElement = false)
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Instantiate(floatingDamagePrefab, transform);
        }

        // สุ่มตำแหน่งนิดหน่อยไม่ให้ทับกัน
        Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
        obj.transform.position = position + offset + Vector3.up;
        obj.SetActive(true);

        var dmgNum = obj.GetComponent<FloatingDamageNumber>();
        dmgNum?.Initialize(damage, isCrit, isElement);

        // คืน pool หลัง lifetime
        StartCoroutine(ReturnToPool(obj, 1.5f));
    }

    System.Collections.IEnumerator ReturnToPool(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
