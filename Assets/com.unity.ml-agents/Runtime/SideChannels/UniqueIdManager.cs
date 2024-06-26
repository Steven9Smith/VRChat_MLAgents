using UdonSharp;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class UniqueIdManager : UdonSharpBehaviour
{
    //private static int counter = 0;
    private static HashSet<string> uniqueIds = new HashSet<string>();

    /* // Generates a new unique ID
     public static string GenerateUniqueId()
     {
         string uniqueId;
         do
         {
             counter++;
             uniqueId = "UID_" + counter.ToString() + "_" + System.DateTime.Now.Ticks.ToString();
         } while (uniqueIds.Contains(uniqueId));

         uniqueIds.Add(uniqueId);
         return uniqueId;
     }*/
    // Generates a new unique GUID
    public static string GenerateUniqueId()
    {
        string uniqueId;
        do
        {
            uniqueId = System.Guid.NewGuid().ToString();
        } while (uniqueIds.Contains(uniqueId));

        uniqueIds.Add(uniqueId);
        return uniqueId;
    }

    // Converts the unique ID to a byte array
    public static byte[] ToByteArray(string uniqueId)
    {
        return Encoding.UTF8.GetBytes(uniqueId);
    }

    // Checks if an ID is unique
    public bool IsUniqueId(string id)
    {
        return uniqueIds.Contains(id);
    }

    // Removes an ID (useful if an object is destroyed or no longer needs to be tracked)
    public void RemoveUniqueId(string id)
    {
        uniqueIds.Remove(id);
    }

    void Start()
    {
        string uniqueId = GenerateUniqueId();
        Debug.Log("Generated Unique ID: " + uniqueId);

        bool isUnique = IsUniqueId(uniqueId);
        Debug.Log("Is Unique: " + isUnique);
    }
}