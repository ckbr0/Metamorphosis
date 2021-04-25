using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;
using Hazel;
using UnhollowerBaseLib;
using BepInEx;
using BepInEx.IL2CPP;

using PlayerInfo = GameData.LGBOMGHJELL;

namespace Metamorphosis
{
	[HarmonyPatch(typeof(PlayerControl))]
	class PlayerControlPatch
	{
		public static List<Metamorph> Metamorphs = null;

		public static List<PlayerControl> GetInfected(Il2CppReferenceArray<PlayerInfo> infections, ref List<byte> infectedIds)
		{
			List<PlayerControl> infected = new List<PlayerControl>();

			foreach (PlayerInfo infection in infections)
			{
				byte playerId = infection.GJPBCGFPMOD.PlayerId;
				PlayerControl infectedControl = GetPlayerControlById(playerId);
				if (infectedControl != null)
                {
					infected.Add(infectedControl);
					infectedIds.Add(playerId);
				}
			}

			return infected;
		}

		public static bool IsMetamorph(byte playerId, out Metamorph outMetamorph)
        {
			outMetamorph = null;
			if (Metamorphs != null)
			{
				foreach (Metamorph metamorph in Metamorphs)
				{
					if (metamorph.PlayerId == playerId)
					{
						outMetamorph = metamorph;
						return true;
					}
				}
			}
			return false;
        }

		public static bool IsMetamorph(byte playerId)
        {
			if (Metamorphs != null)
			{
				foreach (Metamorph metamorph in Metamorphs)
				{
					if (metamorph.PlayerId == playerId)
					{
						return true;
					}
				}
			}
			return false;
        }

		public static void InitMetamorphs()
        {
			if (Metamorphs != null)
				Metamorphs.Clear();
			
			Metamorphs = new List<Metamorph>();
			Metamorph.LocalMetamorph = null;
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
            if (Metamorph.LocalMetamorph != null)
            {
				Metamorph.LocalMetamorph.FixedUpdate();
            }
        }

		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerControl.HandleRpc))]
		public static void Postfix(byte ACCJCEHMKLN, MessageReader HFPCBBHJIPJ)
		{
			switch (ACCJCEHMKLN)
			{
				case (byte)CustomRPC.SetMetamorphs:
					{
						InitMetamorphs();

						byte[] infections = HFPCBBHJIPJ.ReadBytesAndSize();
						Metamorphosis.Logger.LogMessage(String.Format($"HandleRpc SetMetamorphs Length: {infections.Length}"));
						foreach (byte infectedId in infections)
						{
							PlayerControl infectedControl = GetPlayerControlById(infectedId);
							if (infectedControl != null)
                            {
								Metamorph metamorph = new Metamorph(infectedControl);
								Metamorphs.Add(metamorph);
								Metamorphosis.Logger.LogDebug(String.Format($"HandleRpc SetMetamorphs Add Metamorph: {metamorph.PlayerId}"));
								if (metamorph.PlayerId == PlayerControl.LocalPlayer.PlayerId)
								{
									Metamorphosis.Logger.LogDebug(String.Format($"HandleRpc SetMetamorphs is local player {metamorph.PlayerId}"));
									Metamorph.LocalMetamorph = metamorph;
									if (HudManagerPatch.MorphButton != null)
									{
										HudManagerPatch.MorphButton.StartCooldown(CustomGameOptions.MorphCooldown+9.0f);
										HudManagerPatch.MorphButton.EffectDuration = CustomGameOptions.MorphDuration;
										HudManagerPatch.MorphButton.CooldownDuration = CustomGameOptions.MorphCooldown;
									}
								}
							}
						}

						break;
					}
				case (byte)CustomRPC.SetMorph:
					{
						Metamorphosis.Logger.LogDebug("HandleRpc SetMorph");

						byte playerId = HFPCBBHJIPJ.ReadByte();
            			string name = HFPCBBHJIPJ.ReadString();
            			byte colorId = HFPCBBHJIPJ.ReadByte();
						uint skinId = HFPCBBHJIPJ.ReadUInt32();
						uint hatId = HFPCBBHJIPJ.ReadUInt32();
						uint petId = HFPCBBHJIPJ.ReadUInt32();

						MorphInfo target = new MorphInfo(playerId, name, colorId, skinId, hatId, petId);

						Metamorph metamorph;				
						if (IsMetamorph(playerId, out metamorph))
						{
							
							bool updateName = !IsMetamorph(PlayerControl.LocalPlayer.PlayerId);
							metamorph.MorphTo(target, updateName);
						}
						else
						{
							Metamorphosis.Logger.LogError("HandleRpc SetMorph: Player is not a Metamorph");
						}
						break;
					}
				case (byte)RPC.StartMeeting:
					{
						Metamorphosis.Logger.LogDebug("HandleRpc StartMeeting");
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
		public static void Postfix(Il2CppReferenceArray<PlayerInfo> BHNEINNHPIJ)
		{
			Metamorphosis.Logger.LogDebug($"RcpSetInfected postfix start");
			InitMetamorphs();

			List<byte> infectedIds = new List<byte>();
			List<PlayerControl> infected = GetInfected(BHNEINNHPIJ, ref infectedIds);
			foreach (PlayerControl playerControl in infected)
            {
				Metamorph metamorph = new Metamorph(playerControl);
				Metamorphs.Add(metamorph);
				Metamorphosis.Logger.LogDebug($"RcpSetInfected postfix: {metamorph.PlayerId} added");
				if (playerControl.PlayerId == PlayerControl.LocalPlayer.PlayerId)
				{	
					Metamorphosis.Logger.LogDebug($"RcpSetInfected postfix: Metamorph is a local player {metamorph.PlayerId}");
					Metamorph.LocalMetamorph = metamorph;
					if (HudManagerPatch.MorphButton != null)
					{
						HudManagerPatch.MorphButton.StartCooldown(CustomGameOptions.MorphCooldown+9.0f);
						HudManagerPatch.MorphButton.EffectDuration = CustomGameOptions.MorphDuration;
						HudManagerPatch.MorphButton.CooldownDuration = CustomGameOptions.MorphCooldown;
					}
				}
            }

			MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetMetamorphs, Hazel.SendOption.Reliable);

			writer.WriteBytesAndSize(infectedIds.ToArray());

			writer.EndMessage();

			Metamorphosis.Logger.LogDebug("RcpSetInfected postfix");
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerControl.RpcStartMeeting))]
		public static void Prefix(PlayerInfo MPNMHCKHHOD)
		{
			Metamorphosis.Logger.LogDebug("RpcStartMeeting Prefix");
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
		public static void Postfix(CEIOGGEDKAN DJGAEEMDIDF)
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
