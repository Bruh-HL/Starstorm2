﻿using Moonstorm;
using Moonstorm.Starstorm2.DamageTypes;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine.Networking;

namespace EntityStates.Nemmando
{
    //This might be stupid, and there might be a better way of using the token modifier here, but the damageCoefficient is a base field and i dont know if its possible to add attributes to inherited fields, so this exists now.
    class SwingSword2 : BasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        public static float swingTimeCoefficient = 1f;
        [TokenModifier("SS2_NEMMANDO_PRIMARY_BLADE_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float TokenModifier_dmgCoefficient => new SwingSword2().damageCoefficient;
        public int swingSide;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
        }

        public override void OnExit()
        {
            base.OnExit();
            string animationStateNameEnd = (swingSide == 0) ? "endPrimary1Tree" : "endPrimary2Tree";
            if (!animator.GetBool("isMoving") && animator.GetBool("isGrounded"))
            {
                PlayCrossfade("FullBody, Override", animationStateNameEnd, "Primary.playbackRate", duration * swingTimeCoefficient, 0.3f);
                PlayCrossfade("Gesture, Override", animationStateNameEnd, "Primary.playbackRate", duration * swingTimeCoefficient, 0.3f);
            }
            else PlayCrossfade("Gesture, Override", animationStateNameEnd, "Primary.playbackRate", duration * swingTimeCoefficient, 0.3f);
        }

        public override void PlayAnimation()
        {
            string animationStateName = (swingSide == 0) ? "Primary1Tree" : "Primary2Tree";
            
            if (!animator.GetBool("isMoving") && animator.GetBool("isGrounded"))
            {
                PlayCrossfade("FullBody, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.3f);
                PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.3f);
            }
            else PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.3f);

        }

        void SteppedSkillDef.IStepSetter.SetStep(int i)
        {
            swingSide = i;
            swingEffectMuzzleString = (swingSide == 0) ? "SwingLeft" : "SwingRight";
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)swingSide);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            swingSide = (int)reader.ReadByte();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            DamageAPI.AddModdedDamageType(overlapAttack, Gouge.gougeDamageType);
        }
    }
}