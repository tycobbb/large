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

    [Tooltip("the gold player controller")]
    [SerializeField] GoldPlayerController m_Controller;

    // -- props --
    /// the musical key
    Key m_Key;

    /// the line to play when walking
    Line m_ForwardsLine;

    /// the line to play when walking backwards
    Line m_BackwardsLine;

    /// the line to play when walking sideways
    Line m_SidewaysLine;

    /// the chord to play on jump
    Chord m_JumpChord;

    /// the index of the current step
    int m_StepIdx;

    /// the current step time
    float m_StepTime = 0.0f;

    /// the time of the next step
    float m_NextStepTime = 0.0f;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Key = new Key(m_KeyOf);

        m_ForwardsLine = new Line(
            Tone.I,
            Tone.V
        );

        m_BackwardsLine = new Line(
            Tone.I,
            Tone.III.Flat()
        );

        m_SidewaysLine = new Line(
            Tone.I,
            Tone.VII.Flat()
        );

        m_JumpChord = new Chord(
            Tone.V,
            Quality.Maj5
        );
    }

    void Update() {
        Step();
    }

    // -- commands --
    // update current step progress
    void Step() {
        var c = m_Controller;
        var v = m_Controller.Controller.velocity;

        // copy a bunch of stuff from gpc
        float velocity = WalkVelocity.magnitude * Time.timeScale;
        float stride = 1.0f + velocity * 0.3f;
        m_StepTime += (velocity / stride) * (Time.deltaTime / c.Audio.StepTime);
    }

    /// play step audio
    void PlayStep() {
        // if its time to play a step
        if (m_StepTime < m_NextStepTime) {
            return;
        }

        // find line to play
        var line = Vector3.Dot(WalkVelocity.normalized, Vector3.forward) switch {
            var d when d >= +0.3f => m_ForwardsLine,
            var d when d <= -0.3f => m_BackwardsLine,
            _                     => m_SidewaysLine,
        };

        // play that line
        m_Music.PlayTone(line[m_StepIdx], m_Key);

        // advance step
        m_StepIdx = (m_StepIdx + 1) % line.Length;
        m_NextStepTime += 0.5f;
    }

    /// play jump audio
    void PlayJump() {
        m_Music.PlayChord(m_JumpChord, 3.0f / 60.0f, m_Key);
    }

    // -- queries --
    /// the walking velocity
    Vector3 WalkVelocity {
        get {
            var v = m_Controller.Controller.velocity;
            v.y = 0.0f;
            return v;
        }
    }

    // -- PlayerAudioBehaviour --
    /// when the foosteps play
    public override void PlayFootstepSound() {
        PlayStep();
    }

    /// when the jump plays
    public override void PlayJumpSound() {
        PlayJump();
    }

    /// when the land plays
    public override void PlayLandSound() {
        Debug.Log($"play land");
    }
}
