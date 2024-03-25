using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JarManager : MonoBehaviour
{
    [SerializeField] public ParticleSystem StarfallVfxPrefab;
    [SerializeField] private JarController[] jars = new JarController[9];

    private Dictionary<int, JarController> jarDict = new Dictionary<int, JarController>();

    [SerializeField] private GameObject selection;

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
        ShowInteraction(null);
    }

    public void ClearSpray()
    {
        for (int i = 0; i < jars.Length; i++)
        {
            jars[i].ClearSpray();
        }
    }

    public void ScanAll()
    {
        for (int i = 0; i < jars.Length; i++)
        {
            jars[i].Scan();
        }
    }

    public void ClearScan()
    {
        for (int i = 0; i < jars.Length; i++)
        {
            jars[i].ClearScan();
        }
    }

    public JarController GetJar(int jarId)
    {
        return jarDict[jarId];  
    }

    public List<JarController> JarAlive()
    {
        List<JarController> jarAlive = new List<JarController>();
        for (int i = 0; i < jars.Length; i++)
        {
            var jar = jars[i];
            if (jar.IsBreaked)
                continue;
            jarAlive.Add(jar);
        }

        return jarAlive;
    }

    public void ShowInteraction(Collider collider)
    {
        Vector3 offset = new Vector3(0, 0.1f, 0);
        UIManager.Instance.GameplayPanel.SetupInteractButton(collider);
        if (collider)
        {
            selection.transform.position = collider.transform.position + offset;
            if (!selection.activeSelf)
                selection.SetActive(true);
        }
        else
        {
            if (selection.activeSelf)
                selection.SetActive(false);
        }
    }

    public async UniTask Starfall(int jarId)
    {
        var particle = Instantiate(StarfallVfxPrefab);
        particle.transform.position = GetJar(jarId).transform.position + Vector3.up;

        await UniTask.Delay(2000);

        Destroy(particle);
    }
}
