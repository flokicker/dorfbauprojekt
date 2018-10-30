using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideableObject : MonoBehaviour {

    public HashSet<int> personIDs = new HashSet<int>();
    public bool inBuildingViewRange, isHidden;

    private Transform modelParent;
	protected bool setup = false, destroyed = false, onlyNoRenderOnHide = false;

	// Use this for initialization
	public virtual void Start ()
    {
        destroyed = false;
        if (transform.childCount > 0) modelParent = transform.GetChild(0);
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

        ClickableObject co = GetComponent<ClickableObject>();
        if (!co) co = GetComponentInChildren<ClickableObject>();

        // make sure that this object is not selected when hiding
        if (UIManager.Instance && UIManager.Instance.IsTransformSelected(transform) && hidden) UIManager.Instance.OnHideObjectInfo();
        if (co && hidden)
        {
            co.outlined = false;
            co.UpdateSelectionCircleMaterial();
        }

        if (onlyNoRenderOnHide && modelParent)
            modelParent.gameObject.SetActive(!isHidden);
        else
        {
            gameObject.SetActive(!isHidden);
            if (this is AnimalScript) Debug.Log("gameObj set to inactive");
        }
	}
}
