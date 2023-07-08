﻿using Manifold.IO;

namespace GameCube.GCI
{
    public abstract class GciWithUniqueID<TBinarySerializable> : Gci<TBinarySerializable>
        where TBinarySerializable : IBinarySerializable, IBinaryFileType, new()
    {
        public abstract ushort Unknown { get; }
        public abstract ushort UniqueID { get; }
        public abstract ushort[] UniqueIDs { get; }

        public void ValidateUniqueID()
        {
            // Validate header with regards to expected data
            bool isValidUniqueID = false;
            foreach (var uniqueID in UniqueIDs)
            {
                if (uniqueID == UniqueID)
                {
                    isValidUniqueID = true;
                    break;
                }
            }
            if (!isValidUniqueID)
            {
                string msg = $"{nameof(UniqueIDs)} did not match header value {UniqueID:x4}.";
                throw new InvalidGciException(msg);
            }
        }

        public override void Deserialize(EndianBinaryReader reader)
        {
            base.Deserialize(reader);
            ValidateUniqueID();
        }

        public override void Serialize(EndianBinaryWriter writer)
        {
            base.Serialize(writer);
            ValidateUniqueID();
        }
    }
}
