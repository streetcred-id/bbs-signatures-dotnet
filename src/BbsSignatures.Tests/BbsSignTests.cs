﻿using System;
using NUnit.Framework;

namespace BbsSignatures.Tests
{
    public class BbsSignTests
    {
        [Test(Description = "Get signature size")]
        public void GetSignatureSize()
        {
            var result = Native.bbs_signature_size();

            Assert.AreEqual(112, result);
        }

        [Test(Description = "Sign message")]
        public void SignSingleMessageUsingApi()
        {
            var myKey = BbsProvider.GenerateBlsKey();

            var signature = BbsProvider.Sign(myKey, new[] { "message" });

            Assert.NotNull(signature);
            Assert.AreEqual(signature.Length, Native.bbs_signature_size());
        }

        [Test(Description = "Sign multiple messages")]
        public void SignMultipleeMessages()
        {
            var keyPair = BbsProvider.GenerateBlsKey();

            var signature = BbsProvider.Sign(keyPair, new[] { "message_1", "message_2" });

            Assert.NotNull(signature);
            Assert.AreEqual(BbsProvider.SignatureSize, signature.Length);
        }

        [Test(Description = "Verify throws if invalid signature")]
        public void VerifyThrowsIfInvalidSignature()
        {
            var secretKey = BbsProvider.GenerateBlsKey();
            var publicKey = secretKey.GenerateBbsKey(1);

            Assert.Throws<BbsException>(() => BbsProvider.Verify(publicKey, new[] { "message_0" }, Array.Empty<byte>()), "Signature cannot be empty array");
        }

        [Test(Description = "Sign message with one public key, verify with another")]
        public void SignAndVerifyDifferentKeys()
        {
            var keyPair = BbsProvider.GenerateBlsKey();
            var messages = new[] { "message_1", "message_2" };

            var signature = BbsProvider.Sign(keyPair, messages);

            var result = BbsProvider.Verify(keyPair.GenerateBbsKey(2), messages, signature);
            Assert.True(result);
        }
    }
}
