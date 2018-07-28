using UnityEngine;

[System.Serializable]
public class DatabaseData : ScriptableObject {

    // Identifier is unique for a specific group of database-data
    public int id;
    // Display name
    public new string name;
    
    // Sort database lists by their id
    public static int SortById(DatabaseData d1, DatabaseData d2)
    {
        return d1.id.CompareTo(d2.id);
    }
}
