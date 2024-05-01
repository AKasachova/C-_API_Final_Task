using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using RestSharp;
using NLog;
using NUnit.Framework.Internal;
using NUnit.Allure.Core;
using NUnit.Allure.Attributes;
using Allure.Commons;

namespace APIAutomation.Tests
{
    [TestFixture]
    [AllureNUnit]
    [AllureSuite("Upload Users")]
    public class UploadUsersTests
    {
        private RestClient _clientForReadScope;
        private RestRequest _requestForReadScopeGetUsers;
        private RestRequest _requestForReadScopeGetZipCodes;

        private RestClient _clientForWriteScope;
        private RestRequest _requestForWriteScope;

        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void Setup()
        {
            logger.Info("Setting up tests...");

            var clientForReadScope = ClientForReadScope.GetInstance();
            _clientForReadScope = clientForReadScope.GetRestClient();
            _requestForReadScopeGetZipCodes = new RestRequest("/zip-codes");
            _requestForReadScopeGetUsers = new RestRequest("/users");

            var clientForWriteScope = ClientForWriteScope.GetInstance();
            _clientForWriteScope = clientForWriteScope.GetRestClient();
            _requestForWriteScope = new RestRequest("/users/upload", Method.Post);
        }

        [Test]
        [AllureDescription("Test to upload users with all correct data")]
        public void CheckAllUsersAreReplacedWithFiled_Test()
        {
            logger.Info("Starting CheckAllUsersAreReplacedWithFiled_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                _clientForReadScope.AddDefaultHeader("Accept", "application/json");
                RestResponse getAllUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
                List<User> initialUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsersResponse.Content);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get all available zip codes" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadScopeGetZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Create file with new users(all correct data) and upload it in the service, get response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                List<User> users = new List<User>
                 {
                     new User{Age = RandomUserGenerator.GenerateRandomAge(), Name = RandomUserGenerator.GenerateRandomName(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = availableZipCodes[0]},
                     new User{Age = RandomUserGenerator.GenerateRandomAge(), Name = RandomUserGenerator.GenerateRandomName(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = availableZipCodes[1]}
                 };
                var usersCountFile = users.Count;

                string currentDirectory = Directory.GetCurrentDirectory();
                string jsonFilePath = Path.Combine(Directory.GetParent(currentDirectory).Parent.Parent.FullName, "Test_data", "Users.json");

                var usersJson = JsonConvert.SerializeObject(users, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                File.WriteAllText(jsonFilePath, usersJson);

                _requestForWriteScope.AddFile("file", jsonFilePath, "multipart/form-data");

                string tempFilePath = Path.GetTempFileName();
                string usersFile = File.ReadAllText(jsonFilePath);
                File.WriteAllText(tempFilePath, usersJson);
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                RestResponse postResponse = _clientForWriteScope.Execute(_requestForWriteScope);
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the response and all users are replaced with users from file " };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                RestResponse getAllUpdatedUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getAllUpdatedUsersResponse.Content);
                var jsonResponse = getAllUpdatedUsersResponse.Content;
                dynamic responseObject = JsonConvert.DeserializeObject(jsonResponse);
                int uploadedUsersCount = responseObject.Count;

                Assert.Multiple(() =>
                {
                    Assert.That((int)postResponse.StatusCode, Is.EqualTo(201), "Expected status code 201 (Created) but received " + (int)postResponse.StatusCode);
                    Assert.That(updatedUsers, Is.EquivalentTo(users), "Received users list doesn't correspond expected one!");
                    Assert.That(uploadedUsersCount, Is.EqualTo(usersCountFile), "The number of users in the respond is not equivalent to the number of sent users!");
                });
                logger.Info("CheckAllUsersAreReplacedWithFiled_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        [Test]
        [AllureDescription("Test to upload users and at least one user has incorrect (unavailable) zip code.")]
        [AllureIssue("BUG: The system displays 500 code after uploading json file with user with unavailable Zip Code.")]
        public void CheckUsersAreNotAdded_UnavailableZipCode_Test()
        {
            logger.Info("Starting CheckUsersAreNotAdded_UnavailableZipCode_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                _clientForReadScope.AddDefaultHeader("Accept", "application/json");
                RestResponse getAllUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
                List<User> initialUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsersResponse.Content);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get all available zip codes and create unavailable" };
                RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadScopeGetZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
                string unavailableZipCode;
                do
                {
                    unavailableZipCode = RandomUserGenerator.GenerateRandomZipCode();
                }
                while (availableZipCodes.Contains(unavailableZipCode));
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Create file with new users(at least one user has unavailable zip code) and upload it in the service, get response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                List<User> users = new List<User>
                 {
                     new User{Age = RandomUserGenerator.GenerateRandomAge(), Name = RandomUserGenerator.GenerateRandomName(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = availableZipCodes[0]},
                     new User{Age = RandomUserGenerator.GenerateRandomAge(), Name = RandomUserGenerator.GenerateRandomName(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = unavailableZipCode}
                 };

                string currentDirectory = Directory.GetCurrentDirectory();
                string jsonFilePath = Path.Combine(Directory.GetParent(currentDirectory).Parent.Parent.FullName, "Test_data", "Users.json");

                var usersJson = JsonConvert.SerializeObject(users, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                File.WriteAllText(jsonFilePath, usersJson);
                _requestForWriteScope.AddFile("file", jsonFilePath, "multipart/form-data");

                string tempFilePath = Path.GetTempFileName();
                string usersFile = File.ReadAllText(jsonFilePath);
                File.WriteAllText(tempFilePath, usersJson);
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                RestResponse postResponse = _clientForWriteScope.Execute(_requestForWriteScope);
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the response and users are not uploaded" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                RestResponse getAllUpdatedUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getAllUpdatedUsersResponse.Content);

                Assert.Multiple(() =>
                {
                    //Test fails: Status code is 500
                    Assert.That((int)postResponse.StatusCode, Is.EqualTo(424), "Expected status code 424 but received " + (int)postResponse.StatusCode);
                    //Test fails: Received list contains the only one user sent in the file (user with available Zip Code)
                    Assert.That(updatedUsers, Is.EquivalentTo(initialUsers), "Received users list doesn't correspond expected one!");
                });
                logger.Info("CheckUsersAreNotAdded_UnavailableZipCode_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        /*Bug: The system displays 500 code after uploading json file with user with unavailable Zip Code
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send POST request to /users endpoint and Request body contains 
         * json file with users and at least one user has incorrect (unavailable) zip code
         * 
         * Expected result: there is 424 response code
         * Actual result: there is 500 response code*/


        /*Bug: Received list contains the only one user sent in the file (user with available Zip Code) 
         * after uploading json file with user with unavailable Zip Code
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send POST request to /users endpoint and Request body contains json file with
         * two users and 1st user has incorrect (unavailable) zip code, 2nd - available zip code
         * 
         * Expected result: initial list of users wasn't updated
         * Actual result: initial list of users was updated: Received list contains the only one user from the file (user with available Zip Code)*/

        [Test]
        [AllureDescription("Test to upload users and at least one user has missed required field.")]
        [AllureIssue("BUG: The system displays 500 code after uploading json file with user with unavailable zip code.")]
        public void CheckUsersAreNotAdded_MissedRequiredField_Test()
        {
            logger.Info("Starting CheckUsersAreNotAdded_MissedRequiredField_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                _clientForReadScope.AddDefaultHeader("Accept", "application/json");
                RestResponse getAllUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
                List<User> initialUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsersResponse.Content);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get all available zip codes" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadScopeGetZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Create file with new users(at least one user has missed required field) and upload it in the service, get response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                List<User> users = new List<User>
                        {
                            new User{Age = RandomUserGenerator.GenerateRandomAge(), Name = RandomUserGenerator.GenerateRandomName(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = availableZipCodes[0]},
                            new User{Age = RandomUserGenerator.GenerateRandomAge(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = availableZipCodes[1]}
                        };

                string currentDirectory = Directory.GetCurrentDirectory();
                string jsonFilePath = Path.Combine(Directory.GetParent(currentDirectory).Parent.Parent.FullName, "Test_data", "Users.json");

                var usersJson = JsonConvert.SerializeObject(users, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                File.WriteAllText(jsonFilePath, usersJson);


                _requestForWriteScope.AddFile("file", jsonFilePath, "multipart/form-data");

                string tempFilePath = Path.GetTempFileName();
                string usersFile = File.ReadAllText(jsonFilePath);
                File.WriteAllText(tempFilePath, usersJson);
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                RestResponse postResponse = _clientForWriteScope.Execute(_requestForWriteScope);
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the response and users are not uploaded" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                RestResponse getAllUpdatedUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getAllUpdatedUsersResponse.Content);

                Assert.Multiple(() =>
                {
                    //Test fails: Status code is 500
                    Assert.That((int)postResponse.StatusCode, Is.EqualTo(409), "Expected status code 409 but received " + (int)postResponse.StatusCode);
                    //Test fails: Received list contains the only one user sent in the file (user with all required fields)
                    Assert.That(updatedUsers, Is.EquivalentTo(initialUsers), "Received users list doesn't correspond expected one!");
                });
                logger.Info("CheckUsersAreNotAdded_MissedRequiredField_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        /*Bug: The system displays 500 code after uploading json file with user with missed required field
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send POST request to /users endpoint and Request body contains 
         * json file with users and at least one user has missed required field
         * 
         * Expected result: there is 424 response code
         * Actual result: there is 500 response code*/


        /*Bug: Received list contains the only one user sent in the file (user with all correct data) 
         * after uploading json file with user with missed required field
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send POST request to /users endpoint and Request body contains json file with
         * two users and 1st user has all correct data, 2nd - missed required field
         * 
         * Expected result: initial list of users wasn't updated
         * Actual result: initial list of users was updated: Received list contains the only one user from the file (user with all correct data)*/
        [TearDown]
        public void TearDown()
        {
            logger.Info("Tearing down tests...");
            LogManager.Shutdown();
        }
    }
}
