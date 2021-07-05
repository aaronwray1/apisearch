using System;
using System.Threading;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Spatial;

namespace searchapi
{
    class Program
    {
        // This sample shows how to delete, create, upload documents and query an index
        static void Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            SearchIndexClient indexClient = CreateSearchIndexClient(configuration);

            string indexName = configuration["SearchIndexName"];

            Console.WriteLine("{0}", "Deleting index...\n");
            DeleteIndexIfExists(indexName, indexClient);

            Console.WriteLine("{0}", "Creating index...\n");
            CreateIndex(indexName, indexClient);

            SearchClient searchClient = indexClient.GetSearchClient(indexName);

            Console.WriteLine("{0}", "Uploading documents...\n");
            UploadDocuments(searchClient);

            SearchClient indexClientForQueries = CreateSearchClientForQueries(indexName, configuration);

            RunQueries(indexClientForQueries);

        }

        private static SearchIndexClient CreateSearchIndexClient(IConfigurationRoot configuration)
        {
            string searchServiceEndPoint = configuration["SearchServiceEndPoint"];
            string adminApiKey = configuration["SearchServiceAdminApiKey"];

            SearchIndexClient indexClient = new SearchIndexClient(new Uri(searchServiceEndPoint), new AzureKeyCredential(adminApiKey));
            return indexClient;
        }

        private static SearchClient CreateSearchClientForQueries(string indexName, IConfigurationRoot configuration)
        {
            string searchServiceEndPoint = configuration["SearchServiceEndPoint"];
            string queryApiKey = configuration["SearchServiceQueryApiKey"];

            SearchClient searchClient = new SearchClient(new Uri(searchServiceEndPoint), indexName, new AzureKeyCredential(queryApiKey));
            return searchClient;
        }

        private static void DeleteIndexIfExists(string indexName, SearchIndexClient indexClient)
        {
            try
            {
                if (indexClient.GetIndex(indexName) != null)
                {
                    indexClient.DeleteIndex(indexName);
                }
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                //if exception occurred and status is "Not Found", this is work as expect
                Console.WriteLine("Failed to find index and this is because it's not there.");
            }
        }

        private static void CreateIndex(string indexName, SearchIndexClient indexClient)
        {
            FieldBuilder fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(Secret));

            var definition = new SearchIndex(indexName, searchFields);

            indexClient.CreateOrUpdateIndex(definition);
        }


        // Upload documents in a single Upload request.
        private static void UploadDocuments(SearchClient searchClient)
        {
            IndexDocumentsBatch<Secret> batch = IndexDocumentsBatch.Create(
                IndexDocumentsAction.Upload(
                    new Secret()
                    {
                        ID = "2",
                        Username = "keneil.jordan@avanade.com",
                        SecretName = "banjo",
                        Password = "1230456",
                        OrganisationID = "1",

                    }),
                IndexDocumentsAction.Upload(
                        new Secret()
                        {
                            ID = "3",
                            Username = "aaron.wray@avanade.com",
                            SecretName = "guitar",
                            Password = "1230456",
                            OrganisationID = "1",

                        }));

            try
            {
                IndexDocumentsResult result = searchClient.IndexDocuments(batch);
            }
            catch (Exception)
            {
                // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                // the batch. Depending on your application, you can take compensating actions like delaying and
                // retrying. For this simple demo, we just log the failed document keys and continue.
                Console.WriteLine("Failed to index some of the documents: {0}");
            }

            Console.WriteLine("Waiting for documents to be indexed...\n");
            Thread.Sleep(2000);
        }



        private static void RunQueries(SearchClient searchClient)
        {
            SearchResults<Secret> results;
            Console.WriteLine("Search the entire index for the term 'ken' and return the Username field:\n");
            results = searchClient.Search<Secret>("aaron.wray");
            WriteDocuments(results);
        }

        private static void WriteDocuments(SearchResults<Secret> searchResults)
        {
            foreach (SearchResult<Secret> result in searchResults.GetResults())
            {
                Console.WriteLine(result.Document.Username);
            }

            Console.WriteLine();
        }
    }
}
