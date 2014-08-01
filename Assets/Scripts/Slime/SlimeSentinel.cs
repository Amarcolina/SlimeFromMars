using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlimeSentinel : MonoBehaviour {
    private static List<Slime> _slimesToDestroy = new List<Slime>();
    private static Dictionary<Slime, int> _slimeIndexMap = new Dictionary<Slime, int>();
    private static bool _isRunningDestroyCoroutine = false;
    private static SlimeSentinel _sentinelInstance = null;

    public static void addSlimeToDestroyList(Slime slime) {
        _slimeIndexMap[slime] = _slimesToDestroy.Count;
        _slimesToDestroy.Add(slime);

        if (_sentinelInstance == null) {
            GameObject sentinelObj = new GameObject("Slime Sentinel");
            _sentinelInstance = sentinelObj.AddComponent<SlimeSentinel>();
        }

        if (!_isRunningDestroyCoroutine) {
            _sentinelInstance.StartCoroutine("destroySlimeCoroutine");
        }
    }

    public static void removeSlimeFromDestroyList(Slime slime) {
        int index = _slimeIndexMap[slime];
        Slime topSlime = _slimesToDestroy[_slimesToDestroy.Count - 1];
        _slimesToDestroy[index] = topSlime;
        _slimeIndexMap[topSlime] = index;
        _slimesToDestroy.RemoveAt(_slimesToDestroy.Count - 1);
    }

    private IEnumerator destroySlimeCoroutine() {
        _isRunningDestroyCoroutine = true;
        while (_slimesToDestroy.Count > 1) {
            yield return new WaitForSeconds(1.0f);
            if (_slimesToDestroy.Count != 0) {
                int randomIndex = Random.Range(0, _slimesToDestroy.Count);
                Slime slimeToDestroy = _slimesToDestroy[randomIndex];
                removeSlimeFromDestroyList(slimeToDestroy);
                slimeToDestroy.damageSlime(1.0f);
            }
        }
        _isRunningDestroyCoroutine = false;
    }
}
