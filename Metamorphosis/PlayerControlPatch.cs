using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;
using Hazel;
using UnhollowerBaseLib;
using BepInEx;
using BepInEx.IL2CPP;

using PlayerInfo = GameData.GOOIGLGKMCE;

namespace Metamorphosis
{
	[HarmonyPatch(typeof(PlayerControl))]
	class PlayerControlPatch
	{
		public static List<Metamorph> Metamorphs;

		public static List<PlayerControl> GetInfected(Il2CppReferenceArray<PlayerInfo> infections)
		{
			List<PlayerControl> infected = new List<PlayerControl>();

			foreach (PlayerInfo infection in infections)
			{
				PlayerControl infectedControl = GetPlayerControlById(infection.IBJBIALCEKB.PlayerId);
				if (infectedControl != null)
                {
					infected.Add(infectedControl);
				}
			}

			return infected;
		}

		public static bool IsMetamorph(PlayerControl playerControl)
        {
			foreach (Metamorph metamorph in Metamorphs)
            {
				if (metamorph.PlayerId == playerControl.PlayerId)
                {
					return true;
                }
            }
			return false;
        }

		public static void InitMetamorphs()
        {
			Metamorphs = new List<Metamorph>();

			/*if (HudManagerPatch.morphButton != null)
			{
				//HudManagerPatch.morphButton.visible = false;
				//HudManagerPatch.morphButton.clickable = false;
				//HudManagerPatch.morphButton.Update();
			}*/
		}

		public static PlayerControl GetPlayerControlById(byte id)
        {
			foreach(PlayerControl playerControl in PlayerControl.AllPlayerControls)
            {
				if (playerControl.PlayerId == id)
                {
					return playerControl;
                }
            }
			return null;
        }

		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerControl.FixedUpdate))]
		public static void Postfix(PlayerControl __instance)
		{
            if (Metamorphs != null)
            {
                foreach (Metamorph metamorph in Metamorphs)
                {
                    metamorph.FixedUpdate();
                }
            }
        }

		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerControl.HandleRpc))]
		public static void Postfix(byte ACCJCEHMKLN, MessageReader HFPCBBHJIPJ)
		{
			switch (ACCJCEHMKLN)
			{
				case (byte)RPC.SetInfected:
					{
						HFPCBBHJIPJ.Position = 2;
						Metamorphosis.Logger.LogMessage(String.Format("HandleRpc SetInfected"));
						Metamorphosis.Logger.LogMessage(String.Format($"HandleRpc SetInfected MessageReader Length: {HFPCBBHJIPJ.Length}"));
						Metamorphosis.Logger.LogMessage(String.Format($"HandleRpc SetInfected MessageReader BytesRemaining: {HFPCBBHJIPJ.BytesRemaining}"));

						byte[] infections = HFPCBBHJIPJ.ReadBytesAndSize();
						Metamorphosis.Logger.LogMessage(String.Format($"HandleRpc SetInfected Length: {infections.Length}"));
						InitMetamorphs();
						foreach (byte infectedId in infections)
						{
							PlayerControl infectedControl = GetPlayerControlById(infectedId);
							if (infectedControl != null)
                            {
								Metamorphosis.Logger.LogMessage(String.Format($"HandleRpc SetInfected Add Metamorph: {infectedId}"));
								Metamorphs.Add(new Metamorph(infectedControl));
							}
							if (infectedControl.PlayerId == PlayerControl.LocalPlayer.PlayerId)
							{
								Metamorphosis.Logger.LogMessage(String.Format($"HandleRpc SetInfected MorphButton StartCooldown: {infectedControl.PlayerId}"));
								if (HudManagerPatch.MorphButton != null)
									HudManagerPatch.MorphButton.StartCooldown(HudManagerPatch.MorphButton.CooldownDuration+9.0f);
							}
						}
						Metamorphosis.Logger.LogMessage(String.Format("HandleRpc SetInfected metamorphs created"));
						//HFPCBBHJIPJ.Position = 0;

						break;
					}
				case (byte)RPC.SetName:
					{
						Metamorphosis.Logger.LogMessage(String.Format("HandleRpc SetName"));
						if (Metamorphs != null)
						{
							if (IsMetamorph(PlayerControl.LocalPlayer))
							{
								foreach (Metamorph metamorph in Metamorphs)
								{
									metamorph.SetOriginalName();
								}
							}
						}
						break;
					}
				case (byte)RPC.SetColor:
					{
						Metamorphosis.Logger.LogMessage("HandleRpc SetColor");
						break;
					}
				case (byte)RPC.SetSkin:
					{
						Metamorphosis.Logger.LogMessage("HandleRpc SetSkin");
						break;
					}
				case (byte)RPC.SetHat:
					{
						Metamorphosis.Logger.LogMessage("HandleRpc SetHat");
						break;
					}
				case (byte)RPC.SetPet:
					{
						Metamorphosis.Logger.LogMessage("HandleRpc SetPet");
						break;
					}
				case (byte)RPC.StartMeeting:
					{
						Metamorphosis.Logger.LogMessage("HandleRpc StartMeeting");
						if (PlayerControlPatch.Metamorphs != null)
						{
							foreach (Metamorph metamorph in PlayerControlPatch.Metamorphs)
							{
								metamorph.MorphBack();
							}
						}
						break;
					}
				case (byte)CustomRPC.SyncCustomSettings:
					{
						CustomGameOptions.MorphDuration = System.BitConverter.ToSingle(HFPCBBHJIPJ.ReadBytes(4), 0);
						CustomGameOptions.MorphCooldown = System.BitConverter.ToSingle(HFPCBBHJIPJ.ReadBytes(4), 0);
						break;
					}
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerControl.RpcSetInfected))]
		public static void Postfix(Il2CppReferenceArray<PlayerInfo> FMAOEJEHPAO)
		{
			InitMetamorphs();

			List<PlayerControl> infected = GetInfected(FMAOEJEHPAO);
			foreach (PlayerControl playerControl in infected)
            {
				Metamorphs.Add(new Metamorph(playerControl));
				if (playerControl.PlayerId == PlayerControl.LocalPlayer.PlayerId)
				{
					if (HudManagerPatch.MorphButton != null)
						HudManagerPatch.MorphButton.StartCooldown(HudManagerPatch.MorphButton.CooldownDuration+9.0f);
				}
            }
			Metamorphosis.Logger.LogMessage("RcpSetInfected postfix");
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerControl.RpcSetName))]
		public static void Postfix(string ILCMIGKHPJE)
		{
			Metamorphosis.Logger.LogMessage("RpcSetName postfix: " + ILCMIGKHPJE);
			if (Metamorphs != null)
			{
				if (IsMetamorph(PlayerControl.LocalPlayer))
				{
					//Metamorphosis.Logger.LogMessage($"RpcSetName postfix: local player is metamorph {PlayerControl.LocalPlayer.PlayerId}");
					foreach (Metamorph metamorph in Metamorphs)
					{
						//Metamorphosis.Logger.LogMessage($"RpcSetName postfix: playerId: {metamorph.PlayerId}");
						metamorph.SetOriginalName();
					}
				}
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerControl.RpcSetColor))]
		public static void Postfix(byte MCBMEOBLGBM)
		{
			Metamorphosis.Logger.LogMessage("RpcSetColor postfix: " + MCBMEOBLGBM);
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerControl.RpcSetPet))]
		public static void Postfix(uint OKCKCDKKMPL)
		{
			Metamorphosis.Logger.LogMessage("RpcSetPet postfix: pet id: " + OKCKCDKKMPL);
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerControl.RpcSetHat))]
		public static void Postfix2(uint GAHEEOBFPPM)
		{
			Metamorphosis.Logger.LogMessage("RpcSetHat postfix: hat id: " + GAHEEOBFPPM);
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerControl.RpcSetSkin))]
		public static void Postfix3(uint JJJEILFGKOE)
		{
			Metamorphosis.Logger.LogMessage("RpcSetSkin postfix: hat id: " + JJJEILFGKOE);
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerControl.RpcStartMeeting))]
		public static void Prefix(PlayerInfo PLABNNNBHAC)
		{
			Metamorphosis.Logger.LogMessage("RpcStartMeeting Prefix");
			if (PlayerControlPatch.Metamorphs != null)
            {
                foreach (Metamorph metamorph in PlayerControlPatch.Metamorphs)
                {
					metamorph.MorphBack();
                }
            }
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerControl.RpcSyncSettings))]
		public static void Postfix(PAMOPBEDCNI OMFKMPLOPPM)
		{
			if (PlayerControl.AllPlayerControls.Count > 1)
			{
				MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SyncCustomSettings, Hazel.SendOption.Reliable);

				writer.Write(CustomGameOptions.MorphDuration);
				writer.Write(CustomGameOptions.MorphCooldown);

				writer.EndMessage();
			}
		}
	}
}
