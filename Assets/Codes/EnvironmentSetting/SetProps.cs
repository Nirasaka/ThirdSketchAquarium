using UnityEngine;
using Meta.XR.MRUtilityKit;
using NUnit.Framework;
using System.Collections.Generic;

public class SetProps : MonoBehaviour
{
    public int numOfProps = 10;
    public int maxNumOfError = 20;
    public List<GameObject> prefabs;
    public MRUKAnchor.SceneLabels labels;

    private GameObject parent;

    public void Start()
    {
        parent = new GameObject("Props");
    }

    public void OnSpaceReady()
    {
        var currentRoom = MRUK.Instance.GetCurrentRoom();
        
        LabelFilter labelFilter = new LabelFilter(labels);
        Vector3 pos;
        Vector3 normal;

        int i = 0;
        int error = 0;
        while (i < numOfProps || error < maxNumOfError)
        {
            if (currentRoom.GenerateRandomPositionOnSurface(MRUK.SurfaceType.FACING_UP, 0.05f, labelFilter, out pos, out normal))
            {
                if (currentRoom.IsPositionInRoom(pos) && !currentRoom.IsPositionInSceneVolume(pos))
                {
                    Instantiate(prefabs[ i % 3 ], pos, Quaternion.identity, parent.transform);
                    //Debug.Log("配置完了");
                    i++;
                }
                else
                {
                    //Debug.LogError("場所が部屋内ではなかったため配置失敗");
                    error++;
                }
            }
            else
            {
                //Debug.LogError("置き場がなかったため配置失敗");
                error++;
            }
        }
    }

    public void DeleteAllProps()
    {
        for(int i = parent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.transform.GetChild(i).gameObject);
        }
        parent.transform.DetachChildren();
    }
}
