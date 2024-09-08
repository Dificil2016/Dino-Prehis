using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    public IEnumerator UpdateHP(float newhp)
    {
        float currHp = health.transform.localScale.x;
        float changeAmount = currHp - newhp;

        while (currHp - newhp > Mathf.Epsilon)
        {
            currHp -= changeAmount * Time.deltaTime;
            health.transform.localScale = new Vector3(currHp, 1f);
            yield return null;
        }
        health.transform.localScale = new Vector3(newhp, 1f);
    }
}
