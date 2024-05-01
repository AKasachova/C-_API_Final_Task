using Newtonsoft.Json;
using NLog;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using RestSharp;
using System.Net;
using NUnit.Allure.Core;
using NUnit.Allure.Attributes;
using Allure.Commons;

namespace APIAutomation.Tests
{
    [TestFixture]
    [AllureNUnit]
    [AllureSuite("ZipCode Endpoint GET")]
    public class ZipCodeEndpointGETTests
    {
        private RestClient _client;
        private RestRequest _request;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void Setup()
        {
            logger.Info("Setting up tests...");

            var client = ClientForReadScope.GetInstance();
            _client = client.GetRestClient();
            _request = new RestRequest("/zip-codes");
        }

        [Test]
        [AllureDescription("Test to get all available zip codes in the app for now")]
        public void GetAllZipCodes_ReturnsAllAvailableZipCodes_Test()
        {
            logger.Info("Starting GetAllZipCodes_ReturnsAllAvailableZipCodes_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: get all available zip codes in the app for now" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                _client.AddDefaultHeader("Accept", "application/json");
                RestResponse response = _client.Execute(_request);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the GET response and all recieved zip codes correspond to all expected to receive" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                List<string> expectedZipCodes = new List<string> { "12345", "23456", "ABCDE" };
                List<string> actualZipCodes = JsonConvert.DeserializeObject<List<string>>(response.Content);

                Assert.Multiple(() =>
                {
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
                    Assert.That(expectedZipCodes.Count, Is.EqualTo(actualZipCodes.Count), "Number of zip codes in response doesn't match expected count!");
                    Assert.That(actualZipCodes.Distinct().Count(), Is.EqualTo(expectedZipCodes.Count), "Response contains duplicate zip codes.");

                    foreach (string zipCode in actualZipCodes)
                    {
                        CollectionAssert.Contains(expectedZipCodes, zipCode, $"Unexpected zip code '{zipCode}' found in response.");
                    }
                });
                logger.Info("GetAllZipCodes_ReturnsAllAvailableZipCodes_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }
    }

    [TestFixture]
    [AllureNUnit]
    [AllureSuite("ZipCode Endpoint POST")]
    public class ZipCodeEndpointPOSTTests
    {
        private RestClient _client;
        private RestRequest _request;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void Setup()
        {
            logger.Info("Setting up tests...");

            var client = ClientForWriteScope.GetInstance();
            _client = client.GetRestClient();
            _request = new RestRequest("/zip-codes/expand", Method.Post);
        }
        private string[] GenerateUniqueZipCodes(int count)
        {
            List<string> zipCodes = new List<string>();

            for (int i = 0; i < count; i++)
            {
                zipCodes.Add(Guid.NewGuid().ToString().Substring(0, 5));
            }

            return zipCodes.ToArray();
        }

        private static string[] GenerateUniqueDataWithDuplicatesForAvailableZipCodes(int count)
        {
            List<string> zipCodes = new List<string>();
            for (int i = 0; i < count; i++)
            {
                string randomZipCode = Guid.NewGuid().ToString().Substring(0, 5);
                zipCodes.Add(randomZipCode);

                if (new Random().Next(2) == 0)
                {
                    zipCodes.Add(randomZipCode);
                }
            }
            return zipCodes.ToArray();
        }

        [Test]
        [AllureDescription("Test to add zip codes to available zip codes of the app")]
        public void PostZipCodes_ReturnsAllAddedZipCodes_Test()
        {
            logger.Info("Starting PostZipCodes_ReturnsAllAddedZipCodes_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Generate unique zip codes and send them to the service and receive response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                string[] sentZipCodes = GenerateUniqueZipCodes(6);
                string requestBody = JsonConvert.SerializeObject(sentZipCodes);

                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, requestBody);
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                _request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
                _client.AddDefaultHeader("Accept", "application/json");

                RestResponse response = _client.Execute(_request);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the response and Zip codes from request body are added to available zip codes of application." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                string[] actualZipCodes = JsonConvert.DeserializeObject<string[]>(response.Content);

                Assert.Multiple(() =>
                {
                    Assert.That((int)response.StatusCode, Is.EqualTo(201));
                    Assert.That(sentZipCodes.All(actualZipCodes.Contains), Is.True, "Sent zip codes are not equal to actual zip codes.");
                });
                logger.Info("PostZipCodes_ReturnsAllAddedZipCodes_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }
        
        [Test]
        [AllureDescription("Test to add zip codes to available zip codes of the app with duplications of for available zip codes.")]
        [AllureIssue("BUG: The system displays duplicates in available zip codes in response when the request has dublicates in available zip codes.")]
        public void PostZipCodes_SendDuplicatesForAvailableZipCodes_Test()
        {
            logger.Info("Starting PostZipCodes_ReturnsAllAddedZipCodes_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Generate unique zip codes with duplicates and send them to the service and receive response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                string[] sentZipCodes = GenerateUniqueDataWithDuplicatesForAvailableZipCodes(6);
                string requestBody = JsonConvert.SerializeObject(sentZipCodes);

                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, requestBody);
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                _request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
                _client.AddDefaultHeader("Accept", "application/json");

                RestResponse response = _client.Execute(_request);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the response and zip codes from request body are added to available zip codes of application and there are no duplications in available zip codes." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                string[] actualZipCodes = JsonConvert.DeserializeObject<string[]>(response.Content);

                bool hasDuplicates = actualZipCodes.GroupBy(x => x).Any(g => g.Count() > 1);

                Assert.Multiple(() =>
                {
                    Assert.That((int)response.StatusCode, Is.EqualTo(201));
                    Assert.That(sentZipCodes.All(actualZipCodes.Contains), Is.True, "Sent zip codes are not equal to actual zip codes.");
                    Assert.That(hasDuplicates, Is.False, "Duplicate zip codes found among the received zip codes.");
                });
                logger.Info("PostZipCodes_ReturnsAllAddedZipCodes_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        /*Bug: The system displays duplicates in available zip codes in response when the request has dublicates in available zip codes
         * 
         * Preconditions:
         * -the user is authorized
         * 
         * Steps:
         * 1. Send POST request to /zip-codes endpoint and request body contains list of zip codes 
         * which has duplications for available zip codes
         * 
         * Expected result: there are no duplications in available zip codes for the response 
         * Actual result: there are duplications in available zip codes for the response  
         */

        [Test]
        [AllureDescription("Test to add zip codes which have duplications for already used zip codes to available zip codes of the app.")]
        [AllureIssue("BUG:The system displays duplications between available zip codes and already used zip codes (request has duplications for already used zip codes).")]
        public void PostZipCodes_SendDuplicatesForExistingZipCodes_Test()
        {
            logger.Info("Starting PostZipCodes_SendDuplicatesForExistingZipCodes_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Generate list of zip codes(existing and unique) and send them to the service and receive response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                string[] existingZipCodes = new string[] { "12345", "23456", "ABCDE" };
                string[] newZipCodes = GenerateUniqueZipCodes(3);
                string[] combinedZipCodes = existingZipCodes.Concat(newZipCodes).ToArray();

                string requestBody = JsonConvert.SerializeObject(combinedZipCodes);

                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, requestBody);
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                _request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
                _client.AddDefaultHeader("Accept", "application/json");

                RestResponse response = _client.Execute(_request);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the response and zip codes from request body are added to available zip codes of application and there are no duplications between available zip codes and already used zip codes." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                string[] actualZipCodes = JsonConvert.DeserializeObject<string[]>(response.Content);
                bool hasDuplicatesForExistingZipCodes = existingZipCodes.Any(zipCode => actualZipCodes.Count(code => code == zipCode) > 1);

                Assert.Multiple(() =>
                {
                    Assert.That((int)response.StatusCode, Is.EqualTo(201));
                    Assert.That(hasDuplicatesForExistingZipCodes, Is.False, "Duplicate zip codes found among the existing zip codes in the received zip codes.");
                });
                logger.Info("PostZipCodes_SendDuplicatesForExistingZipCodes_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        /*Bug: The system displays duplications between available zip codes and already used zip codes (request has duplications for already used zip codes )
         * 
         * Preconditions:
         * -the user is authorized
         * 
         * Steps:
         * 1. Send POST request to /zip-codes endpoint and request body contains list of zip codes 
         * which has duplications for existing zip codes
         * 
         * Expected result: there are no duplications between available zip codes and already used zip codes for the response 
         * Actual result: there are duplications between available zip codes and already used zip codes for the response 
         */
    }
}
