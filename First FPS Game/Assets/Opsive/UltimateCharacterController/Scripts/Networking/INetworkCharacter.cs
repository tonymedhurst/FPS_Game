using UnityEngine;
using Opsive.UltimateCharacterController.Character.Abilities;

namespace Opsive.UltimateCharacterController.Networking.Character
{
    /// <summary>
    /// Acts as a bridge between the character controller and the underlying networking implementation.
    /// </summary>
    public interface INetworkCharacter
    {
        /// <summary>
        /// Is the networking implementation server authoritative?
        /// </summary>
        /// <returns>True if the network transform is server authoritative.</returns>
        bool IsServerAuthoritative();

        /// <summary>
        /// Is the game instance on the server?
        /// </summary>
        /// <returns>True if the game instance is on the server.</returns>
        bool IsServer();

        /// <summary>
        /// Is the character the local player?
        /// </summary>
        /// <returns>True if the character is the local player.</returns>
        bool IsLocalPlayer();
    }
}