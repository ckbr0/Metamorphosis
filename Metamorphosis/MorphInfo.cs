using PlayerInfo = GameData.LGBOMGHJELL;

namespace Metamorphosis
{
    public readonly struct MorphInfo
    {
        public byte PlayerId { get; }
        public string Name { get; }
        public int ColorId { get; }
        public uint SkinId { get; }
        public uint HatId { get; }
        public uint PetId { get; }

        public MorphInfo(byte playerId,
                         string name,
                         int colorId,
                         uint skinId,
                         uint hatId,
                         uint petId)
        {
            this.PlayerId = playerId;
            this.Name = name;
            this.ColorId = colorId;
            this.SkinId = skinId;
            this.HatId = hatId;
            this.PetId = petId;
        }

        public MorphInfo(PlayerControl playerControl) : this(playerControl.PlayerId,
                                                             playerControl.name,
                                                             playerControl.FIMGDJOCIGD.IMMNCAGJJJC,
                                                             playerControl.FIMGDJOCIGD.CGNMKICGLOG,
                                                             playerControl.FIMGDJOCIGD.CPGFLBANALE,
                                                             playerControl.FIMGDJOCIGD.LBHODBKCBKA)
        {
        }
    }
}