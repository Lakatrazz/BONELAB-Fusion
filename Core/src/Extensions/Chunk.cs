using SLZ.Marrow.SceneStreaming;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Extensions {
    public static class ChunkExtensions {
        public static List<Chunk> GetChunks(this Chunk chunk) { 
            var chunks = new List<Chunk>();
            chunks.Add(chunk);
            chunks.AddRange(chunk.linkedChunks);
            return chunks;
        }
    }
}
