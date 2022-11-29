﻿namespace IdentityApi.Interfaces
{
    public interface IMessageManager
    {
        /// <summary>
        /// Sends a registration message
        /// </summary>
        /// <param name="to">Recipient of message</param>
        /// <param name="key">The key for verifying login</param>
        public void SendRegistrationMessage(string to, string key);

        /// <summary>
        /// Sends a login attempt message
        /// </summary>
        /// <param name="to">Recipient of message</param>
        /// <param name="otp">The otp for verifying login</param>
        public void SendLoginAttemptMessage(string to, string otp);
    }
}
