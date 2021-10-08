using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public class Collectable : MonoBehaviour
{
    public CollectablesDistributor Distributor;

    private void OnDestroy()
    {
        //Relying on the null out on destruction behaviour has timing issues. Unity can cleanup objects whenever it feels like it
        Distributor?.SpawnedCollectables.QuickRemove(this);
        PlayerState.Instance?.CollecedItem();
    }
}
