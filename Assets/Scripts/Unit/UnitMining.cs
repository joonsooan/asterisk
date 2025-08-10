using System.Collections;
using UnityEngine;

public class UnitMining : MonoBehaviour
{
    [Header("Gathering Stats")]
    public int miningRate = 10;
    public float miningRange = 1.5f;
    private Coroutine _mineCoroutine;
    private ResourceNode _targetResourceNode;

    public bool IsMining {
        get {
            return _mineCoroutine != null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, miningRange);
    }

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
        yield return new WaitForSeconds(1.0f);

        while (true) {
            if (_targetResourceNode != null && !_targetResourceNode.IsDepleted) {
                _targetResourceNode.Mine(miningRate);
                Debug.Log($"자원 채굴 중... 남은 양: {_targetResourceNode.amountToMine}");
            }
            else {
                StopMining();
                yield break;
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
}
