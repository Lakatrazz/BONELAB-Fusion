using SLZ.Marrow.SceneStreaming;
using System.Collections.Generic;

namespace LabFusion.Extensions
{
    public static class ChunkExtensions
    {
        public static List<Chunk> GetChunks(this Chunk chunk)
        {
            var chunks = new List<Chunk>();
            chunks.Add(chunk);
            chunks.AddRange(chunk.linkedChunks);
            return chunks;
        }
    }
}
