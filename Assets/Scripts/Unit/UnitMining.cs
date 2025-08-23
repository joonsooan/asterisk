using System;
using System.Collections;
using UnityEngine;

public class UnitMining : MonoBehaviour
{
    [Header("Gathering Stats")]
    public int miningRate = 10;
    public float miningRange = 1.5f;

    private Coroutine _mineCoroutine;
    private WaitForSeconds _miningDelay;
    private ResourceNode _targetResourceNode;

    private void Awake()
    {
        _miningDelay = new WaitForSeconds(1.0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, miningRange);
    }

    public event Action<ResourceType, int> OnResourceMined;

    public void StartMining(ResourceNode target)
    {
        _targetResourceNode = target;
        if (_mineCoroutine == null) {
            _mineCoroutine = StartCoroutine(MineResourceCoroutine());
        }
    }

    public void StopMining()
    {
        if (_mineCoroutine != null) {
            StopCoroutine(_mineCoroutine);
            _mineCoroutine = null;
        }
    }

    private IEnumerator MineResourceCoroutine()
    {
        yield return _miningDelay;

        while (true) {
            if (_targetResourceNode != null && !_targetResourceNode.IsDepleted) {
                int minedAmount = _targetResourceNode.Mine(miningRate);
                OnResourceMined?.Invoke(_targetResourceNode.resourceType, minedAmount);
            }
            else {
                StopMining();
                yield break;
            }
            yield return _miningDelay;
        }
    }
}
