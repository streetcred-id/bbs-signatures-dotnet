﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BbsSignatures
{
    public class BbsProvider
    {
        public static int SignatureSize => Native.bbs_signature_size();

        public static int BlindSignatureSize => Native.bbs_blind_signature_size();

        /// <summary>
        /// Signs the messages
        /// </summary>
        /// <param name="secretKey">My key.</param>
        /// <param name="messages">The messages.</param>
        /// <returns></returns>
        public static byte[] Sign(BlsKey blsKey, string[] messages)
        {
            if (blsKey.SecretKey is null) throw new BbsException("Secret key not found");

            var publicKey = blsKey.GenerateBbsKey((uint)messages.Length);

            using var context = new UnmanagedMemoryContext();

            var handle = Native.bbs_sign_context_init(out var error);
            context.ThrowIfNeeded(error);

            foreach (var message in messages)
            {
                Native.bbs_sign_context_add_message_string(handle, message, out error);
                context.ThrowIfNeeded(error);
            }

            context.Reference(publicKey.ToArray(), out var publicKey_);
            Native.bbs_sign_context_set_public_key(handle, publicKey_, out error);
            context.ThrowIfNeeded(error);

            context.Reference(blsKey.SecretKey.ToArray(), out var secretKey_);
            Native.bbs_sign_context_set_secret_key(handle, secretKey_, out error);
            context.ThrowIfNeeded(error);

            Native.bbs_sign_context_finish(handle, out var signature, out error);
            context.ThrowIfNeeded(error);

            context.Dereference(signature, out var signature_);
            return signature_;
        }

        /// <summary>
        /// Unblinds the signature asynchronous.
        /// </summary>
        /// <param name="blindedSignature">The blinded signature.</param>
        /// <param name="blindingFactor">The blinding factor.</param>
        /// <returns></returns>
        public static byte[] UnblindSignature(byte[] blindedSignature, byte[] blindingFactor)
        {
            using var context = new UnmanagedMemoryContext();

            context.Reference(blindedSignature, out var blindedSignature_);
            context.Reference(blindingFactor, out var blindingFactor_);

            Native.bbs_unblind_signature(blindedSignature_, blindingFactor_, out var unblindSignature, out var error);
            context.ThrowIfNeeded(error);

            context.Dereference(unblindSignature, out var unblindSignature_);

            return unblindSignature_;
        }

        /// <summary>
        /// Verifies the asynchronous.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <param name="messages">The messages.</param>
        /// <param name="signature">The signature.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static bool Verify(BbsKey publicKey, string[] messages, byte[] signature)
        {
            using var context = new UnmanagedMemoryContext();

            var handle = Native.bbs_verify_context_init(out var error);
            context.ThrowIfNeeded(error);

            context.Reference(publicKey.ToArray(), out var publicKey_);
            Native.bbs_verify_context_set_public_key(handle, publicKey_, out error);
            context.ThrowIfNeeded(error);

            context.Reference(signature, out var signature_);
            Native.bbs_verify_context_set_signature(handle, signature_, out error);
            context.ThrowIfNeeded(error);

            foreach (var message in messages)
            {
                Native.bbs_verify_context_add_message_string(handle, message, out error);
                context.ThrowIfNeeded(error);
            }

            var result = Native.bbs_verify_context_finish(handle, out error);
            context.ThrowIfNeeded(error);

            return result == 1;
        }

        /// <summary>
        /// Verifies the proof asynchronous.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <param name="proof">The proof.</param>
        /// <param name="revealedMessages">The indexed messages.</param>
        /// <param name="nonce">The nonce.</param>
        /// <returns></returns>
        public static SignatureProofStatus VerifyProof(BbsKey publicKey, byte[] proof, IndexedMessage[] revealedMessages, string nonce)
        {
            using var context = new UnmanagedMemoryContext();

            var handle = Native.bbs_verify_proof_context_init(out var error);
            context.ThrowIfNeeded(error);

            context.Reference(publicKey.ToArray(), out var publicKey_);
            Native.bbs_verify_proof_context_set_public_key(handle, publicKey_, out error);
            context.ThrowIfNeeded(error);

            Native.bbs_verify_proof_context_set_nonce_string(handle, nonce, out error);
            context.ThrowIfNeeded(error);

            context.Reference(proof, out var proof_);
            Native.bbs_verify_proof_context_set_proof(handle, proof_, out error);
            context.ThrowIfNeeded(error);

            foreach (var item in revealedMessages)
            {
                Native.bbs_verify_proof_context_add_message_string(handle, item.Message, out error);
                context.ThrowIfNeeded(error);

                Native.bbs_verify_proof_context_add_revealed_index(handle, item.Index, out error);
                context.ThrowIfNeeded(error);
            }

            var result = Native.bbs_verify_proof_context_finish(handle, out error);
            context.ThrowIfNeeded(error);

            return (SignatureProofStatus)result;
        }

        /// <summary>
        /// Verifies the blind commitment asynchronous.
        /// </summary>
        /// <param name="proof">The proof.</param>
        /// <param name="blindedIndices">The blinded indices.</param>
        /// <param name="publicKey">The public key.</param>
        /// <param name="nonce">The nonce.</param>
        /// <returns></returns>
        public static SignatureProofStatus VerifyBlindedCommitment(byte[] proof, uint[] blindedIndices, BbsKey publicKey, string nonce)
        {
            using var context = new UnmanagedMemoryContext();

            var handle = Native.bbs_verify_blind_commitment_context_init(out var error);
            context.ThrowIfNeeded(error);

            Native.bbs_verify_blind_commitment_context_set_nonce_string(handle, nonce, out error);
            context.ThrowIfNeeded(error);

            context.Reference(proof, out var proof_);
            Native.bbs_verify_blind_commitment_context_set_proof(handle, proof_, out error);
            context.ThrowIfNeeded(error);

            context.Reference(publicKey.ToArray(), out var publicKey_);
            Native.bbs_verify_blind_commitment_context_set_public_key(handle, publicKey_, out error);
            context.ThrowIfNeeded(error);

            foreach (var item in blindedIndices)
            {
                Native.bbs_verify_blind_commitment_context_add_blinded(handle, item, out error);
                context.ThrowIfNeeded(error);
            }

            var result = Native.bbs_verify_blind_commitment_context_finish(handle, out error);
            context.ThrowIfNeeded(error);

            return (SignatureProofStatus)result;
        }

        /// <summary>
        /// Blinds the commitment asynchronous.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <param name="nonce">The nonce.</param>
        /// <param name="messages">The messages.</param>
        /// <returns></returns>
        public static BlindedCommitment CreateBlindedCommitment(BbsKey publicKey, string nonce, IndexedMessage[] blindedMessages)
        {
            using var context = new UnmanagedMemoryContext();

            var handle = Native.bbs_blind_commitment_context_init(out var error);
            context.ThrowIfNeeded(error);

            foreach (var item in blindedMessages)
            {
                Native.bbs_blind_commitment_context_add_message_string(handle, item.Index, item.Message, out error);
                context.ThrowIfNeeded(error);
            }

            Native.bbs_blind_commitment_context_set_nonce_string(handle, nonce, out error);
            context.ThrowIfNeeded(error);

            context.Reference(publicKey.ToArray(), out var publicKey_);
            Native.bbs_blind_commitment_context_set_public_key(handle, publicKey_, out error);
            context.ThrowIfNeeded(error);

            Native.bbs_blind_commitment_context_finish(handle, out var commitment, out var outContext, out var blindingFactor, out error);
            context.ThrowIfNeeded(error);

            context.Dereference(commitment, out var _commitment);
            context.Dereference(outContext, out var _outContext);
            context.Dereference(blindingFactor, out var _blindingFactor);

            return new BlindedCommitment(_outContext, _blindingFactor, _commitment);
        }

        /// <summary>
        /// Blinds the sign asynchronous.
        /// </summary>
        /// <param name="keyPair">The signing key containing the secret BLS key.</param>
        /// <param name="commitment">The commitment.</param>
        /// <param name="messages">The messages.</param>
        /// <returns></returns>
        public static byte[] BlindSign(BlsKey keyPair, BbsKey publicKey, byte[] commitment, IndexedMessage[] messages)
        {
            using var context = new UnmanagedMemoryContext();

            var handle = Native.bbs_blind_sign_context_init(out var error);
            context.ThrowIfNeeded(error);

            foreach (var item in messages)
            {
                Native.bbs_blind_sign_context_add_message_string(handle, item.Index, item.Message, out error);
                context.ThrowIfNeeded(error);
            }

            context.Reference(publicKey.ToArray(), out var publicKey_);
            Native.bbs_blind_sign_context_set_public_key(handle, publicKey_, out error);
            context.ThrowIfNeeded(error);

            context.Reference(keyPair.SecretKey.ToArray(), out var secretKey_);
            Native.bbs_blind_sign_context_set_secret_key(handle, secretKey_, out error);
            context.ThrowIfNeeded(error);

            context.Reference(commitment, out var commitment_);
            Native.bbs_blind_sign_context_set_commitment(handle, commitment_, out error);
            context.ThrowIfNeeded(error);

            Native.bbs_blind_sign_context_finish(handle, out var blindedSignature, out error);
            context.ThrowIfNeeded(error);

            context.Dereference(blindedSignature, out var blindedSignature_);
            return blindedSignature_;
        }

        /// <summary>
        /// Creates the proof asynchronous.
        /// </summary>
        /// <param name="myKey">My key.</param>
        /// <param name="nonce">The nonce.</param>
        /// <param name="messages">The messages.</param>
        /// <returns></returns>
        public static byte[] CreateProof(BbsKey publicKey, ProofMessage[] proofMessages, byte[] blindingFactor, byte[] signature, string nonce)
        {
            using var context = new UnmanagedMemoryContext();

            var handle = Native.bbs_create_proof_context_init(out var error);
            context.ThrowIfNeeded(error);

            context.Reference(blindingFactor, out var blindingFactor_);
            foreach (var message in proofMessages)
            {
                Native.bbs_create_proof_context_add_proof_message_string(handle, message.Message, message.ProofType, blindingFactor_, out error);
                context.ThrowIfNeeded(error);
            }

            Native.bbs_create_proof_context_set_nonce_string(handle, nonce, out error);
            context.ThrowIfNeeded(error);

            context.Reference(publicKey.ToArray(), out var publicKey_);
            Native.bbs_create_proof_context_set_public_key(handle, publicKey_, out error);
            context.ThrowIfNeeded(error);

            context.Reference(signature, out var unblindedSignature_);
            Native.bbs_create_proof_context_set_signature(handle, unblindedSignature_, out error);
            context.ThrowIfNeeded(error);

            Native.bbs_create_proof_context_finish(handle, out var proof, out error);
            context.ThrowIfNeeded(error);

            context.Dereference(proof, out var proof_);

            return proof_;
        }


        /// <summary>
        /// Generates new <see cref="BlsKey"/> using a random seed.
        /// </summary>
        /// <returns></returns>
        public static BlsKey GenerateBlsKey() => GenerateBlsKey(Array.Empty<byte>());

        /// <summary>
        /// Generates new <see cref="BlsKey" /> using a input seed as string
        /// </summary>
        /// <param name="seed">The seed.</param>
        /// <returns></returns>
        public static BlsKey GenerateBlsKey(string seed) => GenerateBlsKey(Encoding.UTF8.GetBytes(seed ?? throw new Exception("Seed cannot be null")));

        /// <summary>
        /// Creates new <see cref="BlsKey"/> using a input seed as byte array.
        /// </summary>
        /// <param name="seed">The seed.</param>
        /// <returns></returns>
        public static BlsKey GenerateBlsKey(byte[] seed)
        {
            using var context = new UnmanagedMemoryContext();

            context.Reference(seed, out var seed_);
            var result = Native.bls_generate_key(seed_, out var pk, out var sk, out var error);
            context.ThrowIfNeeded(error);

            context.Dereference(pk, out var publicKey);
            context.Dereference(sk, out var secretKey);

            return new BlsKey(secretKey, publicKey);
        }
    }
}