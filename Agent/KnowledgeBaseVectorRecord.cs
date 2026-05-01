using System;
using Microsoft.Extensions.VectorData;

namespace AgentApi.Agent
{
    // Top-level vector record so reflection-based mappers can discover the attributes correctly
    public class KnowledgeBaseVectorRecord
    {
        [VectorStoreKey]
        public Guid Id { get; set; }

        [VectorStoreData]
        public string Question { get; set; } = string.Empty;

        [VectorStoreData]
        public string Answer { get; set; } = string.Empty;

        // Ensure the vector dimension matches the embedding dimension used when storing
        [VectorStoreVector(768)]
        public float[]? Vector { get; set; }
    }
}
