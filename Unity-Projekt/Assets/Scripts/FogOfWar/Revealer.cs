using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revealer : MonoBehaviour {

	public int sight;

	private void Start()
	{
		FogOfWar.Instance.RegisterRevealer(this);
	}
}
