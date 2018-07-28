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

	// Spawn a item with given itemdata
	public static void SpawnItem(ItemData itd)
	{
		if(itd.resAm == 0) return;
		GameObject prefab;
		if(itd.resId >= Instance.itemPrefabs.Count)
		{
			prefab = Instance.planeItemPrefab;
		}
		else
		{
			prefab = Instance.itemPrefabs[itd.resId];
		}
		GameObject go = (GameObject)Instantiate(prefab, itd.GetPosition(), itd.GetRotation(), Instance.itemParentTransform);
		Item it = go.AddComponent<Item>();
		it.SetItemData(itd);
		if(itd.resId >= Instance.itemPrefabs.Count)
		{
			go.transform.position += new Vector3(0,0.01f,0);
			go.GetComponent<MeshRenderer>().material.SetTexture("_MainTex",ResourceData.Get(itd.resId).icon.texture);
		}
	}

	// Spawn a item with given properites at worldPos
	public static void SpawnItem(int id, int amount, Vector3 worldPos)
	{
		ItemData itd = new ItemData();
		itd.resId = id;
		itd.resAm = amount;
		itd.SetPosition(worldPos);
		itd.SetRotation(Quaternion.Euler(0,Random.Range(0,360),0));
		SpawnItem(itd);
	}
}
