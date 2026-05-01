using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using OllamaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static AgentApi.Agent.IngestDataIntoVectorStoreService;

namespace AgentApi.Agent
{
    public class IngestDataIntoVectorStoreService
    {
        public async Task RunSample()
        {
            var ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"), "nomic-embed-text:latest");
            // Sample Data simulating an Internal Employee Handbook / Knowledge base
            List<KnowledgeBaseEntry> knowledgeBase = new List<KnowledgeBaseEntry>
            {
                new("What is the WI-FI Password at the Office?", "The Password is 'Guest42'"),
                new("Is Christmas Eve a full or half day off", "It is a full day off"),
                new("How do I register vacation?", "Go to the internal portal and under Vacation Registration (top right), enter your request. Your manager will be notified and will approve/reject the request"),
                new("What do I need to do if I'm sick?", "Inform you manager, and if you have any meetings remember to tell the affected colleagues/customers"),
                new("Where is the employee handbook?", "It is located [here](https://www.yourcompany.com/hr/handbook.pdf)"),
                new("Who is in charge of support?", "John Doe is in charge of support. His email is john@yourcompany.com"),
                new("I can't log in to my office account", "Take hold of Susan. She can reset your password"),
                new("When using the CRM System if get error 'index out of bounds'", "That is a known issue. Log out and back in to get it working again. The CRM team have been informed and status of ticket can be seen here: https://www.crm.com/tickets/12354"),
                new("What is the policy on buying books and online courses?", "Any training material under 20$ you can just buy.. anything higher need an approval from Richard"),
                new("Is there a bounty for find candidates for an open job position?", "Yes. 1000$ if we hire them... Have them send the application to jobs@yourcompany.com")
            };

            // Decide embedding dimension expected by your embedding model. Keep 768 here.
            const int embeddingDimension = 768;


            // Do not pass a possibly-null embedding generator to the vector store; we'll populate vectors explicitly below.
            string connectionString = $"Data Source={Path.GetTempPath()}\\af-course-vector-store.db";
            VectorStore vectorStore = new Microsoft.SemanticKernel.Connectors.SqliteVec.SqliteVectorStore(connectionString, new SqliteVectorStoreOptions
            {
                EmbeddingGenerator = ollamaClient
            });

            VectorStoreCollection<Guid, KnowledgeBaseVectorRecord> vectorStoreCollection = vectorStore.GetCollection<Guid, KnowledgeBaseVectorRecord>("knowledge_base");

            // Ensure collection exists, delete and recreate so schema matches expected vector dimension
            await vectorStoreCollection.EnsureCollectionExistsAsync();
            await vectorStoreCollection.EnsureCollectionDeletedAsync();
            await vectorStoreCollection.EnsureCollectionExistsAsync();

            int counter = 0;
            foreach (KnowledgeBaseEntry entry in knowledgeBase)
            {
                counter++;
                Console.Write($"\rEmbedding Data: {counter}/{knowledgeBase.Count}");


                // Generate a deterministic placeholder embedding. Replace with real embedding generation in production.
               // float[] vector = GenerateDeterministicEmbedding(entry.Question + "\n" + entry.Answer, embeddingDimension);
                // Replace this line:
                // var vector = await ollamaClient.GenerateVectorAsync(entry.Question + "\n" + entry.Answer, new EmbeddingGenerationOptions
                // {
                //     Dimensions = embeddingDimension,
                // });

                // With this:
                //var vectorMemory = await ollamaClient.GenerateVectorAsync(entry.Question + "\n" + entry.Answer, new EmbeddingGenerationOptions
                //{
                //    //Dimensions = embeddingDimension,
                //});
                //float[] vector = vectorMemory.ToArray();
                await vectorStoreCollection.UpsertAsync(new KnowledgeBaseVectorRecord
                {
                    Id = Guid.NewGuid(),
                    Question = entry.Question,
                    Answer = entry.Answer
                    // store as JSON text so SQLite column is TEXT
                  // Vector = vector
                });
            }

            Console.WriteLine();
            Console.WriteLine("\rEmbedding complete...");

            Console.WriteLine();

            Console.WriteLine("Listing all data in the vector-store");
            await foreach (KnowledgeBaseVectorRecord existingRecord in vectorStoreCollection.GetAsync(record => record.Id != Guid.Empty, int.MaxValue))
            {
         
                Console.WriteLine($"Q: {existingRecord.Question} - A: {existingRecord.Answer} - Vector length: {existingRecord.Vector.Length}");
            }
        }

        // Public SearchResult DTO
        public record SearchResult(string Question, string Answer, float Score);

        // Search the vector store by generating an embedding for the query and returning top-k results using cosine similarity
        public async Task<List<SearchResult>> SearchAsync(string query, int top = 3)
        {
            var searchResults = new List<SearchResult>();
            var ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"), "nomic-embed-text:latest");

            const int embeddingDimension = 768;
            string connectionString = $"Data Source={Path.GetTempPath()}\\af-course-vector-store.db";
            VectorStore vectorStore = new Microsoft.SemanticKernel.Connectors.SqliteVec.SqliteVectorStore(connectionString, new SqliteVectorStoreOptions
            {
                EmbeddingGenerator = ollamaClient
            });

            var embedding = await ollamaClient.EmbedAsync(new OllamaSharp.Models.EmbedRequest { Input = [query] });

            VectorStoreCollection<Guid, KnowledgeBaseVectorRecord> vectorStoreCollection = vectorStore.GetCollection<Guid, KnowledgeBaseVectorRecord>("knowledge_base");

            // Ensure collection exists
            await foreach (VectorSearchResult<KnowledgeBaseVectorRecord> searchResult in vectorStoreCollection.SearchAsync(query, 3))
            {
                string searchResultAsQAndA = $"Q: {searchResult.Record.Question} - A: {searchResult.Record.Answer}";
                searchResults.Add(new SearchResult(searchResult.Record.Question, searchResult.Record.Answer, (float)searchResult.Score));
              //  Output.Gray($"Search result [Score: {searchResult.Score}] {searchResultAsQAndA}");
                //mostSimilarKnowledge.AppendLine(searchResultAsQAndA);
            }
          return searchResults;
        }

        public record KnowledgeBaseEntry(string Question, string Answer);

        public class KnowledgeBaseVectorRecord
        {
            [VectorStoreKey]
            public required Guid Id { get; set; }

            [VectorStoreData]
            public required string Question { get; set; }

            [VectorStoreData]
            public required string Answer { get; set; }

            // Store JSON text representing vector to satisfy SQLite TEXT/BLOB requirement
            [VectorStoreVector(768)]
            public string Vector => $"Q: {Question} - A: {Answer}";
        }
    }
}
