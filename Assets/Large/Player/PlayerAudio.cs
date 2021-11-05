using UnityEngine;
using Hertzole.GoldPlayer;

/// the player audio hooks
public class PlayerAudio : PlayerAudioBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the musical key")]
    [SerializeField] Root m_KeyOf = Root.C;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the music player")]
    [SerializeField] Musicker m_Music;

    // -- props --
    /// the musical key
    Key m_Key;

    /// the line to play when walking
    Line m_Line;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Key = new Key(m_KeyOf);

        m_Line = new Line(
            Tone.I,
            Tone.III,
            Tone.II,
            Tone.V,
            Tone.I.Octave(1)
        );
    }

    // -- PlayerAudioBehaviour --
    public override void PlayFootstepSound() {
        m_Music.PlayLine(m_Line, m_Key);
    }

    public override void PlayJumpSound() {
        Debug.Log($"play jump");
    }

    public override void PlayLandSound() {
        Debug.Log($"play land");
    }
}
