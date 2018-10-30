using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HerdCenter : MonoBehaviour
{
    public int animalId;
    public int animalCount = 0;

    private Projector selectionCircle;

    private HashSet<int> selAnimals = new HashSet<int>();

    void Start()
    {
        selectionCircle = Instantiate(GameManager.Instance.selectionCirclePrefab, transform).GetComponent<Projector>();
        selectionCircle.material = new Material(selectionCircle.material);

        float radius = Mathf.Max(0.001f, Animal.Get(animalId).maxDistFromHerdCenter);
        if (selectionCircle)
        {
            selectionCircle.orthographicSize = radius;
            selectionCircle.material.SetFloat("_Radius", 0.25f);
            selectionCircle.material.SetFloat("_Border", 1f / radius * 0.01f);
        }

        selectionCircle.enabled = false;
    }

    public void SetAnimalSelected(int id, bool sel)
    {
        if (sel)
            selAnimals.Add(id);
        else selAnimals.Remove(id);

        ShowRange(selAnimals.Count > 0);
    }

    public void ShowRange(bool show)
    {
        selectionCircle.enabled = show;
    }
}
