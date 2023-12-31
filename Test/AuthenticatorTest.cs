﻿using CoupDeSonde.Authentication;

namespace Test
{
    /// <summary>
    ///  Cette classe regroupe les tests de la classe qui implémente l'appel au service d'authentification
    /// </summary>
    public class AuthenticatorTest
    {
        private KeyValuePair<string, string> VALID_COMBO = new KeyValuePair<string, string>("C6A39641-2F1D-45A3-8979-2115FAE82B04", "frank");
        private KeyValuePair<string, string> INVALID_COMBO = new KeyValuePair<string, string>("C6A39641-2F1D-45A3-8979-2115FAE82B06", "ERREUR");

        [Fact]
        public void AuthenticatorTest_PassingValidParameterShouldReturnName()
        {
            //Setup
            Authenticator testObject = new Authenticator();
            Assert.True(File.Exists(testObject.ServicePathGet()));

            //Test
            var authenticationResult = testObject.Authenticate(VALID_COMBO.Key);

            //Assert
            Assert.Equal(authenticationResult, VALID_COMBO.Value);
        }

        [Fact]
        public void AuthenticatorTest_PassingInValidParameterShouldReturnERREUR()
        {
            //Setup
            Authenticator testObject = new Authenticator();
            Assert.True(File.Exists(testObject.ServicePathGet()));

            //Test
            var authenticationResult = testObject.Authenticate(INVALID_COMBO.Key);

            //Assert
            Assert.Equal(authenticationResult, INVALID_COMBO.Value);
        }

    }
}