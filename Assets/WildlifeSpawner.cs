using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildlifeSpawner : MonoBehaviour
{
    public int SectorsX = 10;
    public int SectorsZ = 10;
    public int WildlifeAmountPerSector;
    public BoxCollider DistributionArea;
    public GameObject[] Prefabs;
    public Sector[,] Sectors;
    public int CollectablesAmountInSectorToPreventWildlifeSpawning = 1;
    public float SpawnFrequency;

    public List<GameObject> m_spawnedWildLife = new List<GameObject>();
    public int MaxWildlife;
    public float EntityTravelSpeed = 5.0f;

    [RangeBeginEnd(1f, 100f)]
    public RangeFloat WildlifeHeightAboveTerrain = new RangeFloat(0f, 10f);
    public LayerMask TerrainMask;

    public CollectablesDistributor CollectablesRegistry { get; private set; }
    private GameObject Submarine;

    void Awake()
    {
        Sectors = new Sector[SectorsX,SectorsZ];
        var bounds = DistributionArea.bounds;
        var corner1 = bounds.center - bounds.extents;
        float sectorUnitSizeX = bounds.size.x / SectorsX;
        float sectorUnitSizeZ = bounds.size.z / SectorsZ;

        for (int arrayx = 0; arrayx < SectorsX; arrayx++)
        {
            for (int arrayz = 0; arrayz < SectorsZ; arrayz++)
            {
                var sectorPos = new Vector3(
                    corner1.x+arrayx*sectorUnitSizeX, 
                    corner1.y,
                    corner1.z + arrayz * sectorUnitSizeZ);
                //Debug.LogFormat("{0},{1}", arrayx, arrayz);
                //HashSectorPosition(sectorPos, out var arrayx, out var arrayz);
                Sectors[arrayx, arrayz] = new Sector(
                    sectorPos,
                    sectorPos + new Vector3(sectorUnitSizeX, bounds.size.y, sectorUnitSizeZ),
                    SpawnFrequency * UnityEngine.Random.value);
            }
        }
        CollectablesRegistry = FindObjectOfType<CollectablesDistributor>();
        Submarine = FindObjectOfType<SubmarineController>().gameObject;
    }

    private void HashSectorPosition(Vector3 p, out int x, out int z)
    {
        var bounds = DistributionArea.bounds;
        p += bounds.extents;

        float sectorUnitScaleX = (float)SectorsX / bounds.size.x;
        float sectorUnitScaleY = (float)SectorsZ / bounds.size.z;
        p.Scale(new Vector3(sectorUnitScaleX,0f, sectorUnitScaleY));
        x = Mathf.FloorToInt(p.x);
        z = Mathf.FloorToInt(p.z);
        //Debug.LogFormat("{0},{1}", x, z);
    }

    void Update()
    {
        foreach (var sector in Sectors)
        {
            sector.Reset();
        }
        foreach (var spawned in CollectablesRegistry.SpawnedCollectables)
        {
            if (spawned == null) continue;
            HashSectorPosition(spawned.transform.position, out var arrayx, out var arrayz);
            if (IsBetween(arrayx, 0, SectorsX) && IsBetween(arrayz, 0, SectorsZ))
            {
                Sectors[arrayx, arrayz].CollectablesAmount++;
            }
        }

        foreach (var spawned in m_spawnedWildLife)
        {
            if (spawned == null) continue;
            spawned.transform.position += spawned.transform.forward * EntityTravelSpeed * Time.deltaTime;
            if(Physics.Raycast(spawned.transform.position + Vector3.up * 1000, Vector3.down, 
                hitInfo:out var hit,  maxDistance: 2000, layerMask: TerrainMask))
            {
                float hit_distance = hit.distance;
                if (!WildlifeHeightAboveTerrain.InRange(hit_distance))
                {
                    spawned.transform.position = Vector3.MoveTowards(spawned.transform.position, hit.point + Vector3.up * WildlifeHeightAboveTerrain.Clamp(hit_distance), EntityTravelSpeed * Time.deltaTime);
                }
            }
            HashSectorPosition(spawned.transform.position, out var arrayx, out var arrayz);
            if (!DistributionArea.bounds.Contains(spawned.transform.position))
            {
                Destroy(spawned);
                continue;
            }
            if (IsBetween(arrayx, 0, SectorsX) && IsBetween(arrayz, 0, SectorsZ))
            {
                Sectors[arrayx, arrayz].WildlifeAmount++;
            }
        }
        m_spawnedWildLife.RemoveAll(s => s == null);
        foreach (var sector in Sectors)
        {
            if(sector.CollectablesAmount < CollectablesAmountInSectorToPreventWildlifeSpawning 
                && sector.WildlifeAmount < WildlifeAmountPerSector
                && m_spawnedWildLife.Count < MaxWildlife
                && !sector.bounds.Contains(Submarine.transform.position))
            {
                sector.SpawnTimer -= Time.deltaTime;
                if (sector.SpawnTimer < 0)
                {
                    SpawnWildlifeInSector(sector);
                    sector.SpawnTimer = SpawnFrequency * UnityEngine.Random.value;
                }
            }
        }
    }

    public bool IsBetween(int testValue, int bound1, int bound2)
    {
        return (testValue >= Math.Min(bound1, bound2) && testValue < Math.Max(bound1, bound2));
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        foreach (var sector in Sectors)
        {
            Gizmos.color = sector.CollectablesAmount > 0 ? Color.red : Color.green;
            Gizmos.DrawWireCube(sector.bounds.center, sector.bounds.size*0.9f);
        }
    }

    private void SpawnWildlifeInSector(Sector sector)
    {
        m_spawnedWildLife.Add(CollectablesDistributor.SpawnRandomPrefabAtRandomPlaceInBounds(
            Prefabs.Length,Prefabs,sector.bounds,transform));
        //Debug.LogFormat("Spawning new wildlife in section {0}", sector.bounds);
    }

    public class Sector
    {
        public readonly Vector3 corner1;
        public readonly Vector3 corner2;
        public readonly Bounds bounds;
        public int CollectablesAmount;
        public int WildlifeAmount;
        public float SpawnTimer;

        public Sector(Vector3 corner1, Vector3 corner2, float spawnTimer)
        {
            this.corner1 = corner1;
            this.corner2 = corner2;
            this.bounds = new Bounds((corner1 + corner2) / 2f, corner2 - corner1);
            this.CollectablesAmount = 0;
            this.WildlifeAmount = 0;
            this.SpawnTimer = spawnTimer;
        }

        internal void Reset()
        {
            this.CollectablesAmount = 0;
            this.WildlifeAmount = 0;
        }
    }
}
