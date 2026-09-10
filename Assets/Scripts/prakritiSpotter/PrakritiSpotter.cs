using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrakritiSpotter : MonoBehaviour
{
    public List<GameObject> moles;
    public Vector2 spawnDelay = new Vector2(0.5f, 1.5f);
    public Vector2 visibleTime = new Vector2(0.8f, 1.5f);

    void Start()
    {
        foreach (var m in moles)
        {
            m.SetActive(false);
        }

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnDelay.x, spawnDelay.y));

            GameObject mole = moles[Random.Range(0, moles.Count)];

            if (!mole.activeSelf)
                StartCoroutine(ShowMole(mole));
        }
    }

    IEnumerator ShowMole(GameObject mole)
    {
        mole.SetActive(true);

        Prakriti prakriti = mole.GetComponent<Prakriti>();
        if (prakriti != null)
            prakriti.PopIn();

        yield return new WaitForSeconds(Random.Range(visibleTime.x, visibleTime.y));

        if (mole.activeSelf)
            mole.SetActive(false);
    }
}