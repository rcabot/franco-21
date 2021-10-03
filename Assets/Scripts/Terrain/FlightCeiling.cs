using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
class FlightCeiling : MonoBehaviour
{
    private void Start()
    {
        TerrainManager terrain_manager = TerrainManager.Instance;
        if (terrain_manager != null)
        {
            Rect terrain_bounds = terrain_manager.TerrainAreaWithEdge;
            BoxCollider collider = GetComponent<BoxCollider>();

            collider.size = terrain_bounds.size.XZ() + Vector3.up;
            collider.center = Vector3.up * 0.5f;
            transform.position = terrain_bounds.center.XZ() + Vector3.up * terrain_manager.Definition.MaxHeight;
        }

        Destroy(this);
    }
}
