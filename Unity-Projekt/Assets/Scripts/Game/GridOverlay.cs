using UnityEngine;

using System.Collections;



public class GridOverlay : MonoBehaviour
{
	
	[ SerializeField ] private Transform _transform;
	
	[ SerializeField ] private Material _material;
	
	[ SerializeField ] public Vector2 _gridSize;
	
	[ SerializeField ] public int _rows;
	
	[ SerializeField ] public int _columns;
	
	
	
	void Start()
	{
		
		UpdateGrid();
		
	}
	
	
	
	public void UpdateGrid()
	{
		
		_transform.localScale = new Vector3( _gridSize.x, _gridSize.y, 1.0f );
		
		_material.SetTextureScale( "_MainTex", new Vector2( _columns, _rows ) );
		
	}
	
}
