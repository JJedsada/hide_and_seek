using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JarManager : MonoBehaviour
{
    [SerializeField] private JarController[] jars = new JarController[9];

    private Dictionary<int, JarController> jarDict = new Dictionary<int, JarController>();

    public void Initialize()
    {
        for (int i = 0; i < jars.Length; i++)
        {
            jars[i].JarId = i + 1;
            jars[i].Setup();
            jarDict.Add(jars[i].JarId, jars[i]);
        }
    }

    public void SetDefalt()
    {
        for (int i = 0; i < jars.Length; i++)
        {
            jars[i].Setup();
        }
    }

    public JarController GetJar(int jarId)
    {
        return jarDict[jarId];
    }
}
