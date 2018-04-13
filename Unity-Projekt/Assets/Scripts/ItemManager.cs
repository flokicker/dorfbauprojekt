﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : Singleton<ItemManager> {

    [SerializeField]
    private Transform itemParentTransform;
    [SerializeField]
    private List<GameObject> itemPrefabs;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Spawn a item with given properites at worldPos
	public static void SpawnItem(int id, int amount, Vector3 worldPos)
	{
		GameObject go = (GameObject)Instantiate(Instance.itemPrefabs[id], worldPos, Quaternion.Euler(0,Random.Range(0,360),0), Instance.itemParentTransform);
		Item it = go.AddComponent<Item>();
		it.Set(id, amount);
	}
}