using System;
using System.Collections.Generic;
using UnityEngine;

namespace Guiyuu.Gameloop {
    public class SfxController : MonoBehaviour {
        private static SfxController sc;
        public static SfxController inst => sc;
        
        
        public List<AudioClip> sfxClips;
        
        public List<AudioSource> sourcesPool;

        private Dictionary<string, AudioClip> sfxClipDict = new();
        private List<AudioSource> sourcesInUse = new();

        private void Start() {
            sc = this;
            foreach (AudioClip clip in sfxClips) sfxClipDict[clip.name] = clip;
        }

        private void Update() {
            foreach (AudioSource source in new List<AudioSource>(sourcesInUse)) {
                if (source.isPlaying) continue;
                sourcesInUse.Remove(source);
                sourcesPool.Add(source);
            }
        }

        public void play(string sfxName) {
            if (!sfxClipDict.ContainsKey(sfxName)) return;
            if (sourcesPool.Count == 0) return;
            
            AudioSource source = sourcesPool[0];
            
            sourcesPool.RemoveAt(0);
            sourcesInUse.Add(source);
            
            source.clip = sfxClipDict[sfxName];
            source.Play();
        }
    }
}