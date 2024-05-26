using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Guiyuu.Gameloop {
    public class MusicController : MonoBehaviour {
        public List<AudioSource> audios;

        private AudioSource currentPlayingAudio = null;

        private bool isPlaying {
            get {
                if (currentPlayingAudio == null) return false;
                return currentPlayingAudio.isPlaying;
            }
        }

        private const float cooldownTime = 5;
        private float accumulateTime = 0;
        private void Update() {
            if (isPlaying) return;
            accumulateTime += Time.deltaTime;
            
            if (accumulateTime < cooldownTime) return;
            accumulateTime = 0;
            
            AudioSource source = audios[Random.Range(0, audios.Count)];
            currentPlayingAudio = source;
            currentPlayingAudio.Play();
        }
    }
}