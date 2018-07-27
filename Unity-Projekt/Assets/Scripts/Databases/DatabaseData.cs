using UnityEngine;

[System.Serializable]
public class DatabaseData : ScriptableObject {

    // Identifier is unique for a specific group of database-data
    public int id;
    // Display name
    public new string name;
}
