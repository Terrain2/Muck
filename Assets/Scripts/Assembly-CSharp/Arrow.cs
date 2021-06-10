﻿
using UnityEngine;

// Token: 0x02000003 RID: 3
public class Arrow : MonoBehaviour
{
	// Token: 0x17000001 RID: 1
	// (get) Token: 0x06000003 RID: 3 RVA: 0x000020BA File Offset: 0x000002BA
	// (set) Token: 0x06000004 RID: 4 RVA: 0x000020C2 File Offset: 0x000002C2
	public int damage { get; set; }

	// Token: 0x17000002 RID: 2
	// (get) Token: 0x06000005 RID: 5 RVA: 0x000020CB File Offset: 0x000002CB
	// (set) Token: 0x06000006 RID: 6 RVA: 0x000020D3 File Offset: 0x000002D3
	public bool otherPlayersArrow { get; set; }

	// Token: 0x06000007 RID: 7 RVA: 0x000020DC File Offset: 0x000002DC
	private void Awake()
	{
		this.rb = base.GetComponent<Rigidbody>();
	}

	// Token: 0x06000008 RID: 8 RVA: 0x000020EA File Offset: 0x000002EA
	private void Update()
	{
		base.transform.rotation = Quaternion.LookRotation(this.rb.velocity);
	}

	// Token: 0x06000009 RID: 9 RVA: 0x00002108 File Offset: 0x00000308
	private void OnCollisionEnter(Collision other)
	{
		if (this.done)
		{
			return;
		}
		this.done = true;
		int layer = other.gameObject.layer;
		if (!this.otherPlayersArrow && (layer == LayerMask.NameToLayer("Player") || layer == LayerMask.NameToLayer("Enemy")))
		{
			Hitable componentInChildren = other.transform.root.GetComponentInChildren<Hitable>();
			if (!componentInChildren)
			{
				return;
			}
			PowerupCalculations.DamageResult damageMultiplier = PowerupCalculations.Instance.GetDamageMultiplier(this.fallingWhileShooting, this.speedWhileShooting);
			float damageMultiplier2 = damageMultiplier.damageMultiplier;
			bool crit = damageMultiplier.crit;
			float lifesteal = damageMultiplier.lifesteal;
			int num = (int)((float)this.damage * damageMultiplier2);
			Vector3 pos = other.collider.ClosestPoint(base.transform.position);
			HitEffect hitEffect = HitEffect.Normal;
			if (damageMultiplier.sniped)
			{
				hitEffect = HitEffect.Big;
			}
			else if (crit)
			{
				hitEffect = HitEffect.Crit;
			}
			else if (damageMultiplier.falling)
			{
				hitEffect = HitEffect.Falling;
			}
			componentInChildren.Hit(num, 1f, (int)hitEffect, pos);
			PlayerStatus.Instance.Heal(Mathf.CeilToInt((float)num * lifesteal));
			if (damageMultiplier.sniped)
			{
				PowerupCalculations.Instance.HitEffect(PowerupCalculations.Instance.sniperSfx);
			}
			if (damageMultiplier2 > 0f && damageMultiplier.hammerMultiplier > 0f)
			{
				int num2 = 0;
				PowerupCalculations.Instance.SpawnOnHitEffect(num2, true, pos, (int)((float)num * damageMultiplier.hammerMultiplier));
				ClientSend.SpawnEffect(num2, pos);
			}
		}
		this.StopArrow(other);
	}

	// Token: 0x0600000A RID: 10 RVA: 0x00002270 File Offset: 0x00000470
	private void StopArrow(Collision other)
	{
		this.rb.isKinematic = true;
		base.transform.SetParent(other.transform);
		this.done = true;
		base.gameObject.AddComponent<DestroyObject>().time = 10f;
	Destroy(this);
	Destroy(this.audio);
		this.trail.emitting = false;
		Vector3 position = base.transform.position;
		Vector3 forward = -base.transform.forward;
		ParticleSystem component =Instantiate(this.hitFx, position, Quaternion.LookRotation(forward)).GetComponent<ParticleSystem>();
		Renderer component2 = other.gameObject.GetComponent<Renderer>();
		Material material = null;
		if (component2 != null)
		{
			material = component2.material;
		}
		else
		{
			SkinnedMeshRenderer componentInChildren = other.transform.root.GetComponentInChildren<SkinnedMeshRenderer>();
			if (componentInChildren)
			{
				material = componentInChildren.material;
			}
		}
		if (material)
		{
			component.GetComponent<Renderer>().material = material;
		}
	Destroy(base.gameObject);
	}

	// Token: 0x04000002 RID: 2
	private Rigidbody rb;

	// Token: 0x04000003 RID: 3
	public AudioSource audio;

	// Token: 0x04000004 RID: 4
	public TrailRenderer trail;

	// Token: 0x04000005 RID: 5
	public GameObject hitFx;

	// Token: 0x04000007 RID: 7
	public bool fallingWhileShooting;

	// Token: 0x04000008 RID: 8
	public float speedWhileShooting;

	// Token: 0x0400000A RID: 10
	private bool done;
}
