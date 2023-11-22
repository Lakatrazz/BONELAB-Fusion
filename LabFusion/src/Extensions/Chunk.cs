using System.Collections.Generic;
using SLZ.Marrow.SceneStreaming;

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
