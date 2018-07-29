using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : Singleton<ItemManager> {

    [SerializeField]
    private Transform itemParentTransform;
    [SerializeField]
    private List<GameObject> itemPrefabs;
	[SerializeField]
	private GameObject planeItemPrefab;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static void SpawnItem(int id, int am, Vector3 pos, float randX, float randY)
    {
        GameItem toSpawn = new GameItem(id, am);
        toSpawn.SetPosition(pos +
            new Vector3(Random.Range(-randX, randX), 0, Random.Range(-randY, randY)) * Grid.SCALE * 0.8f);
        toSpawn.SetRotation(Quaternion.Euler(0,Random.Range(0,360),0));
        SpawnItem(toSpawn);
    }

	// Spawn a item with given itemdata
	public static void SpawnItem(GameItem gameItem)
	{
		if(gameItem.Amount == 0) return;
		GameObject prefab;
		if(gameItem.resource.models.Count == 0)
		{
			prefab = Instance.planeItemPrefab;
		}
		else
		{
            prefab = gameItem.resource.models[gameItem.variation].gameObject;
		}

		GameObject go = (GameObject)Instantiate(prefab, gameItem.GetPosition(), gameItem.GetRotation(), Instance.itemParentTransform);

        ItemScript it = go.AddComponent<ItemScript>();
		it.SetItem(gameItem);

		if(gameItem.resource.models.Count == 0)
		{
			go.transform.position += new Vector3(0,0.01f,0);
			go.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", gameItem.resource.icon.texture);
		}
	}
}
