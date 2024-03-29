﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LunaCinemasBackEndInDotNet.BusinessLogic;
using LunaCinemasBackEndInDotNet.Controllers;
using LunaCinemasBackEndInDotNet.Models;
using LunaCinemasBackEndInDotNet.Persistence;
using LunaCinemasTest.MockPersistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LunaCinemasTest.MockedPersistence_tests
{
    [TestClass]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class MockedUserTests
    {
        private UserController _userController;
        private ICustomerContext _mockCustomerContext;

        [TestInitialize]
        public void CreateControllerAndMockContext()
        {
            _mockCustomerContext = new MockCustomerContext();
            _userController = new UserController(new AccountCreationService(_mockCustomerContext, new SecurityService(new MockAccessTokenContext()), new MockAdminContext()), new ExistingUserService(_mockCustomerContext, new SecurityService(new MockAccessTokenContext()),new MockAdminContext() ));
        }

        [TestMethod]
        public void UserAccountCanBeCreated()
        {
            string testFirstname = "testuser1";
            Assert.IsTrue(_mockCustomerContext.FindByEmail(testFirstname).Count == 0);
            _userController.AddCustomer(new List<string>{testFirstname, "test", "bob@gmail.com","safestpassword"});
            List<Customer> actualResult = _mockCustomerContext.FindByEmail(testFirstname);
            Assert.IsTrue(actualResult.Count == 1);
        }

        [TestMethod]
        public void UserAccountCannotBeCreatedWithoutFirstName()
        {
            ActionResult<ResponseObject<string>> actualResult =_userController.AddCustomer(new List<string> { "", "Smith", "smith@smithing.com", "safestpassword"});
            Assert.IsFalse(actualResult.Value.successful);
        }

        [TestMethod]
        public void UserAccountCannotBeCreatedWithoutPassword()
        {
            ActionResult<ResponseObject<string>> actualResult = _userController.AddCustomer(new List<string> { "hithere", "lastname", "", "0123456789012345678901234567890123456789012345678901234567891234"});
            Assert.IsFalse(actualResult.Value.successful);
        }

        [TestMethod]
        public void ResponseFromCreatingUserAccountRepresentsGuid()
        {
            ActionResult<ResponseObject<string>> actualResponse =
                _userController.AddCustomer(new List<string> { "Mr", "Anderson", "Neo@matrix.com", "testPassword"});
            try
            {
                Guid.Parse(actualResponse.Value.contentList[0]);
                Assert.IsTrue(actualResponse.Value.successful);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ResponseFromCreatingUserAccountIsValidAccessToken()
        {
            ActionResult<ResponseObject<string>> initialResponse =
                _userController.AddCustomer(new List<string> { "Mr", "Anderson", "Neo@matrix.com", "testPassword"});
            try
            {
                string accessToken = initialResponse.Value.contentList[0];
                ActionResult<ResponseObject<string>> actualResponse = _userController.VerifyAccessToken(accessToken);
                Assert.IsTrue(actualResponse.Value.successful);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void InvalidAccessTokensAreNotVerified()
        {
            ActionResult<ResponseObject<string>> actualResponse = _userController.VerifyAccessToken(Guid.NewGuid().ToString());
            Assert.IsFalse(actualResponse.Value.successful);
        }

        [TestMethod]
        public void UserCanLoginWithValidCredentials()
        {
            CreateTestCustomer("test", "test", "test@test.com", "testPassword");
            ActionResult<ResponseObject<string>> actualResponse =
                _userController.AttemptLogin(new List<string>{"testUser", "testPassword"});
            Assert.IsTrue(actualResponse.Value.successful);
            string accessToken = actualResponse.Value.contentList[0];
            Assert.IsTrue(_userController.VerifyAccessToken(accessToken).Value.successful);
        }

        [TestMethod]
        public void UserCannotLoginWithInvalidUsernameAndPassword()
        {
            CreateTestCustomer("test", "test", "test@test.com", "testPassword");
            ActionResult<ResponseObject<string>> actualResponse =
                _userController.AttemptLogin(new List<string> { "sadsf", "ffdd" });
            Assert.IsFalse(actualResponse.Value.successful);
            Assert.IsNull(actualResponse.Value.contentList);
        }

        [TestMethod]
        public void UserCannotLoginWithInvalidPassword()
        {
            CreateTestCustomer("test", "test", "test@test.com", "testPassword");
            ActionResult<ResponseObject<string>> actualResponse =
                _userController.AttemptLogin(new List<string>{ "testUser", "ffdd" });
            Assert.IsFalse(actualResponse.Value.successful);
            Assert.IsNull(actualResponse.Value.contentList);
        }

        [TestMethod]
        public void OnceUserLogsOutTokenCannotBeUsed()
        {
            string accessToken = _userController.AddCustomer(new List<string> { "bob", "twit", "thisemail", "pass"}).Value.contentList[0];
            ActionResult<ResponseObject<string>> actualResponse = _userController.Logout(accessToken);
            Assert.IsTrue(actualResponse.Value.successful);
            ActionResult<ResponseObject<string>> attemptToReuseToken = _userController.VerifyAccessToken(accessToken);
            Assert.IsFalse(attemptToReuseToken.Value.successful);
        }

        [TestMethod]
        public void IfUserLogsInWhileLoggedInThenOnlyLatestTokenCanBeUsed()
        {
            CreateTestCustomer("Jeff", "nobody", "Jeff.nobody@google.com", "nobodyspassword");
            string accessToken1 = _userController.AttemptLogin(new List<string> { "bob", "pass"}).Value.contentList[0];
            ActionResult<ResponseObject<string>> firstLoginResponse = _userController.Logout(accessToken1);
            Assert.IsTrue(firstLoginResponse.Value.successful);
            string accessToken2 = _userController.AttemptLogin(new List<string> { "bob", "pass" }).Value.contentList[0];
            ActionResult<ResponseObject<string>> secondLoginResponse = _userController.Logout(accessToken2);
            Assert.IsTrue(secondLoginResponse.Value.successful);
            ActionResult<ResponseObject<string>> attemptToReuseToken1 = _userController.VerifyAccessToken(accessToken1);
            Assert.IsFalse(attemptToReuseToken1.Value.successful);
            ActionResult<ResponseObject<string>> attemptToReuseToken2 = _userController.VerifyAccessToken(accessToken2);
            Assert.IsTrue(attemptToReuseToken2.Value.successful);
        }

        [TestMethod]
        public void UserCanBeDeleted()
        {
            CreateTestCustomer("Jeff", "nobody", "Jeff.nobody@google.com", "nobodyspassword");
            ActionResult<ResponseObject<string>> actualResponse =
                _userController.DeleteUser(new[] {"sally", "guessmyname"});
            Assert.IsTrue(actualResponse.Value.successful);
            Assert.IsTrue(_mockCustomerContext.FindByEmail("sally").Count < 1);
        }

        [TestMethod]
        public void OnceUserIsDeletedAccessTokenCannotBeVerified()
        {
            CreateTestCustomer("Jeff", "nobody", "Jeff.nobody@google.com", "nobodyspassword");
            ActionResult<ResponseObject<string>> login1Response =
                _userController.AttemptLogin(new List<string> { "nobody", "nobodyspassword"});
            Assert.IsTrue(login1Response.Value.successful);
            ActionResult<ResponseObject<string>> deletionResponse =
                _userController.DeleteUser(new[] {"nobody", "nobodyspassword"});
            Assert.IsTrue(deletionResponse.Value.successful);
            ActionResult<ResponseObject<string>> verifyAccessTokenFromLogin1 =
                _userController.VerifyAccessToken(login1Response.Value.contentList[0]);
            Assert.IsFalse(verifyAccessTokenFromLogin1.Value.successful);
        }

        [TestMethod]
        public void OnceUserIsDeletedCredentialsAreNotRecognised()
        {
            CreateTestCustomer("Jeff", "nobody", "Jeff.nobody@google.com", "nobodyspassword");
            ActionResult<ResponseObject<string>> login1Response =
                _userController.AttemptLogin(new List<string> { "nobody", "nobodyspassword" });
            Assert.IsTrue(login1Response.Value.successful);
            ActionResult<ResponseObject<string>> deletionResponse =
                _userController.DeleteUser(new [] { "nobody", "nobodyspassword" });
            Assert.IsTrue(deletionResponse.Value.successful);
            ActionResult<ResponseObject<string>> login2Response =
                _userController.AttemptLogin(new List<string> { "nobody", "nobodyspassword" });
            Assert.IsFalse(login2Response.Value.successful);
        }

        private void CreateTestCustomer(string firstName, string lastName, string email, string password)
        {
            _userController.AddCustomer(new List<string> { firstName, lastName, email, password});
        }
    }
}