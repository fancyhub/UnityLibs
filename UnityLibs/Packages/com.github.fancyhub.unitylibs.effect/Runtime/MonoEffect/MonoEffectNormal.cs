/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public class MonoEffectNormal : MonoEffect
    {
        public MonoEffectAnimator _Animator;
        public MonoEffectParticle _Particle;
        public MonoEffectAnimation _Animation;

        public override void Play(MonoEffectInitParam init_param)
        {
            base.Play(init_param);

            _Animator.Start(init_param.TimeElapsed);
            _Particle.Start(init_param.TimeElapsed);
            _Animation.Start(init_param.TimeElapsed);
        }
       

#if UNITY_EDITOR
        public override void EdCollect()
        {
            _Animator.EdCollect(transform);
            _Particle.EdCollect(transform);
            _Animation.EdCollect(transform);
        }

        public override void LinkTo(Transform tar)
        {
            throw new NotImplementedException();
        }

        protected override void OnPrepare()
        {
            throw new NotImplementedException();
        }

        protected override void OnPlay(float time_elapsed)
        {
            throw new NotImplementedException();
        }

        protected override void OnScale(float scale)
        {
            throw new NotImplementedException();
        }
#endif
    }


    [Serializable]
    public struct MonoEffectAnimator
    {
        public Animator[] Animators;
        public MonoEffectAnimator(Animator[] animators)
        {
            this.Animators = animators;
        }

        public void Start(float timeElapsed)
        {
            foreach (var anim in this.Animators)
            {
                if (anim.runtimeAnimatorController == null)
                    continue;
                if (!anim.gameObject.activeInHierarchy)
                    continue;
                anim.speed = 1.0f;

                AnimatorStateInfo currentStateInfo = anim.GetCurrentAnimatorStateInfo(0);
                if (currentStateInfo.fullPathHash != 0)
                {
                    anim.Play(currentStateInfo.fullPathHash, 0, 0);
                }
            }
        }

        public void SetScale(float scale)
        {
            foreach (var anim in this.Animators)
                anim.speed = scale;
        }

        public void EdCollect(Transform tran)
        {
            Animators = tran.GetComponentsInChildren<Animator>(true);
        }
    }

    [Serializable]
    public struct MonoEffectParticle
    {
        public ParticleSystem[] ParticleSystems;

        public MonoEffectParticle(ParticleSystem[] particleSystems)
        {
            this.ParticleSystems = particleSystems;
        }

        public void Start(float timeElapsed)
        {
            foreach (var p in this.ParticleSystems)
            {
                var main = p.main;
                main.simulationSpeed = 1.0f;

                p.Simulate(timeElapsed);
                p.Play();
            }
        }

        public void EdCollect(Transform tran)
        {
            ParticleSystems = tran.GetComponentsInChildren<ParticleSystem>(true);
        }
    }

    [Serializable]
    public struct MonoEffectAnimation
    {
        public Animation[] Animations;
        public MonoEffectAnimation(Animation[] animations)
        {
            this.Animations = animations;
        }

        public void Start(float timeElapsed)
        {
            foreach (Animation anim in this.Animations)
            {
                foreach (AnimationState anim_state in anim)
                {
                    anim_state.speed = 1.0f;
                }
            }
        }

        public void EdCollect(Transform tran)
        {
            Animations = tran.GetComponentsInChildren<Animation>(true);
        }
    }
}
