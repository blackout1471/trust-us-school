namespace IdentityApi.Interfaces
{
    public interface ILeakedPasswordProvider
    {
        /// <summary>
        /// Calls the underlying service to check whether the given password has been leaked.
        /// </summary>
        /// <param name="password">The password to check.</param>
        /// <returns>True if password has been leaked, false otherwise.</returns>
        Task<bool> GetIsPasswordLeakedAsync(string password);
    }
}
