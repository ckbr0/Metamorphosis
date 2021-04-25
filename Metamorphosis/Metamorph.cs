using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using Hazel;
using UnhollowerBaseLib;

using PlayerInfo = GameData.LGBOMGHJELL;
using PhysicsHelpers = FJFJIDCFLDJ;
using Constants = LNCOKMACBKP;
using Object = UnityEngine.Object;

namespace Metamorphosis
{
    class Metamorph
    {
        public static Metamorph LocalMetamorph = null;

        private PlayerControl playerControl;
        private PlayerControl lastPlayerContact;


        public MorphInfo OrignalInfo;
        public byte PlayerId;

        public Metamorph(PlayerControl pc)
        {
            this.playerControl = pc;
            this.PlayerId = pc.PlayerId;

            this.OrignalInfo = new MorphInfo(pc);
        }

        public void FixedUpdate()
        {
            if (PlayerControl.AllPlayerControls.Count > 1)
            {
                if (isAlive())
                {
                    if (playerControl.POECPOEKKNO)
                    {
                        PlayerControl closestPlayer = FindClosestTarget(PlayerControl.LocalPlayer);

                        Metamorph outMetamorph = null;
                        if (closestPlayer != null && !PlayerControlPatch.IsMetamorph(closestPlayer.PlayerId, out outMetamorph))
                        {
                            lastPlayerContact = closestPlayer;
                        }
                    }
                }
            }
        }

        public void Update()
        {
            if (PlayerControl.AllPlayerControls.Count > 1)
            {
                HudManagerPatch.MorphButton.Visible = true;
                if (!isAlive())
                {
                    HudManagerPatch.MorphButton.Clickable = false;
                }
                else
                {
                    if (playerControl.POECPOEKKNO)
                    {
                        if (lastPlayerContact != null && !HudManagerPatch.MorphButton.IsEffectActive)
                        {
                            MorphInfo target = new MorphInfo(lastPlayerContact);
                            HudManagerPatch.MorphButton.SetTarget(target);
                            lastPlayerContact = null;
                        }
                        if (Input.GetKeyDown(KeyCode.F))
                        {
                            HudManagerPatch.MorphButton.PerformMorph();
                        }
                    }
                }
            }
        }

        public void MorphTo(MorphInfo target, bool updateName = true)
        {
            Metamorphosis.Logger.LogDebug(
                $@"Morph from {PlayerControl.LocalPlayer.PlayerId} 
                into {target.PlayerId}: 
                name: {target.Name}, 
                color: {target.ColorId}, 
                skin: {target.SkinId}, 
                hat: {target.HatId}, 
                pet: {target.PetId}");

            if (updateName)
                playerControl.nameText.text = target.Name;
            playerControl.KJAENOGGEOK.material.SetColor("_BackColor", Palette.ShadowColors[target.ColorId]);
            playerControl.KJAENOGGEOK.material.SetColor("_BodyColor", Palette.PlayerColors[target.ColorId]);
            playerControl.HatRenderer.SetHat(target.HatId, target.ColorId);
            playerControl.nameText.transform.localPosition = new Vector3(0f, (target.HatId == 0U) ? 0.7f : 1.05f, -0.5f);
            
            PlayerPhysics playerPhysics = playerControl.MyPhysics;
            if (playerControl.MyPhysics.Skin.skin.ProdId != DestroyableSingleton<HatManager>.MGCNALHEEBA.AllSkins[(int)target.SkinId].ProdId)
            {
                SkinData nextSkin = DestroyableSingleton<HatManager>.MGCNALHEEBA.AllSkins[(int)target.SkinId];
                AnimationClip clip = null;
                var spriteAnim = playerPhysics.Skin.animator;
                var anim = spriteAnim.m_animator;
                var skinLayer = playerPhysics.Skin;

                AnimationClip currentPhysicsAnim = playerPhysics.NIKGMJIKBMP.GetCurrentAnimation();
                if (currentPhysicsAnim == playerPhysics.RunAnim)
                    clip = nextSkin.RunAnim;
                else if (currentPhysicsAnim == playerPhysics.SpawnAnim)
                    clip = nextSkin.SpawnAnim;
                else if (currentPhysicsAnim == playerPhysics.EnterVentAnim)
                    clip = nextSkin.EnterVentAnim;
                else if (currentPhysicsAnim == playerPhysics.ExitVentAnim)
                    clip = nextSkin.ExitVentAnim;
                else if (currentPhysicsAnim == playerPhysics.IdleAnim)
                    clip = nextSkin.IdleAnim;
                else
                    clip = nextSkin.IdleAnim;

                float progress = playerPhysics.NIKGMJIKBMP.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                skinLayer.skin = nextSkin;

                spriteAnim.Play(clip, 1f);
                anim.Play("a", 0, progress % 1);
                anim.Update(0f);
            }

            if (playerControl.CurrentPet == null || playerControl.CurrentPet.ProductId != HatManager.MGCNALHEEBA.AllPets[(int)target.PetId].ProductId)
            {
                if (playerControl.CurrentPet)
                    Object.Destroy(playerControl.CurrentPet.gameObject);
                
                playerControl.CurrentPet = Object.Instantiate<PetBehaviour>(HatManager.MGCNALHEEBA.AllPets[(int)target.PetId]);
                playerControl.CurrentPet.transform.position = playerControl.transform.position;
                playerControl.CurrentPet.Source = playerControl;
                playerControl.CurrentPet.BDBDGFDELMB = playerControl.BDBDGFDELMB;
                PlayerControl.SetPlayerMaterialColors(target.ColorId, playerControl.CurrentPet.rend);
            }
            else if (playerControl.CurrentPet) 
            {
                PlayerControl.SetPlayerMaterialColors(target.ColorId, playerControl.CurrentPet.rend);
            }
        }

        public void RpcMorphTo(MorphInfo target)
        {
            Metamorphosis.Logger.LogDebug($"Rpc Morp to");
            MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetMorph, Hazel.SendOption.Reliable);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(target.Name);
            writer.Write(target.ColorId);
            writer.Write(target.SkinId);
            writer.Write(target.HatId);
            writer.Write(target.PetId);
            writer.EndMessage();

            MorphTo(target, false);
        }

        public void MorphBack()
        {
            Metamorphosis.Logger.LogMessage($"Morph back");

            /*if (!playerControl.moveable)
            {
                playerControl.SetName(OrignalInfo.Name);
                playerControl.SetColor(OrignalInfo.ColorId);
                playerControl.SetSkin(OrignalInfo.SkinId);
                playerControl.SetHat(OrignalInfo.HatId, OrignalInfo.ColorId);
                playerControl.SetPet(OrignalInfo.PetId);
                playerControl.CurrentPet.CAMJPMDIIMN = playerControl.CAMJPMDIIMN;
            }
            else
            {*/
                MorphTo(OrignalInfo);
            //}

            if (LocalMetamorph != null)
            {
                if (HudManagerPatch.MorphButton != null)
                {
                    HudManagerPatch.MorphButton.EndEffect(false);
                }
            }
        }

        public PlayerControl FindClosestTarget(PlayerControl player)
        {
            PlayerControl result = null;
            float num = 0.4f;
            if (!ShipStatus.Instance)
            {
                return null;
            }
            Vector2 truePosition = player.GetTruePosition();
            var allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.MFFAGDHDHLO && playerInfo.FNPNJHNKEBK != player.PlayerId && !playerInfo.IAGJEKLJCCI && !playerInfo.GJPBCGFPMOD.inVent)
                {
                    PlayerControl obj = playerInfo.GJPBCGFPMOD;
                    if (obj)
                    {
                        Vector2 vector = obj.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.HLIEDNLNBBH(truePosition, vector.normalized, magnitude, Constants.LEOCDMEJGPA))
                        {
                            result = obj;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }

        public bool isAlive()
        {
            if (playerControl != null)
            {
                return !playerControl.FIMGDJOCIGD.IAGJEKLJCCI;
            }
            return false;
        }
    }
}
