using System;
using System.Collections.Generic;
using System.Text;

using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;

using AssemblyUnhollower;

using UnityEngine;
using Object = UnityEngine.Object;
using System.ComponentModel;

namespace Metamorphosis
{
    public class MorphButton : IDisposable
    {
        private KillButtonManager killButtonManager;
        private HudManager hudManager;

        private MorphInfo? morphTarget;
        private MorphInfo? morphOriginal;

        public Sprite ButtonSprite;

        public bool HudVisible { get; set; } = true;

        private float _cooldownDuration = 0F;
        public float CooldownDuration { get { return _cooldownDuration; } set { _cooldownDuration = Mathf.Max(0F, value); } }

        private float _cooldownTime = 0F;
        public float CooldownTime { get { return _cooldownTime; } private set { _cooldownTime = Mathf.Max(0F, value); } }
        public bool IsCoolingDown { get { return CooldownTime > 0F; } }

        public bool Visible { get; set; } = false;

        public bool Clickable { get; set; } = false;

        public bool IsUsable { get { return HudVisible && Visible && Clickable && ((!IsCoolingDown && morphTarget.HasValue) || IsEffectActive) && GameData.Instance; } }
        private float _effectDuration = 0F;
        public float EffectDuration { get { return _effectDuration; } set { _effectDuration = Mathf.Max(0F, value); } }
        public bool HasEffect { get { return EffectDuration > 0F; } }
        public bool IsEffectActive { get; private set; } = false;

        private Sprite[] sprites;
        private byte currentMorphColorId;

        private bool isDisposed;
        //private byte[] imageData;

        public MorphButton(HudManager hudManager)
        {
            this.hudManager = hudManager;
            CreateButton(hudManager);
        }

        private void CreateButton(HudManager hudManager)
        {
            if (killButtonManager != null || !hudManager?.KillButton /*|| imageData == null*/) return;

            killButtonManager = Object.Instantiate(hudManager.KillButton, hudManager.transform);

            // Load texture and create sprites
            Texture2D tex = new Texture2D(MorphButtonImage.Width, MorphButtonImage.Height, TextureFormat.RGBA32, false);//, TextureFormat.RGBA32, Texture.GenerateAllMips, false, IntPtr.Zero);
            tex.LoadRawTextureData(MorphButtonImage.Data);
            tex.Apply();

            sprites = new Sprite[12];
            for (int i = 0; i < 12; i++)
            {
                int k = i % 6;
                int j = 1;
                if (Math.Floor(i / 6.0f) > 0)
                {
                    j = 0;
                }
                sprites[i] = Sprite.Create(tex, new Rect(k*MorphButtonImage.Width/6, j*MorphButtonImage.Height/2, MorphButtonImage.Width/6, MorphButtonImage.Height/2), new Vector2(0.5f, 0.5f));
            }
            killButtonManager.renderer.sprite = ButtonSprite = sprites[0];

            PassiveButton button = killButtonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener(new Action(() =>
            {
                if (!IsUsable) return;

                PerformMorph();
            }));

            killButtonManager.gameObject.SetActive(HudVisible && Visible);
            killButtonManager.renderer.enabled = HudVisible && Visible;
        }

        public void Update()
        {
            if (!GameData.Instance || PlayerControl.LocalPlayer == null || killButtonManager == null)
            {
                Metamorphosis.Logger.LogDebug("Button update failed");

                IsEffectActive = false;
                CooldownTime = 0F;
                return;
            }

            Vector3 killPos = hudManager.KillButton.transform.localPosition;
            Vector3 repPos = hudManager.ReportButton.transform.localPosition;
            killButtonManager.transform.localPosition = new Vector3(killPos.x + 0.0f, repPos.y + 0.0f);

            if (morphTarget.HasValue)
            {
                killButtonManager.renderer.sprite = sprites[morphTarget.Value.ColorId];
            }
            else
            {
                killButtonManager.renderer.sprite = sprites[currentMorphColorId];
            }

            if (IsCoolingDown && Visible && (PlayerControl.LocalPlayer.moveable || IsEffectActive))
            {
                CooldownTime -= Time.deltaTime;

                if (!IsCoolingDown)
                {
                    if (IsEffectActive)
                    {
                        EndEffect();
                    }
                }
            }

            if (isDisposed) return;

            Clickable = morphTarget.HasValue || IsEffectActive;

            killButtonManager.gameObject.SetActive(HudVisible && Visible);
            killButtonManager.renderer.enabled = HudVisible && Visible;
            killButtonManager.TimerText.enabled = HudVisible && Visible && IsCoolingDown;

            killButtonManager.renderer.color = (IsCoolingDown && !IsEffectActive) || !Clickable ? Palette.DisabledColor : Palette.EnabledColor;

            killButtonManager.renderer.material.SetFloat("_Desat", Clickable ? 0F : 1F);

            UpdateCooldown();
        }

        private void UpdateCooldown()
        {
            float cooldownRate = 0;
            if (!IsEffectActive)
            {
                cooldownRate = CooldownDuration == 0F ? IsCoolingDown ? 1F : 0F : Mathf.Clamp(CooldownTime / (CooldownDuration), 0f, 1f);
            }
            else
            {
                cooldownRate = CooldownDuration == 0F ? IsCoolingDown ? 1F : 0F : Mathf.Clamp(CooldownTime / (EffectDuration), 0f, 1f);
            }
            killButtonManager.renderer?.material?.SetFloat("_Percent", cooldownRate);

            killButtonManager.isCoolingDown = IsCoolingDown;

            killButtonManager.TimerText.Text = Mathf.CeilToInt(CooldownTime).ToString();
            killButtonManager.TimerText.gameObject.SetActive(HudVisible && Visible && killButtonManager.isCoolingDown);
        }

        public void StartCooldown(float? customCooldown = null)
        {
            bool wasCoolingDown = IsCoolingDown;

            CooldownTime = customCooldown ?? CooldownDuration;
        }

        public void StartEffect()
        {
            IsEffectActive = true;

            CooldownTime = EffectDuration;

            killButtonManager.TimerText.Color = new Color(0F, 0.8F, 0F);
        }

        public void EndEffect(bool startCooldown = true)
        {
            if (IsEffectActive)
            {
                CancelMorph();

                IsEffectActive = false;

                killButtonManager.TimerText.Color = Palette.EnabledColor;
            }

            if (startCooldown) StartCooldown();
        }

        public void SetTarget(MorphInfo target, MorphInfo original)
        {
            if (!IsEffectActive && !IsCoolingDown)
            {
                this.morphTarget = target;
                this.morphOriginal = original;
            }
        }

        private void MorphTo(MorphInfo target)
        {
            PlayerControl.LocalPlayer.RpcSetName(target.Name);
            //SetOriginalName();
            PlayerControl.LocalPlayer.RpcSetColor(target.ColorId);
            PlayerControl.LocalPlayer.RpcSetSkin(target.SkinId);
            PlayerControl.LocalPlayer.RpcSetHat(target.HatId);
            PlayerControl.LocalPlayer.RpcSetPet(target.PetId);

            currentMorphColorId = target.ColorId;
            // Send messages ???
            //SendMorphMessage(name, colorId, skinId, hatId, petId);

            Metamorphosis.Logger.LogDebug(
                $@"Morph from {PlayerControl.LocalPlayer.PlayerId} 
                into {target.PlayerId}: 
                name: {target.Name}, 
                color: {target.ColorId}, 
                skin: {target.SkinId}, 
                hat: {target.HatId}, 
                pet: {target.PetId}");

        }

        private void CancelMorph()
        {
            if (morphOriginal.HasValue)
            {
                MorphTo(morphOriginal.Value);
            }
        }

        public void PerformMorph(bool startEffext = true)
        {
            if (IsUsable)
            {
                if (IsEffectActive)
                {
                    EndEffect();
                }
                else if (morphTarget.HasValue)
                {
                    MorphTo(morphTarget.Value);
                    morphTarget = null;

                    if (startEffext) StartEffect();
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        killButtonManager.renderer.enabled = false;
                        killButtonManager.TimerText.enabled = false;
                        foreach (Sprite sprite in sprites)
                        {
                            Object.Destroy(sprite);
                        }
                        sprites = null;

                        Object.Destroy(killButtonManager);
                    }
                    catch
                    {
                    }
                }
                isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
