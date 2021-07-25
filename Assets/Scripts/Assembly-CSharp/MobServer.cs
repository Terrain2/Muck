using System.Collections.Generic;
using UnityEngine;

public abstract class MobServer : MonoBehaviour
{
    protected Mob mob;

    private float syncPositionInterval = 2f;

    protected float FindPositionInterval = 0.5f;

    protected float behaviourInterval = 0.1f;

    protected float[] findPositionInterval = new float[3] { 0.5f, 2f, 5f };

    protected int previousTargetId = -1;

    private void Awake()
    {
        mob = GetComponent<Mob>();
    }

    protected void StartRoutines()
    {
        InvokeRepeating(nameof(SyncPosition), Random.Range(0f, syncPositionInterval), syncPositionInterval);
        Invoke(nameof(SyncFindNextPosition), Random.Range(0f, FindPositionInterval) + findPositionInterval[0]);
        InvokeRepeating(nameof(Behaviour), Random.Range(0f, behaviourInterval) + mob.mobType.spawnTime, behaviourInterval);
    }

    private void Update()
    {
        if (mob.ready)
        {
            Behaviour();
        }
    }

    protected abstract void Behaviour();

    public abstract void TookDamage();

    private void SyncPosition()
    {
        using (Dictionary<int, PlayerManager>.ValueCollection.Enumerator enumerator = GameManager.players.Values.GetEnumerator())
        {
            while (enumerator.MoveNext() && (bool)enumerator.Current)
            {
                ServerSend.MobMove(mob.GetId(), base.transform.position);
            }
        }
    }

    protected void SyncFindNextPosition()
    {
        if (GameManager.players != null)
        {
            Vector3 vector = FindNextPosition();
            if (mob.targetPlayerId != previousTargetId)
            {
                ServerSend.SendMobTarget(mob.id, mob.targetPlayerId);
            }
            previousTargetId = mob.targetPlayerId;
            if (!(vector == Vector3.zero))
            {
                mob.SetDestination(vector);
                ServerSend.MobSetDestination(mob.GetId(), vector);
            }
        }
    }

    protected abstract Vector3 FindNextPosition();
}
