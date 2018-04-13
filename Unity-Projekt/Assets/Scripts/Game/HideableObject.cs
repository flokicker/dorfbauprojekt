using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideableObject : MonoBehaviour {

    public HashSet<int> personIDs = new HashSet<int>();
    public bool inBuildingViewRange, isHidden;

	private bool setup = false;

	// Use this for initialization
	public virtual void Start () {
	}
	
	// Update is called once per frame
	public virtual void Update () {
		if(!setup) 
		{
			setup = true;
			UpdateBuildingViewRange();
		}
	}

	public void UpdateBuildingViewRange()
	{
		inBuildingViewRange = false;
		foreach(BuildingScript bs in BuildingScript.allBuildings)
		{
			if(GameManager.InRange(transform.position, bs.transform.transform.position, bs.GetBuilding().viewRange))
			{
				inBuildingViewRange = true;
				break;
			}
		}
		ChangeHidden(!inBuildingViewRange && personIDs.Count == 0);
	}

	public void ChangeHidden(bool hidden)
	{
		isHidden = hidden;

		if(gameObject.activeSelf != !isHidden)
		{
			Node node = Grid.GetNodeFromWorld(transform.position);
			if(node)
			{
				node.gameObject.SetActive(true);
			}
		}
		gameObject.SetActive(!isHidden);
	}
}
