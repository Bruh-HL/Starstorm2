﻿using Moonstorm.Components;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class BuffTerminationVFX : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffTerminationVFX");

        public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matTerminationOverlay");
    }
}
