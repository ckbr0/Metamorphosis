using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using Hazel;
using UnhollowerBaseLib;

using PlayerInfo = GameData.GOOIGLGKMCE;
using PhysicsHelpers = BFDDHAKIDJF;
using Constants = DPHADHMAPCB;

namespace Metamorphosis
{
    class Metamorph
    {
        private PlayerControl playerControl;
        private PlayerControl lastPlayerContact;

        public MorphInfo OrignalInfo;
        public byte PlayerId;

        public Metamorph(PlayerControl pc)
        {
            this.playerControl = pc;
            this.PlayerId = pc.PlayerId;

            // Save original apperance
            /*PlayerInfo playerInfo = playerControl.PKMHEDAKKHE;
            this.originalName = playerControl.name;
            this.originalColorId = playerInfo.ACBLKMFEPKC;
            this.originalSkinId = playerInfo.FHNDEEGICJP;
            this.originalHatId = playerInfo.KCILOGLJODF;
            this.originalPetId = playerInfo.HIJJGKGBKOJ; // ??*/
            this.OrignalInfo = new MorphInfo(pc);
        }

        public void FixedUpdate()
        {
            if (PlayerControl.AllPlayerControls.Count > 1)
            {
                if (PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    if (isAlive())
                    {
                        if (playerControl.MPEOHLJNPOB)
                        {
                            PlayerControl closestPlayer = FindClosestTarget(PlayerControl.LocalPlayer);

                            if (closestPlayer != null && !PlayerControlPatch.IsMetamorph(closestPlayer))
                            {
                                lastPlayerContact = closestPlayer;
                            }
                        }
                    }
                }
            }
        }

        public void Update()
        {
            if (PlayerControl.AllPlayerControls.Count > 1)
            {
                if (PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    HudManagerPatch.MorphButton.Visible = true;
                    if (!isAlive())
                    {
                        HudManagerPatch.MorphButton.Clickable = false;
                    }
                    else
                    {
                        if (playerControl.MPEOHLJNPOB)
                        {
                            if (lastPlayerContact != null && !HudManagerPatch.MorphButton.IsEffectActive)
                            {
                                MorphInfo target = new MorphInfo(lastPlayerContact);
                                HudManagerPatch.MorphButton.SetTarget(target, OrignalInfo);
                                lastPlayerContact = null;
                            }
                            if (Input.GetKeyDownInt(KeyCode.F))
                            {
                                HudManagerPatch.MorphButton.PerformMorph();
                            }
                        }
                    }
                }
            }
        }

        /*public void SendMorphMessage(string name, byte colorId, uint skinId, uint hatId, uint petId)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.Morph, SendOption.Reliable);
            messageWriter.Write(PlayerControl.LocalPlayer.PlayerId);
            messageWriter.Write(name);
            messageWriter.Write(colorId);
            messageWriter.Write(skinId);
            messageWriter.Write(hatId);
            messageWriter.Write(petId);
            messageWriter.EndMessage();
        }*/

        public void MorphBack()
        {
            playerControl.SetName(OrignalInfo.Name);
            playerControl.SetColor(OrignalInfo.ColorId);
            playerControl.SetSkin(OrignalInfo.SkinId);
            playerControl.SetHat(OrignalInfo.HatId, 0);
            playerControl.SetPet(OrignalInfo.PetId);

            // Send messages ???
            //SendMorphMessage(originalName, originalColorId, originalSkinId, originalHatId, originalPetId);

            Metamorphosis.Logger.LogDebug("Morph back");
        }

        public void SetOriginalName()
        {
            Metamorphosis.Logger.LogMessage($"SetOriginalName: {OrignalInfo.Name}");
            playerControl.nameText.Text = OrignalInfo.Name;
        }

        public PlayerControl FindClosestTarget(PlayerControl player)
        {
            PlayerControl result = null;
            float num = 0.4f;//GameOptionsData.EEPBOJKJCAJ[Mathf.Clamp(PlayerControl.GameOptions.GEMCDKBIFGG, 0, 2)];
            if (!ShipStatus.Instance)
            {
                return null;
            }
            Vector2 truePosition = player.GetTruePosition();
            var allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.HGCENMAGBJO && playerInfo.FMAAJCIEMEH != player.PlayerId && !playerInfo.AKOHOAJIHBE && !playerInfo.IBJBIALCEKB.inVent)
                {
                    PlayerControl obj = playerInfo.IBJBIALCEKB;
                    if (obj)
                    {
                        Vector2 vector = obj.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.OEEHJJNGMLJ(truePosition, vector.normalized, magnitude, Constants.EOJPPJKOKFH))
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
                return !playerControl.PKMHEDAKKHE.AKOHOAJIHBE;
            }
            return false;
        }
    }
}
