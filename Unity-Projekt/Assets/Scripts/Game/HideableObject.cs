using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideableObject : MonoBehaviour {

    public HashSet<int> personIDs = new HashSet<int>();
    public bool inBuildingViewRange, isHidden;

	private bool setup = false, destroyed = false;

	// Use this for initialization
	public virtual void Start () {
	}
	
	public virtual void OnDestroy()
	{
		destroyed = true;
	}

	// Update is called once per frame
	public virtual void Update () {
		if(!setup) 
		{
			setup = true;
			UpdateBuildingViewRange();

            foreach (PersonScript ps in PersonScript.allPeople)
                ps.CheckHideableObject(this, transform);
        }
	}

	public void UpdateBuildingViewRange()
	{
		inBuildingViewRange = false;
		foreach(BuildingScript bs in BuildingScript.allBuildingScripts)
		{
			if(GameManager.InRange(transform.position, bs.transform.transform.position, bs.ViewRange))
			{
				inBuildingViewRange = true;
				break;
			}
		}
		ChangeHidden(!inBuildingViewRange && personIDs.Count == 0);
	}

	public virtual void ChangeHidden(bool hidden)
	{
		if(destroyed || !gameObject) return;

		isHidden = hidden;

		gameObject.SetActive(!isHidden);
	}
}
