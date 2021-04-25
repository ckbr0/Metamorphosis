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
using UnityEngine.Events;
using Object = UnityEngine.Object;
using System.ComponentModel;

namespace Metamorphosis
{
    public class MorphButton //: IDisposable
    {
        private KillButtonManager killButtonManager;
        private HudManager hudManager;

        private MorphInfo? morphTarget;

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

        private Texture2D texture;
        private Sprite[] sprites;

        //private bool isDisposed;

        public MorphButton(HudManager hudManager)
        {
            this.hudManager = hudManager;
            CreateButton(hudManager);
        }

        private void CreateButton(HudManager hudManager)
        {
            if (killButtonManager != null || !hudManager?.KillButton) return;

            killButtonManager = Object.Instantiate(hudManager.KillButton, hudManager.transform);

            // Load texture and create sprites
            texture = new Texture2D(MorphButtonImage.Width, MorphButtonImage.Height, TextureFormat.RGBA32, false);//, TextureFormat.RGBA32, Texture.GenerateAllMips, false, IntPtr.Zero);
            texture.LoadRawTextureData(MorphButtonImage.Data);
            texture.Apply(true, true);

            int numSprites = MorphButtonImage.NumberOfSprites;
            int cols = MorphButtonImage.Cols;
            int rows = MorphButtonImage.Rows;
            sprites = new Sprite[numSprites];
            for (int i = 0; i < numSprites; i++)
            {
                int j = i % (numSprites/rows);
                //int j = 1;
                int k = i / (numSprites/rows);

                //Metamorphosis.Logger.LogMessage($"Create button: color id: {i}, ({j}, {k})");

                sprites[i] = Sprite.Create(texture, new Rect(j*MorphButtonImage.Width/cols, k*MorphButtonImage.Height/rows, MorphButtonImage.Width/cols, MorphButtonImage.Height/rows), new Vector2(0.5f, 0.5f));
            }
            //killButtonManager.renderer.sprite = sprites[0];

            PassiveButton button = killButtonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((UnityAction)(() =>
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

            if (Metamorph.LocalMetamorph == null)
            {
                Visible = false;
            }

            Vector3 killPos = hudManager.KillButton.transform.localPosition;
            Vector3 repPos = hudManager.ReportButton.transform.localPosition;
            killButtonManager.transform.localPosition = new Vector3(killPos.x + 0.0f, repPos.y + 0.0f);

            if (!morphTarget.HasValue)
            {
                killButtonManager.renderer.sprite = sprites[0];
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

            //if (isDisposed) return;

            Clickable = (morphTarget.HasValue || IsEffectActive) && PlayerControl.LocalPlayer.moveable;

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

            killButtonManager.TimerText.text = Mathf.CeilToInt(CooldownTime).ToString();
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

            killButtonManager.TimerText.color = new Color(0F, 0.8F, 0F);
        }

        public void EndEffect(bool callMorphBack = true)
        {
            if (IsEffectActive)
            {
                IsEffectActive = false;

                killButtonManager.TimerText.color = Palette.EnabledColor;

                ClearTarget();

                if (callMorphBack)
                    Metamorph.LocalMetamorph.RpcMorphTo(Metamorph.LocalMetamorph.OrignalInfo);
            }

            StartCooldown();
        }

        public void SetTarget(MorphInfo target)
        {
            if (!IsEffectActive && !IsCoolingDown)
            {
                this.morphTarget = target;
                killButtonManager.renderer.sprite = sprites[morphTarget.Value.ColorId];
            }
        }

        public void ClearTarget()
        {
            this.morphTarget = null;
            killButtonManager.renderer.sprite = sprites[0];
        }

        public void PerformMorph(bool startEffext = true)
        {
            if (IsUsable)
            {
                if (IsEffectActive && PlayerControl.LocalPlayer.moveable)
                {
                    EndEffect();
                }
                else if (morphTarget.HasValue)
                {
                    Metamorph.LocalMetamorph.RpcMorphTo(morphTarget.Value);

                    if (startEffext) StartEffect();
                }
            }
        }

        public void HideButton()
        {
            killButtonManager.gameObject.SetActive(false);
            killButtonManager.renderer.enabled = false;
            killButtonManager.TimerText.enabled = false;
        }

        /*protected virtual void Dispose(bool disposing)
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
                        Object.Destroy(texture);
                        texture = null;

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
        }*/
    }
}
