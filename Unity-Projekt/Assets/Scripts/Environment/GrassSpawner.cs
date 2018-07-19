using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjData
{
    public Vector3 pos;
    public Vector3 scale;
    public Quaternion rot;

    public Matrix4x4 matrix
    {
        get
        {
            return Matrix4x4.TRS(pos, rot, scale);
        }
    }

    public ObjData(Vector3 pos, Vector3 scale, Quaternion rot)
    {
        this.pos = pos;
        this.scale = scale;
        this.rot = rot;
    }
}
public class GrassSpawner : MonoBehaviour {

    public int instances;
    public Mesh grassMesh;
    public Material grassMaterial;

    private List<List<ObjData>> batches = new List<List<ObjData>>();

	// Use this for initialization
	void Start () {
        int batchIndexNum = 0;
        List<ObjData> currBatch = new List<ObjData>();
        for(int i = 0; i <= instances; i++)
        {
            AddObj(currBatch, i);
            batchIndexNum++;
            if(batchIndexNum >= 1000 || i == instances)
            {
                batches.Add(currBatch);
                currBatch = BuildNewBatch();
                batchIndexNum = 0;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        RenderBatches();
	}

    private void AddObj(List<ObjData> currBatch, int i)
    {
        Vector3 position = new Vector3(Random.Range(-1f, 1f) * Grid.SCALE * 50, 1, Random.Range(-1f, 1f) * Grid.SCALE * 50);
        currBatch.Add(new ObjData(position, Vector3.one, Quaternion.identity));
    }

    private List<ObjData> BuildNewBatch()
    {
        return new List<ObjData>();
    }

    private void RenderBatches()
    {
        foreach(var batch in batches)
        {
            Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, batch.Select((a) => a.matrix).ToList());
        }
    }
}
