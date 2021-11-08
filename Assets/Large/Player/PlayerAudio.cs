using UnityEngine;
using Hertzole.GoldPlayer;

/// the player audio hooks
public class PlayerAudio : PlayerAudioBehaviour {
    // -- constants --
    float c_JumpInterval = 3.0f / 60.0f;

    // -- nodes --
    [Header("nodes")]
    [Tooltip("the footsteps music player")]
    [SerializeField] Musicker m_Footsteps;

    [Tooltip("the jump music player")]
    [SerializeField] Musicker m_Jump;

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

    /// the line to play when fluttering
    Line m_Flutter;

    /// the progress to play on jump
    Progression m_JumpProg;

    /// the index of the current step
    int m_StepIdx;

    /// the index of the melody note to play
    int m_MelodyIdx;

    /// the current step time
    float m_StepTime = 0.0f;

    /// the time of the next step
    float m_NextStepTime = 0.0f;

    /// the time to start fluttering
    float m_FlutterTime = 0.0f;

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

        m_JumpProg = new Progression(
            new Chord(
                Tone.V,
                Quality.Maj5
            ),
            new Chord(
                Tone.IV,
                Quality.Maj5
            )
        );

        m_Flutter = new Line(
            Tone.I.Octave(),
            Tone.II.Octave()
        );
    }

    void Update() {
        Step();
        Flutter();
        PlayFlutter();
    }

    // -- commands --
    // update current step progress
    void Step() {
        var c = m_Controller;
        var v = WalkVelocity;

        // pick melody note based on move dir
        var dirW = Vector3.Dot(Vector3.Normalize(v), transform.forward);
        m_MelodyIdx = dirW switch {
            var d when d > +0.8f => 0,
            var d when d > +0.3f => 1,
            var d when d > -0.3f => 2,
            var d when d > -0.8f => 3,
            _                    => 4,
        };

        // pick key based on look dir
        var dirL = Vector3.Dot(transform.forward, Vector3.forward);
        var root = dirL switch {
            var d when d > +0.8f => Root.C,
            var d when d > +0.3f => Root.G,
            var d when d > -0.3f => Root.D,
            var d when d > -0.8f => Root.A,
            _                    => Root.E,
        };

        if (m_Root != root) {
            m_Root = root;
            m_Key = new Key(m_Root);
        }

        // copy a bunch of stuff from gpc
        float dist = v.magnitude * Time.timeScale;
        float stride = 1.0f + dist * 0.3f;
        m_StepTime += (dist / stride) * (Time.deltaTime / c.Audio.StepTime);
    }

    /// flutter when airborne
    void Flutter() {
        var c = m_Controller.Controller;
        if (c.isGrounded) {
            m_FlutterTime = -1.0f;
            return;
        }

        if (m_FlutterTime == -1.0f) {
            m_FlutterTime = Time.time + 0.5f;
        }
    }

    /// play flutter audio
    void PlayFlutter() {
        if (m_FlutterTime == -1.0f) {
            return;
        }

        if (Time.time < m_FlutterTime) {
            return;
        }

        m_Footsteps.PlayLine(m_Flutter, m_Key);
        m_FlutterTime += 0.1f;
    }

    /// play step audio
    void PlayStep() {
        // if its time to play a step
        if (m_StepTime < m_NextStepTime) {
            return;
        }

        // find line to play
        if (m_StepIdx % 2 == 0) {
            m_Footsteps.PlayLine(m_FootstepsBass, m_Key);
        } else {
            var melody = m_FootstepsMelodies[m_MelodyIdx];
            m_Footsteps.PlayTone(melody[m_StepIdx / 2], m_Key);
        }

        // advance step
        m_StepIdx = (m_StepIdx + 1) % 4;
        m_NextStepTime += 0.5f;
    }

    /// play jump audio
    void PlayJump() {
        m_Jump.PlayProgression(
            m_JumpProg,
            c_JumpInterval,
            m_Key
        );
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
    }
}
