using Allure.Commons;
using Newtonsoft.Json;
using NLog;
using NUnit.Allure.Attributes;
using NUnit.Allure.Core;
using NUnit.Framework;
using RestSharp;

namespace APIAutomation.Tests
{
    [TestFixture]
    [AllureNUnit]
    [AllureSuite("Delete Users")]
    public class DeleteUsersTests
    {
        private RestClient _clientForReadScope;
        private RestRequest _requestForReadUsers;
        private RestRequest _requestForReadZipCodes;

        private RestClient _clientForWriteScope;
        private RestRequest _requestForWriteScope;

        private RestResponse deleteResponse;
        private User selectedUser;
        private int initialCountUsers;
        private int initialCountZipCodes;
        private List<User> allUsers;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void Setup()
        {
            logger.Info("Setting up tests...");

            var clientForReadScope = ClientForReadScope.GetInstance();
            _clientForReadScope = clientForReadScope.GetRestClient();
            _requestForReadUsers = new RestRequest("/users");
            _requestForReadZipCodes = new RestRequest("/zip-codes");

            var clientForWriteScope = ClientForWriteScope.GetInstance();
            _clientForWriteScope = clientForWriteScope.GetRestClient();
            _requestForWriteScope = new RestRequest("/users ", Method.Delete);
        }

        [Test]
        [AllureDescription("Test to delete any user and verify that it was deleted and Zip code is returned in list of available zip codes")]
        public void DeleteAnyUser_CheckTheUserWasDeleted_Test()
        {
            logger.Info("Starting DeleteAnyUser_CheckTheUserWasDeleted_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users, count them and select the first one" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                _clientForReadScope.AddDefaultHeader("Accept", "application/json");
                RestResponse getAllUsers = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsers.Content);
                initialCountUsers = allUsers.Count;
                selectedUser = allUsers[0];
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get all available zip codes, count them" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
                initialCountZipCodes = availableZipCodes.Count;
                AllureLifecycle.Instance.StopStep();

                
                StepResult step3 = new StepResult { name = "Step#3: Delete user and receive the response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                _requestForWriteScope.AddJsonBody(selectedUser);
              
                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, JsonConvert.SerializeObject(selectedUser));
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                deleteResponse = _clientForWriteScope.Execute(_requestForWriteScope);
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the Delete response And User is deleted And Zip code is returned in list of available zip codes" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
                bool userFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
                int finalCount = updatedUsers.Count;

                RestResponse getZipCodesResponseUpdated = _clientForReadScope.Execute(_requestForReadZipCodes);
                List<string> availableZipCodesUpdated = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponseUpdated.Content);
                int finalCountZipCodes = availableZipCodesUpdated.Count;
                
                Assert.Multiple(() =>
                {
                    Assert.That((int)deleteResponse.StatusCode, Is.EqualTo(204), "Expected status code 204 but received " + (int)deleteResponse.StatusCode);
                    Assert.That(userFound, Is.False, "The user wasn't deleted!");
                    Assert.That(finalCount == initialCountUsers - 1, Is.True, "Unexpected number of users after deletion:" + finalCount + ", expected number:" + initialCountUsers);
                    Assert.That(availableZipCodesUpdated.Contains(selectedUser.ZipCode) && finalCountZipCodes == initialCountZipCodes + 1, Is.True,
                    $"Zip Code of deleted user wasn't added in list of available Zip Codes.\n" +
                    $"Zip Code needed to be added in the list: {selectedUser.ZipCode}\n" +
                    $"Number of available Zipcodes before deletion: {initialCountZipCodes}, after deletion: {finalCountZipCodes}");
                });
 
                logger.Info("DeleteAnyUser_CheckTheUserWasDeleted_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured {0}", ex.Message);
            }
        }

        [Test]
        [AllureDescription("Test to delete user by it's required data and verify that it was deleted and Zip code is returned in list of available zip codes")]
        [AllureIssue("BUG: The user wasn't deleted by sending delete request with the only required fields.")]
        public void DeleteAnyUserByRequiredFieldsOnly_CheckTheUserWasDeleted_Test()
        {
            logger.Info("Starting DeleteAnyUserByRequiredFieldsOnly_CheckTheUserWasDeleted_Test");
            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users, count them and select the first one, take it's required fields data" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                _clientForReadScope.AddDefaultHeader("Accept", "application/json");
                RestResponse getAllUsers = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsers.Content);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];
                var userToDelete = new
                {
                    Name = selectedUser.Name,
                    Sex = selectedUser.Sex
                };
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get all available zip codes, count them" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
                int initialCountZipCodes = availableZipCodes.Count;
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Delete user and receive the response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                _requestForWriteScope.AddJsonBody(userToDelete);

                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, JsonConvert.SerializeObject(selectedUser));
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                RestResponse deleteResponse = _clientForWriteScope.Execute(_requestForWriteScope);
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the Delete response And User is deleted And Zip code is returned in list of available zip codes" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
                bool userFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
                int finalCount = updatedUsers.Count;

                RestResponse getZipCodesResponseUpdated = _clientForReadScope.Execute(_requestForReadZipCodes);
                List<string> availableZipCodesUpdated = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponseUpdated.Content);
                int finalCountZipCodes = availableZipCodesUpdated.Count;

                Assert.Multiple(() =>
                {
                    Assert.That((int)deleteResponse.StatusCode, Is.EqualTo(204), "Expected status code 204 but received " + (int)deleteResponse.StatusCode);

                    //The test fails (the user wasn't deleted)
                    Assert.That(userFound, Is.False, "The user wasn't deleted!");
                    Assert.That(finalCount == initialCountUsers - 1, Is.True, "Unexpected number of users after deletion:" + finalCount + ", expected number:" + initialCountUsers);
                    Assert.That(availableZipCodesUpdated.Contains(selectedUser.ZipCode) && finalCountZipCodes == initialCountZipCodes + 1, Is.True,
                    $"Zip Code of deleted user wasn't added in list of available Zip Codes.\n" +
                    $"Zip Code needed to be added in the list: {selectedUser.ZipCode}\n" +
                    $"Number of available Zipcodes before deletion: {initialCountZipCodes}, after deletion: {finalCountZipCodes}");
                });
                logger.Info("DeleteAnyUserByRequiredFieldsOnly_CheckTheUserWasDeleted_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured {0}", ex.Message);
            }
        }

        /*Bug: The user wasn't deleted by sending delete request with the only required fields
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send DELETE request to /users endpoint and Request body contains 
         * the only required fields (name and sex) for the user sent
         * 
         * Expected result: the user was deleted
         * Actual result: the user wasn't deleted 
         */

        [Test]
        [AllureDescription("Test to delete user(any required field is missed) and verify that it wasn't deleted")]
        public void DeleteAnyUserWithMissedRequiredField_CheckTheUserWasNotDeleted_Test()
        {
            logger.Info("Starting DeleteAnyUserWithMissedRequiredField_CheckTheUserWasNotDeleted_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users, count them and select the first one to take it's data without any required field" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                _clientForReadScope.AddDefaultHeader("Accept", "application/json");
                RestResponse getAllUsers = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsers.Content);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];
                var userToDelete = new
                {
                    Age = selectedUser.Age,
                    Sex = selectedUser.Sex,
                    ZipCode = selectedUser.ZipCode,
                };
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get all available zip codes, count them" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
                int initialCountZipCodes = availableZipCodes.Count;
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Delete user and receive the response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                _requestForWriteScope.AddJsonBody(userToDelete);

                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, JsonConvert.SerializeObject(selectedUser));
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                RestResponse deleteResponse = _clientForWriteScope.Execute(_requestForWriteScope);
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the Delete response And User isn't deleted And Zip code wasn't deleted from  list of available zip codes" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
                bool userFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
                int finalCount = updatedUsers.Count;

                RestResponse getZipCodesResponseUpdated = _clientForReadScope.Execute(_requestForReadZipCodes);
                List<string> availableZipCodesUpdated = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponseUpdated.Content);
                int finalCountZipCodes = availableZipCodesUpdated.Count;

                Assert.Multiple(() =>
                {
                    Assert.That((int)deleteResponse.StatusCode, Is.EqualTo(409), "Expected status code 409 but received " + (int)deleteResponse.StatusCode);
                    Assert.That(userFound, Is.True, "The user wasn't deleted!");
                    Assert.That(finalCount == initialCountUsers, Is.True, "Unexpected number of users after deletion:" + finalCount + ", expected number:" + initialCountUsers);
                    Assert.That(!availableZipCodesUpdated.Contains(selectedUser.ZipCode) && finalCountZipCodes == initialCountZipCodes, Is.True,
                    $"Zip Code of not deleted user was added in list of available Zip Codes.\n" +
                    $"Zip Code needed not to be added in the list: {selectedUser.ZipCode}\n" +
                    $"Number of available Zipcodes before deletion: {initialCountZipCodes}, after deletion: {finalCountZipCodes}");
                });
                AllureLifecycle.Instance.StopStep();
                logger.Info("DeleteAnyUserWithMissedRequiredField_CheckTheUserWasNotDeleted_Test completed successfully.");

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured {0}", ex.Message);
            }
        }

        [TearDown]
        public void TearDown()
        {
            logger.Info("Tearing down tests...");
            LogManager.Shutdown();
        }
    }
}
