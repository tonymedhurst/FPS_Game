﻿/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using UnityEngine;

namespace Opsive.UltimateCharacterController.Audio
{
    /// <summary>
    /// The AudioClipSet contains an array of AudioClips.
    /// </summary>
    [System.Serializable]
    public class AudioClipSet
    {
        [Tooltip("The delay before the AudioClip should be played.")]
        [SerializeField] protected float m_Delay;
        [Tooltip("An array of AudioClips which belong to the state.")]
        [SerializeField] protected AudioClip[] m_AudioClips;

        public float Delay { get { return m_Delay; } set { m_Delay = value; } }
        public AudioClip[] AudioClips { get { return m_AudioClips; } set { m_AudioClips = value; } }

        /// <summary>
        /// Plays the audio clip with a random set index.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <returns>The audio clip that was played.</returns>
        public AudioClip PlayAudioClip(GameObject gameObject)
        {
            return PlayAudioClip(gameObject, -1);
        }

        /// <summary>
        /// Plays the audio clip with a random set index.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <param name="loop">Does the clip loop?</param>
        /// <returns>The audio clip that was played.</returns>
        public AudioClip PlayAudioClip(GameObject gameObject, bool loop)
        {
            return PlayAudioClip(gameObject, -1, loop);
        }

        /// <summary>
        /// Plays the audio clip with a random set index.
        /// </summary>
        /// <param name="reservedIndex">The index of the component that should be played. -1 indicates any component.</param>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <returns>The audio clip that was played.</returns>
        public AudioClip PlayAudioClip(GameObject gameObject, int reservedIndex)
        {
            return PlayAudioClip(gameObject, reservedIndex, false);
        }

        /// <summary>
        /// Plays the audio clip with a random set index.
        /// </summary>
        /// <param name="reservedIndex">The index of the component that should be played. -1 indicates any component.</param>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        /// <returns>The audio clip that was played.</returns>
        public AudioClip PlayAudioClip(GameObject gameObject, int reservedIndex, bool loop)
        {
            var audioClip = GetAudioClip();
            if (audioClip == null) {
                return null;
            }

            AudioManager.Play(gameObject, audioClip, 1, loop, m_Delay, reservedIndex);
            return audioClip;
        }

        /// <summary>
        /// Returns the AudioClip that should be played.
        /// </summary>
        /// <returns>An AudioClip selected randomly out of the AudioClips array.</returns>
        private AudioClip GetAudioClip()
        {
            if (m_AudioClips == null || m_AudioClips.Length == 0) {
                return null;
            }

            return m_AudioClips[Random.Range(0, m_AudioClips.Length)];
        }

        /// <summary>
        /// Stops playing the audio on the specified GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to stop the audio on.</param>
        /// <param name="reservedIndex">The index of the component that should be stopped. -1 indicates all components.</param>
        public void Stop(GameObject gameObject, int reservedIndex)
        {
            AudioManager.Stop(gameObject, reservedIndex);
        }
    }
}