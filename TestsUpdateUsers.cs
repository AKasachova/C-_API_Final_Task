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
    [AllureSuite("Update Users")]
    public class UpdateUsersTests
    {
        private RestClient _clientForReadScope;
        private RestRequest _requestForReadUsers;
        private RestRequest _requestForReadZipCodes;

        private RestClient _clientForWriteScope;
        private RestRequest _requestForWriteScope;

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
            string requestMethodParam = TestContext.Parameters["requestMethodParamForUpdateUsers"];
            Method requestMethod = Method.Put;
            if (requestMethodParam == "Patch")
            {
                requestMethod = Method.Patch; 
            }
            _requestForWriteScope = new RestRequest("/users", requestMethod);
        }

        [Test]
        [AllureDescription("Test to update user with all new values")]
        public void UpdateAnyUserWithAllValidData_CheckTheUserWasUpdated_Test()
        {
            logger.Info("Starting UpdateAnyUserWithAllValidData_CheckTheUserWasUpdated_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users, count them and select the first one" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                _clientForReadScope.AddDefaultHeader("Accept", "application/json");
                RestResponse getAllUsers = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsers.Content);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get available zip codes and select the first one" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
                string availableZipCode = availableZipCodes[0];
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Update selected user with new one(contains all new correct data) and receive the response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                var updateUser = new
                {
                    userNewValues = new
                    {
                        age = RandomUserGenerator.GenerateRandomAge(),
                        name = RandomUserGenerator.GenerateRandomName(),
                        sex = RandomUserGenerator.GenerateRandomSex(),
                        zipCode = availableZipCode

                    },
                    userToChange = new
                    {
                        age = selectedUser.Age,
                        name = selectedUser.Name,
                        sex = selectedUser.Sex,
                        zipCode = selectedUser.ZipCode
                    }
                };
                _requestForWriteScope.AddJsonBody(updateUser);

                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, JsonConvert.SerializeObject(updateUser));
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                RestResponse updateResponse = _clientForWriteScope.Execute(_requestForWriteScope);
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the response and selected user was updated with the new data" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
                bool updatedUserFound = updatedUsers.Any(u => u.Age == updateUser.userNewValues.age && u.Name == updateUser.userNewValues.name && u.Sex == updateUser.userNewValues.sex && u.ZipCode == updateUser.userNewValues.zipCode);
                bool oldUserFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
                int finalCountUsers = updatedUsers.Count;

                Assert.Multiple(() =>
                {
                    Assert.That((int)updateResponse.StatusCode, Is.EqualTo(200), "Expected status code 200 but received " + (int)updateResponse.StatusCode);
                    Assert.That(updatedUserFound, Is.True, "The user wasn't updated!");
                    Assert.That(oldUserFound, Is.False, "The user needed to be updated is still in the list of available users!");
                    Assert.That(finalCountUsers == initialCountUsers, Is.True, "Unexpected number of users after update:" + finalCountUsers + ", expected number:" + initialCountUsers);
                });
                logger.Info("UpdateAnyUserWithAllValidData_CheckTheUserWasUpdated_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        [Test]
        [AllureDescription("Test to update user with all new values and unavailable zip code")]
        [AllureIssue("BUG: The user needed not to be updated is deleted.")] 
        public void UpdateUserWithUnavailableZipCode_CheckTheUserWasNotUpdated_Test()
        {
            logger.Info("Starting UpdateUserWithUnavailableZipCode_CheckTheUserWasNotUpdated_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users, count them and select the first one" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                _clientForReadScope.AddDefaultHeader("Accept", "application/json");
                RestResponse getAllUsers = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsers.Content);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get available zip codes and create unavailable" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
                string unavailableZipCode;
                do
                {
                    unavailableZipCode = RandomUserGenerator.GenerateRandomZipCode();
                }
                while (availableZipCodes.Contains(unavailableZipCode));
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Update selected user with new one(contains unavailable zip code) and receive the response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                var updateUser = new
                {
                    userNewValues = new
                    {
                        age = RandomUserGenerator.GenerateRandomAge(),
                        name = RandomUserGenerator.GenerateRandomName(),
                        sex = RandomUserGenerator.GenerateRandomSex(),
                        zipCode = unavailableZipCode

                    },
                    userToChange = new
                    {
                        age = selectedUser.Age,
                        name = selectedUser.Name,
                        sex = selectedUser.Sex,
                        zipCode = selectedUser.ZipCode
                    }
                };
                _requestForWriteScope.AddJsonBody(updateUser);

                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, JsonConvert.SerializeObject(updateUser));
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                RestResponse updateResponse = _clientForWriteScope.Execute(_requestForWriteScope);
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the response and selected user was not updated with the new data" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
                bool updatedUserFound = updatedUsers.Any(u => u.Age == updateUser.userNewValues.age && u.Name == updateUser.userNewValues.name && u.Sex == updateUser.userNewValues.sex && u.ZipCode == updateUser.userNewValues.zipCode);
                bool oldUserFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
                int finalCountUsers = updatedUsers.Count;

                Assert.Multiple(() =>
                {
                    Assert.That((int)updateResponse.StatusCode, Is.EqualTo(424), "Expected status code 424 but received " + (int)updateResponse.StatusCode);
                    Assert.That(updatedUserFound, Is.False, "The user was updated!");
                    //The test fails, the user needed not to be updated is deleted!
                    Assert.That(oldUserFound, Is.True, "The user needed not to be updated was updated or deleted!");
                    Assert.That(finalCountUsers == initialCountUsers, Is.True, "Unexpected number of users after update:" + finalCountUsers + ", expected number:" + initialCountUsers);
                });
                logger.Info("UpdateUserWithUnavailableZipCode_CheckTheUserWasNotUpdated_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        /*Bug: The user needed not to be updated is deleted
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send PUT/PATCH request to /users endpoint and Request body contains 
         * user to update and new values and zip code is unavailable
         * 
         * Expected result: the user is not updated
         * Actual result: the user is deleted 
         */

        [Test]
        [AllureDescription("Test to update user with all new values and missed required fields")]
        [AllureIssue("BUG: The user needed not to be updated is deleted.")]
        public void UpdateAnyUserWithoutRequiredFields_CheckTheUserWasNotUpdated_Test()
        {
            logger.Info("Starting UpdateAnyUserWithoutRequiredFields_CheckTheUserWasNotUpdated_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users, count them and select the first one" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                _clientForReadScope.AddDefaultHeader("Accept", "application/json");
                RestResponse getAllUsers = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsers.Content);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get available zip codes" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
                string availableZipCode = availableZipCodes[0];
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Update selected user with new one(doesn't contain required fields) and receive the response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                var updateUser = new
                {
                    userNewValues = new
                    {
                        age = RandomUserGenerator.GenerateRandomAge(),
                        zipCode = availableZipCode

                    },
                    userToChange = new
                    {
                        age = selectedUser.Age,
                        name = selectedUser.Name,
                        sex = selectedUser.Sex,
                        zipCode = selectedUser.ZipCode
                    }
                };
                _requestForWriteScope.AddJsonBody(updateUser);

                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, JsonConvert.SerializeObject(updateUser));
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                RestResponse updateResponse = _clientForWriteScope.Execute(_requestForWriteScope);
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the response and selected user was not updated with the new data" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
                bool updatedUserFound = updatedUsers.Any(u => u.Age == updateUser.userNewValues.age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == updateUser.userNewValues.zipCode);
                bool oldUserFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
                int finalCountUsers = updatedUsers.Count;

                Assert.Multiple(() =>
                {
                    Assert.That((int)updateResponse.StatusCode, Is.EqualTo(409), "Expected status code 409 but received " + (int)updateResponse.StatusCode);
                    Assert.That(updatedUserFound, Is.False, "The user was updated!");
                    //The test fails, the user needed not to be updated is deleted!
                    Assert.That(oldUserFound, Is.True, "The  user needed not to be updated was updated or deleted!");
                    Assert.That(finalCountUsers == initialCountUsers, Is.True, "Unexpected number of users after update:" + finalCountUsers + ", expected number:" + initialCountUsers);
                });
                logger.Info("UpdateAnyUserWithoutRequiredFields_CheckTheUserWasNotUpdated_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        /*Bug: The user needed not to be updated is deleted
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send PUT/PATCH request to /users endpoint and Request body contains 
         * user to update and required fields are missed
         * 
         * Expected result: the user is not updated
         * Actual result: the user is deleted 
         */
        [TearDown]
        public void TearDown()
        {
            logger.Info("Tearing down tests...");
            LogManager.Shutdown();
        }
    }
}
