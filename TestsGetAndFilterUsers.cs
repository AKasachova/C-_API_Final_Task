using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using NLog;
using NUnit.Allure.Core;
using NUnit.Allure.Attributes;
using Allure.Commons;

namespace APIAutomation.Tests
{
    [TestFixture]
    [AllureNUnit]
    [AllureSuite("Filter Users")]
    public class GetAndFilterUsersTests
    {
        private RestClient _client;
        private RestRequest _request;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void Setup()
        {
            "GetAndFilterUsersTests".LogInfo("Setting up tests...");

            var client = ClientForReadScope.GetInstance();
            _client = client.GetRestClient();
            _request = new RestRequest("/users");
        }

        [Test]
        [AllureDescription("Test to get all expected users stored in the application for now")]
        public void GetAllUsers_ReturnsAllExpectedUsers_Test()
        {
            "GetAllUsers_ReturnsAllExpectedUsers_Test".LogInfo("Starting the test...");

            try 
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users stored currently" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                _client.AddDefaultHeader("Accept", "application/json");
                RestResponse response = _client.Execute(_request);
                List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(response.Content);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the GET response and all recieved users correspond to all expected to receive" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                var expectedUsers = new List<User>
                {
                    new User { Name = "Emma Jones", Age = 6, Sex = "MALE", ZipCode = "12345" },
                    new User { Name = "James Davis", Age = 73, Sex = "MALE", ZipCode = "23456" },
                    new User { Name = "James Brown", Age = 58, Sex = "MALE", ZipCode = "ABCDE" },
                    new User { Name = "David Smith", Age = 24, Sex = "MALE", ZipCode = null },
                    new User { Name = "Sophia Miller", Age = 59, Sex = "FEMALE", ZipCode = null }
                };

                Assert.Multiple(() =>
                {
                    Assert.That((int)response.StatusCode, Is.EqualTo(200));
                    Assert.That(actualUsers, Is.EquivalentTo(expectedUsers), "Received users list doesn't correspond expected one!");
            
                });

                "GetAllUsers_ReturnsAllExpectedUsers_Test".LogInfo("The test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                "GetAllUsers_ReturnsAllExpectedUsers_Test".LogError($"An error occured: {ex.Message}");
            }
        }

        [Test]
        [AllureDescription("Test to get all users older than set parameter")]
        public void GetFilteredUsersOlderThan_ReturnsAllExpectedUsers_Test()
        {
            "GetFilteredUsersOlderThan_ReturnsAllExpectedUsers_Test".LogInfo("Starting the test...");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users older than set parameter" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                int olderThan = 60;
                _request.AddParameter("olderThan", olderThan);

                _client.AddDefaultHeader("Accept", "application/json");
                RestResponse response = _client.Execute(_request);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the GET response and all recieved filtered (older than) users correspond to all expected to receive" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                var expectedUsers = new List<User>
                {
                    new User { Name = "James Davis", Age = 73, Sex = "MALE", ZipCode = "23456" },
                };

                List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(response.Content);

                Assert.Multiple(() =>
                {
                    Assert.That((int)response.StatusCode, Is.EqualTo(200));
                    Assert.That(actualUsers, Is.EquivalentTo(expectedUsers), "Received users list doesn't correspond expected one!");
                });
                "GetFilteredUsersOlderThan_ReturnsAllExpectedUsers_Test".LogInfo("The test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                "GetFilteredUsersOlderThan_ReturnsAllExpectedUsers_Test".LogError($"Error occured: {ex.Message}");
            }
        }

        [Test]
        [AllureDescription("Test to get all users yanger than set parameter")]
        public void GetFilteredUsersYoungerThan_ReturnsAllExpectedUsers_Test()
        {
            "GetFilteredUsersYoungerThan_ReturnsAllExpectedUsers_Test".LogInfo("Starting the test...");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users younger than set parameter" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                int youngerThan = 1;
                _request.AddParameter("youngerThan", youngerThan);

                _client.AddDefaultHeader("Accept", "application/json");
                RestResponse response = _client.Execute(_request);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the GET response and all recieved filtered (younger than) users correspond to all expected to receive" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                var expectedUsers = new List<User> { };

                List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(response.Content);

                Assert.Multiple(() =>
                {
                    Assert.That((int)response.StatusCode, Is.EqualTo(200));
                    Assert.That(actualUsers, Is.EquivalentTo(expectedUsers), "Received users list doesn't correspond expected one!");
                });

                "GetFilteredUsersYoungerThan_ReturnsAllExpectedUsers_Test".LogInfo("The test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                "GetFilteredUsersYoungerThan_ReturnsAllExpectedUsers_Test".LogError($"Error occured: {ex.Message}");
            }
        }

        [Test]
        [AllureDescription("Test to get all users with certain sex as set parameter")]
        public void GetFilteredUsersSex_ReturnsAllExpectedUsers_Test()
        {
            "GetFilteredUsersSex_ReturnsAllExpectedUsers_Test".LogInfo("Starting the test...");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users with certain sex as set parameter" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                string sex = "FEMALE";
                _request.AddParameter("sex", sex);

                _client.AddDefaultHeader("Accept", "application/json");
                RestResponse response = _client.Execute(_request);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the GET response and all recieved filtered (by sex) users correspond to all expected to receive" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                var expectedUsers = new List<User>
                {
                    new User { Name = "Sophia Miller", Age = 59, Sex = "FEMALE", ZipCode = null }
                };

                List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(response.Content);

                Assert.Multiple(() =>
                {
                    Assert.That((int)response.StatusCode, Is.EqualTo(200));
                    Assert.That(actualUsers, Is.EquivalentTo(expectedUsers), "Received users list doesn't correspond expected one!");
                });

                "GetFilteredUsersSex_ReturnsAllExpectedUsers_Test".LogInfo("The test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                "GetFilteredUsersSex_ReturnsAllExpectedUsers_Test".LogError($"Error occured: {ex.Message}");
            }
        }

        [TearDown]
        public void TearDown()
        {
            "GetAndFilterUsersTests".LogInfo("Tearing down tests...");
            LogManager.Shutdown();
        }
    }  
}
