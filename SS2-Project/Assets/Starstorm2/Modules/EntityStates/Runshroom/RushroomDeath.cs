﻿namespace EntityStates.Runshroom
{
    public class RushroomDeath : GenericCharacterDeath
    {
        public override void OnEnter()
        {
            base.OnEnter();
            VFXListComponent component = characterBody.GetComponent<VFXListComponent>();
            if (component)
            {
                component.TurnOffVFX();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            
            base.OnExit();
        }
    }
}
