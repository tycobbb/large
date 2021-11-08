using UnityEngine;
using Hertzole.GoldPlayer;

/// the player audio hooks
public class PlayerAudio : PlayerAudioBehaviour {
    // -- nodes --
    [Header("nodes")]
    [Tooltip("the music player")]
    [SerializeField] Musicker m_Music;

    [Tooltip("the gold player controller")]
    [SerializeField] GoldPlayerController m_Controller;

    // -- props --
    /// the current key root
    Root m_Root = Root.C;

    /// the musical key
    Key m_Key;

    /// the bass line when walking
    Line m_FootstepsBass;

    /// the melody line when walking
    Line[] m_FootstepsMelodies;

    /// the chord to play on jump
    Chord m_JumpChord;

    /// the index of the current step
    int m_StepIdx;

    /// the index of the melody note to play
    int m_MelodyIdx;

    /// the current step time
    float m_StepTime = 0.0f;

    /// the time of the next step
    float m_NextStepTime = 0.0f;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Key = new Key(m_Root);

        m_FootstepsBass = new Line(
            Tone.I,
            Tone.V,
            Tone.III,
            Tone.II
        );

        m_FootstepsMelodies = new Line[5] {
            new Line(
                Tone.I.Octave(),
                Tone.V.Octave()
            ),
            new Line(
                Tone.III.Octave(),
                Tone.V.Octave()
            ),
            new Line(
                Tone.VII,
                Tone.V.Octave()
            ),
            new Line(
                Tone.VII.Flat(),
                Tone.V.Octave()
            ),
            new Line(
                Tone.VII.Flat(),
                Tone.III.Flat().Octave()
            ),
        };

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
        var v = WalkVelocity;

        // pick melody note based on move dir
        var dir = Vector3.Dot(Vector3.Normalize(v), transform.forward);
        Debug.Log($"dir {dir}");
        m_MelodyIdx = dir switch {
            var d when d > +0.8f => 0,
            var d when d > +0.3f => 1,
            var d when d > -0.3f => 2,
            var d when d > -0.8f => 3,
            _                    => 4,
        };

        // copy a bunch of stuff from gpc
        float dist = v.magnitude * Time.timeScale;
        float stride = 1.0f + dist * 0.3f;
        m_StepTime += (dist / stride) * (Time.deltaTime / c.Audio.StepTime);
    }

    /// play step audio
    void PlayStep() {
        // if its time to play a step
        if (m_StepTime < m_NextStepTime) {
            return;
        }

        // find line to play
        if (m_StepIdx % 2 == 0) {
            m_Music.PlayLine(m_FootstepsBass, m_Key);
        } else {
            var melody = m_FootstepsMelodies[m_MelodyIdx];
            m_Music.PlayTone(melody[m_StepIdx / 2], m_Key);
        }

        // advance step
        m_StepIdx = (m_StepIdx + 1) % 4;
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
